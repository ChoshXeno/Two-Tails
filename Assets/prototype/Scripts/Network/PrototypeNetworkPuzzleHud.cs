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
        private PrototypeScreenButton ropeUp;
        private PrototypeMotor touchGrip;
        private int? touchPointer;
        private bool gripConsumedClick;
        private void Awake()
        {
            interactLabel = interact.GetComponentInChildren<UnityEngine.UI.Text>(true);
            ropeUp = interact.gameObject.AddComponent<PrototypeScreenButton>();
            ropeUp.enabled = false;
            ropeUp.controlPath = "<Gamepad>/buttonSouth";
            var trigger = interact.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            var down = new UnityEngine.EventSystems.EventTrigger.Entry
                { eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown };
            down.callback.AddListener(data =>
            {
                if (touchPointer.HasValue) return;
                gripConsumedClick = current != null && current.Motor.TryGrip();
                if (!gripConsumedClick) return;
                touchGrip = current.Motor;
                touchPointer = ((UnityEngine.EventSystems.PointerEventData)data).pointerId;
            });
            var up = new UnityEngine.EventSystems.EventTrigger.Entry
                { eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp };
            up.callback.AddListener(data =>
            {
                if (touchPointer == ((UnityEngine.EventSystems.PointerEventData)data).pointerId) ReleaseTouchGrip();
            });
            trigger.triggers.Add(down);
            trigger.triggers.Add(up);
        }

        private void ReleaseTouchGrip()
        {
            if (touchGrip != null) touchGrip.ReleaseGrip();
            touchGrip = null;
            touchPointer = null;
        }

        private void OnEnable() => interact.onClick.AddListener(Press);
        private void OnDisable()
        {
            interact.onClick.RemoveListener(Press);
            ClearPlayer();
        }

        private void ClearPlayer()
        {
            ReleaseTouchGrip();
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

            bool onRope = current != null && current.Motor.IsOnRope;
            ropeUp.enabled = onRope;
            interact.interactable = !onRope;
            interact.gameObject.SetActive(interaction != null);
            if (interactLabel != null) interactLabel.text = onRope ? "Up" : actor != null && actor.HasStrikeTarget ? "Strike" : actor != null && actor.HasPickupTarget ? "Pickup" : "Interact";
            if(jump!=null)jump.SetActive(interaction!=null);
            prompt.text = interaction != null ? interaction.GetPrompt() : "";
            if (plateStatus != null)
                plateStatus.text = interaction != null ? gate.GetStatus() : "";
        }

        private void Press()
        {
            if (gripConsumedClick) { gripConsumedClick = false; return; }
            if (interaction != null && current != null && !current.Motor.IsOnRope)
                interaction.Request();
        }
    }
}
