using UnityEngine;
using UnityEngine.EventSystems;

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
        private NetworkPlayerPrototype boundPlayer;

        private void OnEnable()
        {
            session.Changed += Refresh;
            host.onClick.AddListener(StartHost);
            client.onClick.AddListener(StartClient);
            disconnect.onClick.AddListener(session.Disconnect);
            Refresh();
        }

        private void OnDisable()
        {
            session.Changed -= Refresh;
            host.onClick.RemoveListener(StartHost);
            client.onClick.RemoveListener(StartClient);
            disconnect.onClick.RemoveListener(session.Disconnect);
            Bind(null);
        }

        private void StartHost() => StartSession(true);
        private void StartClient() => StartSession(false);

        private void StartSession(bool asHost)
        {
            EventSystem.current?.SetSelectedGameObject(null);
            session.StartSession(asHost, address.text, port.text);
        }

        private void Refresh()
        {
            host.interactable = client.interactable = address.interactable = port.interactable = session.CanStart;
            disconnect.interactable = session.CanDisconnect;
            status.text = "Status: " + session.Status;
            players.text = $"Players: {session.PlayerCount}/2";
            if (boundPlayer != session.LocalPlayer || (orbit != null && orbit.enabled != (session.LocalPlayer != null)))
                Bind(session.LocalPlayer);
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
