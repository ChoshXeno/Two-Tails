using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace TwoTails.Prototype
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(NetworkManager), typeof(UnityTransport))]
    public sealed class PrototypeNetworkSession : MonoBehaviour
    {
        private const string Full = "Session Full";
        [SerializeField, Tooltip("The two server-assigned spawn locations, in slot order.")]
        private Transform[] spawnPoints;
        [SerializeField, Min(1f), Tooltip("Maximum seconds to wait for a confirmed connection.")]
        private float connectionTimeout = 8f;

        private NetworkManager manager;
        private UnityTransport transport;
        private readonly ulong?[] slots = new ulong?[2];
        private readonly HashSet<NetworkPlayerPrototype> players = new();
        private bool starting;
        private bool stopping;
        private bool connected;
        private bool onlineReserved;
        private bool previousRunInBackground;
        private float deadline;

        public string Status { get; private set; } = "Offline";
        public bool NetworkIdle => !starting && !stopping && !manager.IsListening && !manager.ShutdownInProgress;
        public bool CanStart => !onlineReserved && NetworkIdle;
        public bool CanDisconnect => !onlineReserved && !stopping && (starting || manager.IsListening);
        public bool LocalConnected => connected && LocalPlayer != null && manager.IsConnectedClient;
        public int PlayerCount => players.Count;
        public NetworkPlayerPrototype LocalPlayer { get; private set; }
        public event Action Changed;
        public event Action<string> OnlineConnectionLost;

        private void Awake()
        {
            manager = GetComponent<NetworkManager>();
            transport = GetComponent<UnityTransport>();
            previousRunInBackground = Application.runInBackground;
            Application.runInBackground = true;
            if (!Application.isEditor)
                Screen.fullScreenMode = FullScreenMode.Windowed;
        }

        private void OnEnable()
        {
            manager.ConnectionApprovalCallback += Approve;
            manager.OnClientConnectedCallback += Connected;
            manager.OnClientDisconnectCallback += Disconnected;
            manager.OnTransportFailure += TransportFailed;
        }

        private void OnDisable()
        {
            manager.ConnectionApprovalCallback -= Approve;
            manager.OnClientConnectedCallback -= Connected;
            manager.OnClientDisconnectCallback -= Disconnected;
            manager.OnTransportFailure -= TransportFailed;
            if (manager.IsListening && !manager.ShutdownInProgress)
                manager.Shutdown();
        }

        private void OnDestroy() => Application.runInBackground = previousRunInBackground;

        public void StartSession(bool host, string address, string portText)
        {
            if (!CanStart)
                return;

            if (!IPAddress.TryParse(address.Trim(), out IPAddress ip) ||
                ip.AddressFamily != AddressFamily.InterNetwork ||
                !ushort.TryParse(portText, out ushort port) || port == 0)
            {
                SetStatus("Connection Failed: Enter an IPv4 address and port (1–65535).");
                return;
            }

            Array.Clear(slots, 0, slots.Length);
            transport.SetConnectionData(ip.ToString(), port, host ? ip.ToString() : null);
            starting = true;
            deadline = Time.realtimeSinceStartup + connectionTimeout;
            SetStatus(host ? "Starting Host" : "Connecting");
            bool started = host ? manager.StartHost() : manager.StartClient();
            if (!started && !stopping)
                Stop("Connection Failed: Could not start the transport.");
        }

        public void Disconnect()
        {
            if (CanDisconnect)
                Stop("Disconnected");
        }

        internal bool ReserveOnline()
        {
            if (!CanStart)
                return false;
            ResetPlayers();
            onlineReserved = true;
            return true;
        }

        internal void ShutdownOnline()
        {
            if (onlineReserved && manager.IsListening && !manager.ShutdownInProgress)
                manager.Shutdown();
        }

        internal void ReleaseOnline()
        {
            ResetPlayers();
            onlineReserved = false;
            Changed?.Invoke();
        }

        private void OnApplicationPause(bool paused)
        {
            // Desktop focus changes never disconnect. Resume only cleans up a confirmed
            // broken Direct session; transport timeout continues to detect stale peers.
            if (!paused && !onlineReserved && connected && !manager.IsConnectedClient && !stopping)
                Stop("Disconnected: Connection lost while the app was in the background.");
        }

        private void Update()
        {
            if (starting && Time.realtimeSinceStartup >= deadline)
                Stop("Connection Failed: Timed out.");

            if (!stopping || manager.IsListening || manager.ShutdownInProgress)
                return;

            stopping = false;
            ResetPlayers();
            Changed?.Invoke();
        }

        private void ResetPlayers()
        {
            connected = false;
            Array.Clear(slots, 0, slots.Length);
            players.Clear();
            LocalPlayer = null;
        }

        private void Approve(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            // Reserve before returning approval so a second request cannot claim the same slot.
            int slot = Array.FindIndex(slots, id => id == null);
            if (slot < 0 || stopping)
            {
                response.Approved = false;
                response.Reason = stopping ? "Session ended" : Full;
                return;
            }

            slots[slot] = request.ClientNetworkId;
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Position = spawnPoints[slot].position;
            response.Rotation = spawnPoints[slot].rotation;
        }

        public byte SlotFor(ulong clientId) => (byte)Array.IndexOf(slots, (ulong?)clientId);

        private void Connected(ulong id)
        {
            if (id != manager.LocalClientId || stopping)
                return;

            starting = false;
            connected = true;
            if (onlineReserved)
                Changed?.Invoke();
            else
                SetStatus("Connected");
        }

        private void Disconnected(ulong id)
        {
            int slot = Array.IndexOf(slots, (ulong?)id);
            if (slot >= 0)
                slots[slot] = null;
            if (stopping || id != manager.LocalClientId)
                return;

            string reason = manager.DisconnectReason;
            if (onlineReserved)
            {
                bool wasConnected = connected;
                connected = false;
                OnlineConnectionLost?.Invoke(reason?.Contains(Full) == true ? "Room is full" :
                    wasConnected && !manager.IsHost ? "Host left the room" : "Unable to connect");
                return;
            }
            Stop(!string.IsNullOrEmpty(reason) && reason.Contains(Full) ? Full :
                connected ? "Disconnected" : "Connection Failed: Could not reach the host.");
        }

        private void TransportFailed()
        {
            if (onlineReserved)
            {
                OnlineConnectionLost?.Invoke("Unable to connect");
                return;
            }
            if (!stopping)
                Stop("Connection Failed: Transport error.");
        }

        private void Stop(string message)
        {
            starting = connected = false;
            stopping = true;
            SetStatus(message);
            if (!manager.ShutdownInProgress)
                manager.Shutdown();
        }

        private void SetStatus(string status)
        {
            Status = status;
            Changed?.Invoke();
        }

        internal void Register(NetworkPlayerPrototype player, bool local)
        {
            players.Add(player);
            if (local)
                LocalPlayer = player;
            Changed?.Invoke();
        }

        internal void Unregister(NetworkPlayerPrototype player)
        {
            players.Remove(player);
            if (LocalPlayer == player)
                LocalPlayer = null;
            Changed?.Invoke();
        }
    }
}
