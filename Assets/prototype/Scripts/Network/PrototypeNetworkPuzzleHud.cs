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
        private UnityEngine.UI.Text interactLabel;
        private AnimalMechanicsActor actor;
        private void Awake() => interactLabel = interact.GetComponentInChildren<UnityEngine.UI.Text>(true);

        private void OnEnable() => interact.onClick.AddListener(Press);
        private void OnDisable()
        {
            interact.onClick.RemoveListener(Press);
            ClearPlayer();
        }

        private void ClearPlayer()
        {
            current = null;
            interaction = null;
            actor = null;
        }

        private void Update()
        {
            var local = session.LocalPlayer;
            if (local == null || !local.IsSpawned)
                ClearPlayer();
            else if (current != local)
            {
                current = local;
                interaction = current != null ? current.GetComponent<PrototypeNetworkInteraction>() : null;
                actor = current != null ? current.GetComponent<AnimalMechanicsActor>() : null;
            }

            interact.gameObject.SetActive(interaction != null);
            if (interactLabel != null) interactLabel.text = actor != null && actor.HasStrikeTarget ? "Strike" : actor != null && actor.HasPickupTarget ? "Pickup" : "Interact";
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
