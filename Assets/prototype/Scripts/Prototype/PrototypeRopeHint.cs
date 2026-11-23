using UnityEngine;
using UnityEngine.UI;

namespace TwoTails.Prototype
{
    [RequireComponent(typeof(Text))]
    public sealed class PrototypeRopeHint : MonoBehaviour
    {
        private PrototypeMotor motor;
        private Text label;
        private string hint;
        public void Bind(PrototypeMotor player, string keys)
        {
            motor = player;
            label = GetComponent<Text>();
            label.raycastTarget = false;
            hint = "Up / Down / Jump: " + keys;
        }

        private void Update()
        {
            if (label != null) label.text = motor != null && motor.IsOnRope ? hint : "";
        }
    }
}
