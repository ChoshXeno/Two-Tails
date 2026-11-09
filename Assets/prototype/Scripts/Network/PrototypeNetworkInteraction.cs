using Unity.Netcode;
using UnityEngine;

namespace TwoTails.Prototype
{
    [RequireComponent(typeof(NetworkPlayerPrototype))]
    public sealed class PrototypeNetworkInteraction : NetworkBehaviour
    {
        [SerializeField] private Transform interactionOrigin;
        private PrototypeLocalInput input;
        private NetworkPlayerPrototype player;
        private PrototypeNetworkLever lever;
        private string feedback;
        private float feedbackUntil;

        private void Awake()
        {
            input = GetComponent<PrototypeLocalInput>();
            player = GetComponent<NetworkPlayerPrototype>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
                input.InteractionPressed += Request;
        }

        public override void OnNetworkDespawn()
        {
            input.InteractionPressed -= Request;
            feedback = null;
            feedbackUntil = 0f;
            lever = null;
        }

        private PrototypeNetworkLever FindLever()
        {
            if (lever == null)
                lever = FindAnyObjectByType<PrototypeNetworkLever>();
            return lever;
        }

        public string GetPrompt()
        {
            if (!IsSpawned || !IsOwner)
                return "";
            if (Time.unscaledTime < feedbackUntil)
                return feedback;
            var target = FindLever();
            if (target == null)
                return "";
            string rejected = target.Check(interactionOrigin, player);
            return rejected == null ? target.Prompt : rejected == "Face the lever" ? rejected : "";
        }

        public void Request()
        {
            if (!IsSpawned || !IsOwner)
                return;

            var target = FindLever();
            if (target != null && target.IsSpawned)
                UseLeverRpc(target.NetworkObject);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void UseLeverRpc(NetworkObjectReference target, RpcParams rpc = default)
        {
            if (!IsServer || rpc.Receive.SenderClientId != OwnerClientId ||
                !NetworkPuzzleOccupancy.IsCurrentPlayer(NetworkManager, player))
                return;

            if (!target.TryGet(out var obj, NetworkManager) || !obj.IsSpawned ||
                !obj.TryGetComponent(out PrototypeNetworkLever requested))
            {
                RejectRpc("Interaction unavailable");
                return;
            }

            string rejection = requested.TryUse(interactionOrigin, player);
            if (rejection != null)
                RejectRpc(rejection);
        }

        [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Server)]
        private void RejectRpc(string message)
        {
            feedback = message;
            feedbackUntil = Time.unscaledTime + 1.2f;
        }
    }
}
