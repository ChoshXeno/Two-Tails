using UnityEngine;
using UnityEngine.InputSystem;

namespace TwoTails.Prototype
{
    [DefaultExecutionOrder(-10)]
    [RequireComponent(typeof(PrototypeMotor))]
    public sealed class PrototypeLocalInput : MonoBehaviour
    {
        [SerializeField, Tooltip("Input Actions asset shared by the local prototype characters.")] private InputActionAsset actions;
        [SerializeField, Tooltip("This character's action map. Use Gameplay for P1 or DebugPlayer2 for P2.")] private string actionMap = "Gameplay";
        [SerializeField, Tooltip("Receives the single interaction request emitted by a button press.")] private PrototypeInteractor interactor;

        private InputActionAsset instance;
        private InputActionMap gameplay;
        private InputAction move;
        private InputAction interact;
        private PrototypeMotor motor;
        private bool hasFocus;
        private bool waitForNeutral;

        private void Awake()
        {
            motor = GetComponent<PrototypeMotor>();
            instance = Instantiate(actions);
            gameplay = instance.FindActionMap(actionMap, true);
            move = gameplay.FindAction("Move", true);
            interact = gameplay.FindAction("Interact", true);
        }

        private void OnEnable()
        {
            hasFocus = Application.isFocused;
            interact.performed += OnInteract;
            gameplay.Enable();
        }

        private void OnDisable()
        {
            interact.performed -= OnInteract;
            gameplay.Disable();
            motor.SetMoveInput(Vector2.zero);
        }

        private void OnDestroy()
        {
            Destroy(instance);
        }

        private void OnApplicationFocus(bool focused)
        {
            hasFocus = focused;
            if (focused)
                return;

            waitForNeutral = true;
            motor.SetMoveInput(Vector2.zero);
        }

        private void Update()
        {
            if (!hasFocus)
                return;

            Vector2 input = move.ReadValue<Vector2>();
            if (waitForNeutral)
            {
                waitForNeutral = input.sqrMagnitude > 0.0001f;
                return;
            }

            motor.SetMoveInput(input);
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            if (hasFocus && interactor != null)
                interactor.TryInteract();
        }
    }
}
