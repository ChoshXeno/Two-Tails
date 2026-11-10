using UnityEngine;

namespace TwoTails.Prototype
{
    public sealed class PrototypeNetworkPuzzleHud : MonoBehaviour
    {
        [SerializeField] private PrototypeNetworkSession session;
        [SerializeField] private UnityEngine.UI.Button interact;
        [SerializeField] private UnityEngine.UI.Text prompt;
        [SerializeField] private PrototypeNetworkPlateGate gate;
        [SerializeField] private UnityEngine.UI.Text plateStatus;
        [SerializeField, Tooltip("Jump action touch control, visible only with a local player.")] private GameObject jump;
        private PrototypeNetworkInteraction interaction;
        private NetworkPlayerPrototype current;
        private float nextRefresh;

        private void OnEnable() => interact.onClick.AddListener(Press);
        private void OnDisable() => interact.onClick.RemoveListener(Press);

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh)
                return;
            nextRefresh = Time.unscaledTime + 0.1f;
            if (current != session.LocalPlayer)
            {
                current = session.LocalPlayer;
                interaction = current != null ? current.GetComponent<PrototypeNetworkInteraction>() : null;
            }

            interact.gameObject.SetActive(interaction != null);
            if(jump!=null)jump.SetActive(interaction!=null);
            prompt.text = interaction != null ? interaction.GetPrompt() : "";
            if (plateStatus != null)
                plateStatus.text = interaction != null ? gate.GetStatus() : "";
        }

        private void Press()
        {
            if (interaction != null)
                interaction.Request();
        }
    }
}
