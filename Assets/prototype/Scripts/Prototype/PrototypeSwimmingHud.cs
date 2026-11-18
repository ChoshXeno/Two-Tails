using UnityEngine;
using UnityEngine.UI;

namespace TwoTails.Prototype
{
    public sealed class PrototypeSwimmingHud : MonoBehaviour
    {
        [SerializeField] private PrototypeNetworkSession session;
        [SerializeField] private Text jumpLabel;
        [SerializeField] private GameObject surface;
        private PrototypeScreenButton auxiliaryControl;
        private Text auxiliaryLabel;
        private Text inventoryLabel;
        private Text buildHint;
        private Image auxiliaryImage;
        private Color activeColor;

        private void Awake()
        {
            auxiliaryControl = surface.GetComponent<PrototypeScreenButton>();
            auxiliaryLabel = surface.GetComponentInChildren<Text>(true);
            auxiliaryImage = surface.GetComponent<Image>();
            if (auxiliaryImage != null) activeColor = auxiliaryImage.color;

            inventoryLabel = BeaverInventoryHud.CreateLabel((RectTransform)transform);
            buildHint = PrototypeTestMenu.TextAt(transform, "", Vector2.one, new Vector2(360, 44), 23);
            var rect = buildHint.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(1, 0);
            rect.anchoredPosition = ((RectTransform)surface.transform).anchoredPosition + new Vector2(0, 105);
        }

        private void Update()
        {
            var player = session.LocalPlayer;
            var actor = player != null ? player.GetComponent<AnimalMechanicsActor>() : null;
            bool swimming = player != null && player.GetComponent<PrototypeSwimmer>()?.IsSwimming == true;
            string reason = null;
            bool building = !swimming && actor != null && actor.GetBuildStatus(out reason);
            bool available = swimming || (building && reason == null);

            jumpLabel.text = swimming ? "Dive" : "Jump";
            surface.SetActive(swimming || building);
            auxiliaryControl.enabled = available;
            auxiliaryLabel.text = swimming ? "Surface" : "Build";
            if (auxiliaryImage != null)
                auxiliaryImage.color = available ? activeColor : Color.Lerp(activeColor, Color.gray, .65f);
            BeaverInventoryHud.Refresh(inventoryLabel, actor);
            buildHint.text = building ? reason ?? "" : "";
        }
    }
}
