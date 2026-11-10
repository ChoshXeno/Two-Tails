using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace TwoTails.Prototype
{
    [DefaultExecutionOrder(-90)]
    [RequireComponent(typeof(PrototypeNetworkSession))]
    public sealed class PrototypeOnlineRoom : MonoBehaviour
    {
        [SerializeField, Tooltip("Unity Gaming Services environment shared by both players.")]
        private string environmentName = "production";
        [SerializeField, Min(5), Tooltip("Seconds allowed for sign-in, the room request, and the NGO connection.")]
        private float operationTimeout = 30;

        private PrototypeNetworkSession network;
        private UnityTransport transport;
        private ISession room;
        private CancellationTokenSource opening;
        private bool busy;
        private bool createdRoom;
        private bool membershipEnded;
        private string interrupted;
        private double retryAfter;

        public string Status { get; private set; } = "Ready";
        public string JoinCode => IsConnected && createdRoom ? room.Code : "";
        public string ShortPlayerId { get; private set; } = "";
        public bool IsConnected => room != null && !busy && network.LocalConnected;
        public bool CanOpen => !busy && room == null && network.CanStart && Time.realtimeSinceStartupAsDouble >= retryAfter;
        public bool CanLeave => IsConnected;
        public bool CanCancel => opening != null && !opening.IsCancellationRequested;
        public bool CanSwitchMode => !busy && room == null && network.NetworkIdle;
        public event Action Changed;

        private void Awake()
        {
            network = GetComponent<PrototypeNetworkSession>();
            transport = GetComponent<UnityTransport>();
            if (!Configured)
                Status = "Services are not configured";
        }

        private void OnEnable() => network.OnlineConnectionLost += ConnectionLost;

        private void OnApplicationPause(bool paused)
        {
            if (!paused && room != null && !busy &&
                (room.State != SessionState.Connected || !network.LocalConnected))
                ConnectionLost("Connection lost while the app was in the background");
        }

        private void OnDisable()
        {
            network.OnlineConnectionLost -= ConnectionLost;
            if (opening != null)
                Interrupt("Cancelled");
            else if (room != null && !busy)
                _ = CloseAsync("Ready");
        }

        private bool Configured => !string.IsNullOrWhiteSpace(Application.cloudProjectId) &&
            !string.IsNullOrWhiteSpace(environmentName);

        public Task CreateAsync() => OpenAsync(true, null);
        public Task JoinAsync(string code) => OpenAsync(false, code?.Trim());
        public Task LeaveAsync() => CanLeave ? CloseAsync("Ready") : Task.CompletedTask;

        public void Cancel()
        {
            if (CanCancel)
                Interrupt("Cancelled");
        }

        private async Task OpenAsync(bool create, string code)
        {
            if (!CanOpen)
                return;
            if (!Configured)
            {
                Publish("Services are not configured");
                return;
            }
            if (!create && string.IsNullOrEmpty(code))
            {
                Publish("Enter a join code");
                return;
            }
            if (!network.ReserveOnline())
                return;

            busy = true;
            createdRoom = create;
            membershipEnded = false;
            interrupted = null;
            string result = null;
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(operationTimeout));
            opening = timeout;
            try
            {
                Publish("Initializing");
                await UntilCancelled(PrototypeOnlineIdentity.InitializeAsync(environmentName, PrototypeOnlineIdentity.Profile), timeout.Token);
                Publish("Signing In");
                await UntilCancelled(PrototypeOnlineIdentity.SignInAsync(), timeout.Token);
                ShortPlayerId = PrototypeOnlineIdentity.ShortPlayerId;

                Publish(create ? "Creating" : "Joining");
                var service = MultiplayerService.Instance;
                room = create
                    ? await service.CreateSessionAsync(new SessionOptions { MaxPlayers = 2, IsPrivate = true }
                        .WithRelayNetwork().WithNetworkOptions(new NetworkOptions { RelayProtocol = RelayProtocol.DTLS }), timeout.Token)
                    : await service.JoinSessionByCodeAsync(code, new JoinSessionOptions()
                        .WithNetworkOptions(new NetworkOptions { RelayProtocol = RelayProtocol.DTLS }), timeout.Token);

                room.Deleted += MembershipEnded;
                room.RemovedFromSession += MembershipEnded;
                room.SessionHostChanged += HostChanged;
                if (room.State != SessionState.Connected)
                    MembershipEnded();

                timeout.Token.ThrowIfCancellationRequested();
                Publish("Connecting");
                while (!network.LocalConnected)
                    await Task.Delay(40, timeout.Token);

                if (transport.Protocol != UnityTransport.ProtocolType.RelayUnityTransport)
                    throw new InvalidOperationException("The active transport is not configured for Relay.");

                busy = false;
            }
            catch (Exception error)
            {
                result = interrupted ?? (timeout.IsCancellationRequested ? "Connection timed out" : Describe(error, !create));
                if (error is not OperationCanceledException)
                    LogError(Status, error);
            }
            finally
            {
                opening = null;
            }

            if (result != null)
                await CloseAsync(result);
            else
                Publish("Connected");
        }

        private void ConnectionLost(string message)
        {
            if (opening != null)
                Interrupt(message);
            else if (room != null && !busy)
                _ = CloseAsync(message);
        }

        private void MembershipEnded()
        {
            membershipEnded = true;
            ConnectionLost(createdRoom ? "Room closed" : "Host left the room");
        }

        private void HostChanged(string _) => ConnectionLost("Host left the room");

        private void Interrupt(string message)
        {
            if (opening == null || opening.IsCancellationRequested)
                return;
            interrupted = message;
            Publish("Leaving");
            opening.Cancel();
        }

        private async Task CloseAsync(string result)
        {
            busy = true;
            Publish("Leaving");
            ISession endingRoom = room;
            room = null;
            if (endingRoom != null)
            {
                endingRoom.Deleted -= MembershipEnded;
                endingRoom.RemovedFromSession -= MembershipEnded;
                endingRoom.SessionHostChanged -= HostChanged;
                try
                {
                    if (!membershipEnded && endingRoom.State != SessionState.Deleted)
                    {
                        if (createdRoom && endingRoom.IsHost)
                            await endingRoom.AsHost().DeleteAsync();
                        else
                            await endingRoom.LeaveAsync();
                    }
                }
                catch (SessionException error) when (error.Error is SessionError.SessionNotFound or SessionError.SessionDeleted or SessionError.NotInLobby)
                {
                    // The service already removed the room or the local membership.
                }
                catch (Exception error)
                {
                    LogError("Leaving", error);
                    result = "Disconnected. Unable to confirm room cleanup";
                }
            }

            if (network == null)
                return;
            network.ShutdownOnline();
            while (network != null && !network.NetworkIdle && !Application.exitCancellationToken.IsCancellationRequested)
                await Task.Yield();
            if (network == null)
                return;
            busy = false;
            network.ReleaseOnline();
            Publish(result);
        }

        private string Describe(Exception error, bool joining)
        {
            if (error is RequestFailedException request)
            {
                if (request.ErrorCode is CommonErrorCodes.ApiMissing or CommonErrorCodes.ProjectPolicyAccessDenied or CommonErrorCodes.Forbidden or CommonErrorCodes.NotFound)
                    return "Services are not configured";
                if (request.ErrorCode == CommonErrorCodes.Timeout)
                    return "Connection timed out";
                if (request.ErrorCode == CommonErrorCodes.TooManyRequests)
                    return RateLimited();
            }
            if (error is SessionException session)
            {
                if (session.Error == SessionError.RateLimitExceeded)
                    return RateLimited();
                if (joining && session.Error is SessionError.SessionNotFound or SessionError.SessionDeleted or SessionError.InvalidParameter)
                    return "Invalid or expired code";
                // The 2.4.1 Sessions adapter maps these backend errors to Unknown and retains only the description.
                if (session.Error == SessionError.Unknown && joining)
                {
                    if (session.Message.Contains("is full", StringComparison.OrdinalIgnoreCase))
                        return "Room is full";
                    if (session.Message.Contains("invalid join code", StringComparison.OrdinalIgnoreCase) ||
                        (session.Message.StartsWith("session code ", StringComparison.OrdinalIgnoreCase) &&
                         session.Message.Contains("invalid character", StringComparison.OrdinalIgnoreCase)))
                        return "Invalid or expired code";
                }
            }
            if (error is ServicesInitializationException)
                return "Services could not initialize";
            if (Status == "Signing In")
                return "Unable to sign in";
            return "Unable to connect";
        }

        private string RateLimited()
        {
            retryAfter = Time.realtimeSinceStartupAsDouble + 3;
            return "Too many requests. Try again shortly";
        }

        private void Update()
        {
            if (opening?.IsCancellationRequested == true && Status != "Leaving")
                Publish("Leaving");
            else if (opening != null && (Status == "Creating" || Status == "Joining") && !network.NetworkIdle)
                Publish("Connecting");
            if (retryAfter > 0 && Time.realtimeSinceStartupAsDouble >= retryAfter)
            {
                retryAfter = 0;
                Changed?.Invoke();
            }
        }

        private static async Task UntilCancelled(Task task, CancellationToken token)
        {
            using var signal = CancellationTokenSource.CreateLinkedTokenSource(token);
            if (await Task.WhenAny(task, Task.Delay(Timeout.Infinite, signal.Token)) != task)
            {
                _ = task.ContinueWith(faulted => { _ = faulted.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
                token.ThrowIfCancellationRequested();
            }
            signal.Cancel();
            await task;
            token.ThrowIfCancellationRequested();
        }

        private static void LogError(string step, Exception error)
        {
            string detail = error is SessionException session ? session.Error.ToString() :
                error is RequestFailedException request ? request.ErrorCode.ToString() : error.GetType().Name;
            Debug.LogWarning($"Two Tails online ({step}): {detail}");
        }

        private void Publish(string status)
        {
            Status = status;
            Changed?.Invoke();
        }
    }
}
