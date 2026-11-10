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
        [SerializeField] private CapsuleCollider serverProxy;

        private readonly NetworkVariable<byte> motion = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private void Update() { if(IsSpawned && IsOwner) motion.Value=(byte)Motor.Motion; }
        private void ApplyMotion(byte previous,byte current) { if(!IsOwner && TryGetComponent<PrototypeAnimal>(out var animal)) animal.ApplyMotion((PrototypeMotion)current); }
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
            motion.OnValueChanged+=ApplyMotion;
            ApplyMotion(0,motion.Value);
            session = NetworkManager.GetComponent<PrototypeNetworkSession>();
            if (IsServer)
                slot.Value = session.AnimalSlotFor(OwnerClientId);
            slot.OnValueChanged += ApplySlot;
            ApplySlot(0, slot.Value);

            if (IsOwner)
            {
                Motor.SetLocalContext(Camera.main.transform);
                controller.enabled = true;
                Motor.enabled = true;
                input.enabled = true;
            }
            if (serverProxy != null)
                serverProxy.enabled = IsServer && !IsOwner;

            session.Register(this, IsOwner);
        }

        public override void OnNetworkDespawn()
        {
            motion.OnValueChanged-=ApplyMotion;
            slot.OnValueChanged -= ApplySlot;
            input.enabled = false;
            Motor.enabled = false;
            controller.enabled = false;
            if (serverProxy != null)
                serverProxy.enabled = false;
            if (session != null)
                session.Unregister(this);
        }

        private void ApplySlot(byte previous, byte current)
        {
            if(TryGetComponent<PrototypeAnimal>(out var animal)) animal.Apply(current==0?PrototypeSpecies.Fox:PrototypeSpecies.Monkey);
            capsule.sharedMaterial = current == 0 ? hostMaterial : clientMaterial;
            var night=FindAnyObjectByType<PrototypeNightEnvironment>();
            if(night!=null)foreach(var renderer in GetComponentsInChildren<MeshRenderer>())night.Prepare(renderer);
        }
    }
}


