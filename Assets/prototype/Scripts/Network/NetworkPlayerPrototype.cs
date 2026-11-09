using Unity.Netcode;
using UnityEngine;

namespace TwoTails.Prototype
{
    [RequireComponent(typeof(PrototypeMotor), typeof(PrototypeLocalInput), typeof(CharacterController))]
    public sealed class NetworkPlayerPrototype : NetworkBehaviour
    {
        [SerializeField] private Renderer capsule;
        [SerializeField] private Material hostMaterial;
        [SerializeField] private Material clientMaterial;

        private readonly NetworkVariable<byte> slot = new();
        private PrototypeNetworkSession session;
        public PrototypeMotor Motor { get; private set; }
        private PrototypeLocalInput input;
        private CharacterController controller;

        private void Awake()
        {
            Motor = GetComponent<PrototypeMotor>();
            input = GetComponent<PrototypeLocalInput>();
            controller = GetComponent<CharacterController>();
        }

        public override void OnNetworkSpawn()
        {
            session = NetworkManager.GetComponent<PrototypeNetworkSession>();
            if (IsServer)
                slot.Value = session.SlotFor(OwnerClientId);
            slot.OnValueChanged += ApplySlot;
            ApplySlot(0, slot.Value);

            if (IsOwner)
            {
                Motor.SetLocalContext(Camera.main.transform);
                controller.enabled = true;
                Motor.enabled = true;
                input.enabled = true;
            }

            session.Register(this, IsOwner);
        }

        public override void OnNetworkDespawn()
        {
            slot.OnValueChanged -= ApplySlot;
            input.enabled = false;
            Motor.enabled = false;
            controller.enabled = false;
            if (session != null)
                session.Unregister(this);
        }

        private void ApplySlot(byte previous, byte current)
        {
            capsule.sharedMaterial = current == 0 ? hostMaterial : clientMaterial;
        }
    }
}
