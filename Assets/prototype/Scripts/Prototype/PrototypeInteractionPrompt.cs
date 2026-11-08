using UnityEngine;

namespace TwoTails.Prototype
{
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    public sealed class PrototypeInteractionPrompt : MonoBehaviour
    {
        [SerializeField, Tooltip("Player whose current interaction state appears in this label.")] private PrototypeInteractor interactor;
        [SerializeField, Tooltip("Debug labels use P2 key prompts and are hidden when P2 is disabled.")] private bool debugPlayer;
        private UnityEngine.UI.Text label;
        private PrototypeInteractor.Feedback? previous;

        private void Awake() => label = GetComponent<UnityEngine.UI.Text>();

        private void Update()
        {
            var feedback = interactor != null && interactor.isActiveAndEnabled
                ? interactor.GetFeedback() : PrototypeInteractor.Feedback.Hidden;
            if (feedback == previous)
                return;

            previous = feedback;
            string input = debugPlayer ? "0" : "E / Interact";
            string text = feedback switch
            {
                PrototypeInteractor.Feedback.OpenDoor => $"{input}: Open door",
                PrototypeInteractor.Feedback.CloseDoor => $"{input}: Close door",
                PrototypeInteractor.Feedback.FaceLever => "Face the lever to interact",
                PrototypeInteractor.Feedback.Opening => "Door opening...",
                PrototypeInteractor.Feedback.Closing => "Door closing...",
                _ => ""
            };
            label.text = debugPlayer && text.Length > 0 ? "P2: " + text : text;
        }
    }
}
