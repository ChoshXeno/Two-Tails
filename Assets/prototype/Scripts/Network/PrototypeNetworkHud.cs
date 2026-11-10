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
        [SerializeField, Min(120), Tooltip("Minimum room button height in reference Canvas units.")]
        private float touchHeight = 120;
        private bool keyboardLayout;
        private bool layoutConnected;

        private static void Place(Component component, float x, float y, float width, float height, bool textWrapper = false)
        {
            var rect = (RectTransform)(textWrapper ? component.transform.parent : component.transform);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private void LateUpdate()
        {
            bool keyboard = joinCode != null && joinCode.isFocused;
            bool connected = online != null && online.IsConnected;
            if (keyboard != keyboardLayout || connected != layoutConnected)
                ApplyRoomLayout();
        }

        // Reuses the existing controls. Keeping the input and Join on the top row
        // leaves them above the landscape keyboard without moving gameplay controls.
        public void ApplyRoomLayout()
        {
            if (online == null || !useOnline)
                return;
            keyboardLayout = joinCode.isFocused;
            layoutConnected = online.IsConnected;
            float row = touchHeight + 12;
            var panel = (RectTransform)transform;
            panel.sizeDelta = new Vector2(1100, keyboardLayout ? touchHeight + 24 : layoutConnected ? row + 140 : row * 2 + 140);
            panel.pivot = new Vector2(0.5f, 1);
            panel.anchoredPosition = new Vector2(0, -12);
            var group = (RectTransform)onlineControls.transform;
            group.anchorMin = Vector2.zero; group.anchorMax = Vector2.one;
            group.offsetMin = group.offsetMax = Vector2.zero;
            Place(joinCode, -240, 12, 540, touchHeight);
            Place(joinRoom, 200, 12, 300, touchHeight);
            Place(createRoom, -365, row + 12, 330, touchHeight);
            Place(copyCode, 0, row + 12, 330, touchHeight);
            Place(leaveRoom, 365, row + 12, 330, touchHeight);
            Place(invite, -170, 12, 700, touchHeight, true);
            if (layoutConnected) { Place(invite, -365, 12, 330, touchHeight, true); Place(copyCode, 0, 12, 330, touchHeight); Place(leaveRoom, 365, 12, 330, touchHeight); }
            float footer = layoutConnected ? row + 12 : row * 2 + 12;
            Place(status, -150, footer, 760, 54, true);
            Place(players, -400, footer + 56, 240, 36, true);
            Place(identity, -60, footer + 56, 400, 36, true);
            Place(switchMode, 390, footer + 2, 240, touchHeight);
            title.gameObject.SetActive(false);
            foreach (Transform child in group)
            {
                var label = child.GetComponentInChildren<Text>();
                if (label != null && label.text == "Join Code" && child != joinCode.transform)
                    child.gameObject.SetActive(false);
            }
            joinCode.gameObject.SetActive(!layoutConnected);
            joinRoom.gameObject.SetActive(!layoutConnected);
            createRoom.gameObject.SetActive(!keyboardLayout && !layoutConnected);
            copyCode.gameObject.SetActive(!keyboardLayout);
            leaveRoom.gameObject.SetActive(!keyboardLayout);
            invite.transform.parent.gameObject.SetActive(layoutConnected && !keyboardLayout);
            status.transform.parent.gameObject.SetActive(!keyboardLayout);
            players.transform.parent.gameObject.SetActive(!keyboardLayout);
            identity.transform.parent.gameObject.SetActive(!keyboardLayout);
            switchMode.gameObject.SetActive(!keyboardLayout && !layoutConnected && (Application.isEditor || Debug.isDebugBuild));
        }

        private NetworkPlayerPrototype boundPlayer;
        private bool useOnline = true;
        private string copiedCode;

        private void OnEnable()
        {
            if (joinCode != null)
            {
                joinCode.inputType = InputField.InputType.Standard;
                joinCode.keyboardType = TouchScreenKeyboardType.ASCIICapable;
                joinCode.lineType = InputField.LineType.SingleLine;
            }
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
                switchMode.gameObject.SetActive(Application.isEditor || Debug.isDebugBuild);
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
            if (showOnline) ApplyRoomLayout();
            else
            {
                var panel = (RectTransform)transform;
                panel.sizeDelta = new Vector2(800, 250);
                panel.pivot = new Vector2(0.5f, 1);
                panel.anchoredPosition = new Vector2(0, -8);
                title.gameObject.SetActive(true);
                status.transform.parent.gameObject.SetActive(true);
                players.transform.parent.gameObject.SetActive(true);
                Place(status, 0, 167, 760, 38, true);
                Place(players, 0, 205, 760, 38, true);
                Place(switchMode, 269, 9, 210, 44);
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
