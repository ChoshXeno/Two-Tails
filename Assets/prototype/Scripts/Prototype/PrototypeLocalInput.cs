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
        private InputAction jump; private InputAction surface;
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
            jump=gameplay.FindAction("Jump"); surface=gameplay.FindAction("Surface");
        }

        private void OnEnable()
        {
            hasFocus = Application.isFocused;
            interact.performed += OnInteract;
            interact.canceled += OnInteractReleased;
            if(jump!=null) jump.performed+=OnJump;
            if (surface != null) surface.performed += OnAuxiliary;
            gameplay.Enable();
        }

        private void OnDisable()
        {
            interact.performed -= OnInteract;
            interact.canceled -= OnInteractReleased;
            if(jump!=null) jump.performed-=OnJump;
            if (surface != null) surface.performed -= OnAuxiliary;
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
                waitForNeutral = input.sqrMagnitude > 0.0001f || jump?.IsPressed()==true || surface?.IsPressed()==true;
                return;
            }

            motor.SetMoveInput(input);
            motor.SetRopeClimb(interact.IsPressed(), surface?.IsPressed() == true);
            GetComponent<PrototypeSwimmer>()?.SetInput(surface?.IsPressed() == true, jump?.IsPressed() == true);
        }

        private void OnAuxiliary(InputAction.CallbackContext context)
        {
            if (hasFocus && !paused && !EditingText && !motor.IsOnRope)
                GetComponent<AnimalMechanicsActor>()?.RequestBuild();
        }

        private void OnJump(InputAction.CallbackContext context) { if(hasFocus && !paused && !EditingText) motor.RequestJump(); }

        private void OnInteract(InputAction.CallbackContext context)
        {
            if (!hasFocus || paused || EditingText || motor.IsOnRope)
                return;

            if(motor.TryGrip())return;
            if(GetComponent<AnimalMechanicsActor>()?.TryRequest()==true)return;
            if (interactor != null)
                interactor.TryInteract();
            InteractionPressed?.Invoke();
        }

        private void OnInteractReleased(InputAction.CallbackContext context) => motor.ReleaseGrip();

        private void OnApplicationPause(bool value)
        {
            paused = value;
            waitForNeutral = true;
            motor.ClearPendingInput();
        }
    }
}
