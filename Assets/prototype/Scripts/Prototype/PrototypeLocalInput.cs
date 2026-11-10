using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
        private InputAction jump;
        private PrototypeMotor motor;
        private bool hasFocus;
        private bool paused;
        private bool waitForNeutral;
        private static bool EditingText => EventSystem.current?.currentSelectedGameObject?
            .GetComponent<InputField>()?.isFocused == true;
        public event System.Action InteractionPressed;

        private void Awake()
        {
            motor = GetComponent<PrototypeMotor>();
            instance = Instantiate(actions);
            gameplay = instance.FindActionMap(actionMap, true);
            move = gameplay.FindAction("Move", true);
            interact = gameplay.FindAction("Interact", true);
            jump=gameplay.FindAction("Jump");
        }

        private void OnEnable()
        {
            hasFocus = Application.isFocused;
            interact.performed += OnInteract;
            if(jump!=null) jump.performed+=OnJump;
            gameplay.Enable();
        }

        private void OnDisable()
        {
            interact.performed -= OnInteract;
            if(jump!=null) jump.performed-=OnJump;
            gameplay.Disable();
            motor.ClearPendingInput();
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
            motor.ClearPendingInput();
        }

        private void Update()
        {
            if (!hasFocus || paused)
                return;
            if (EditingText)
            {
                waitForNeutral = true;
                motor.ClearPendingInput();
                return;
            }

            Vector2 input = move.ReadValue<Vector2>();
            if (waitForNeutral)
            {
                waitForNeutral = input.sqrMagnitude > 0.0001f;
                return;
            }

            motor.SetMoveInput(input);
        }

        private void OnJump(InputAction.CallbackContext context) { if(hasFocus && !paused && !EditingText) motor.RequestJump(); }

        private void OnInteract(InputAction.CallbackContext context)
        {
            if (!hasFocus || paused || EditingText)
                return;

            if (interactor != null)
                interactor.TryInteract();
            InteractionPressed?.Invoke();
        }

        private void OnApplicationPause(bool value)
        {
            paused = value;
            waitForNeutral = true;
            motor.ClearPendingInput();
        }
    }
}
