using UnityEngine;
using UnityEngine.UI;

namespace TwoTails.Prototype
{
    internal sealed class BeaverInventoryHud : MonoBehaviour
    {
        private AnimalMechanicsActor actor;
        private Text inventory;
        private Text buildHint;
        private string buildKey;
        private Button buildButton;

        public static Text CreateLabel(RectTransform viewport)
        {
            var label = PrototypeTestMenu.TextAt(viewport, "", new Vector2(0, 1), new Vector2(320, 44), 25);
            label.alignment = TextAnchor.MiddleLeft;
            label.rectTransform.pivot = new Vector2(0, 1);
            label.rectTransform.anchoredPosition = new Vector2(24, -22);
            return label;
        }

        public static void Refresh(Text label, AnimalMechanicsActor actor)
        {
            string content = actor != null && actor.IsBeaver ? $"Woods collected: {actor.Inventory}" : "";
            if (label.text != content) label.text = content;
        }

        public void Configure(RectTransform viewport, AnimalMechanicsActor player, string key)
        {
            actor = player;
            buildKey = key;
            inventory = CreateLabel(viewport);
            buildHint = PrototypeTestMenu.TextAt(viewport, "", new Vector2(.5f, .77f), new Vector2(640, 45), 24);
            PrototypeTestMenu.ButtonAt(viewport, "Build", new Vector2(.8f, .16f), PressBuild, new Vector2(180, 90));
            buildButton = viewport.Find("Build").GetComponent<Button>();
            buildButton.gameObject.SetActive(false);
        }

        private void PressBuild() => actor?.RequestBuild();

        private void Update()
        {
            Refresh(inventory, actor);
            string reason = null;
            bool visible = actor != null && actor.GetBuildStatus(out reason);
            buildButton.gameObject.SetActive(visible);
            buildButton.interactable = visible && reason == null;
            buildHint.text = visible ? reason ?? $"{buildKey}: Build" : "";
        }
    }
}
