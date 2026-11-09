using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TwoTails.Prototype
{
    public sealed class PrototypeNetworkHud : MonoBehaviour
    {
        [SerializeField] private PrototypeNetworkSession session;
        [SerializeField] private PrototypeFollowCamera followCamera;
        [SerializeField] private PrototypeOrbitDrag orbit;
        [SerializeField] private GameObject stick;
        [SerializeField] private UnityEngine.UI.InputField address;
        [SerializeField] private UnityEngine.UI.InputField port;
        [SerializeField] private UnityEngine.UI.Button host;
        [SerializeField] private UnityEngine.UI.Button client;
        [SerializeField] private UnityEngine.UI.Button disconnect;
        [SerializeField] private UnityEngine.UI.Text status;
        [SerializeField] private UnityEngine.UI.Text players;
        [SerializeField] private PrototypeOnlineRoom online;
        [SerializeField] private GameObject directControls;
        [SerializeField] private GameObject onlineControls;
        [SerializeField] private Button switchMode;
        [SerializeField] private Text modeLabel;
        [SerializeField] private Text title;
        [SerializeField] private InputField joinCode;
        [SerializeField] private Button createRoom;
        [SerializeField] private Button joinRoom;
        [SerializeField] private Button copyCode;
        [SerializeField] private Button leaveRoom;
        [SerializeField] private Text leaveLabel;
        [SerializeField] private Text invite;
        [SerializeField] private Text identity;
        private NetworkPlayerPrototype boundPlayer;
        private bool useOnline = true;
        private string copiedCode;

        private void OnEnable()
        {
            session.Changed += Refresh;
            host.onClick.AddListener(StartHost);
            client.onClick.AddListener(StartClient);
            disconnect.onClick.AddListener(session.Disconnect);
            if (online != null)
            {
                online.Changed += Refresh;
                switchMode.onClick.AddListener(SwitchMode);
                createRoom.onClick.AddListener(CreateRoom);
                joinRoom.onClick.AddListener(JoinRoom);
                copyCode.onClick.AddListener(CopyCode);
                leaveRoom.onClick.AddListener(LeaveRoom);
            }
            Refresh();
        }

        private void OnDisable()
        {
            session.Changed -= Refresh;
            host.onClick.RemoveListener(StartHost);
            client.onClick.RemoveListener(StartClient);
            disconnect.onClick.RemoveListener(session.Disconnect);
            if (online != null)
            {
                online.Changed -= Refresh;
                switchMode.onClick.RemoveListener(SwitchMode);
                createRoom.onClick.RemoveListener(CreateRoom);
                joinRoom.onClick.RemoveListener(JoinRoom);
                copyCode.onClick.RemoveListener(CopyCode);
                leaveRoom.onClick.RemoveListener(LeaveRoom);
            }
            Bind(null);
        }

        private void StartHost() => StartSession(true);
        private void StartClient() => StartSession(false);

        private void StartSession(bool asHost)
        {
            EventSystem.current?.SetSelectedGameObject(null);
            session.StartSession(asHost, address.text, port.text);
        }

        private void SwitchMode()
        {
            if (!online.CanSwitchMode)
                return;
            EventSystem.current?.SetSelectedGameObject(null);
            useOnline = !useOnline;
            Refresh();
        }

        private void CreateRoom()
        {
            EventSystem.current?.SetSelectedGameObject(null);
            _ = online.CreateAsync();
        }

        private void JoinRoom()
        {
            EventSystem.current?.SetSelectedGameObject(null);
            _ = online.JoinAsync(joinCode.text);
        }

        private void CopyCode()
        {
            if (string.IsNullOrEmpty(online.JoinCode))
                return;
            GUIUtility.systemCopyBuffer = copiedCode = online.JoinCode;
            Refresh();
        }

        private void LeaveRoom()
        {
            if (online.CanCancel)
                online.Cancel();
            else
                _ = online.LeaveAsync();
        }

        private void Refresh()
        {
            host.interactable = client.interactable = address.interactable = port.interactable = session.CanStart;
            disconnect.interactable = session.CanDisconnect;
            bool showOnline = online != null && useOnline;
            status.text = "Status: " + (showOnline ? online.Status : session.Status);
            players.text = $"Players: {session.PlayerCount}/2";
            if (online != null)
            {
                onlineControls.SetActive(showOnline);
                directControls.SetActive(!showOnline);
                switchMode.interactable = online.CanSwitchMode;
                modeLabel.text = showOnline ? "Direct Test" : "Online Relay";
                title.text = showOnline ? "Online Relay" : "Direct Connection Test";
                createRoom.interactable = joinRoom.interactable = joinCode.interactable = online.CanOpen;
                leaveRoom.interactable = online.CanLeave || online.CanCancel;
                leaveLabel.text = online.CanCancel ? "Cancel" : "Leave Room";
                string code = online.JoinCode;
                copyCode.interactable = !string.IsNullOrEmpty(code);
                if (string.IsNullOrEmpty(code))
                    copiedCode = null;
                invite.text = copyCode.interactable ? $"Room Code: {code}" + (copiedCode == code ? " (Copied)" : "") : "";
                identity.text = Debug.isDebugBuild && !string.IsNullOrEmpty(online.ShortPlayerId) ?
                    $"Player ID: {online.ShortPlayerId}" : "";
            }
            NetworkPlayerPrototype player = showOnline && !online.IsConnected ? null : session.LocalPlayer;
            if (boundPlayer != player || (orbit != null && orbit.enabled != (player != null)))
                Bind(player);
        }

        private void Bind(NetworkPlayerPrototype player)
        {
            boundPlayer = player;
            bool local = player != null;
            if (orbit != null)
                orbit.enabled = local;
            if (stick != null)
                stick.SetActive(local);
            if (followCamera != null)
                followCamera.Bind(local ? player.transform : null, local ? player.Motor.Facing : null);
        }
    }
}
