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
            var mechanics=GetComponent<AnimalMechanicsActor>();
            if(mechanics!=null&&!string.IsNullOrEmpty(mechanics.Prompt))return mechanics.Prompt;
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

            if(GetComponent<AnimalMechanicsActor>()?.TryRequest()==true)return;
            var target = FindLever();
            if (target != null && target.IsSpawned)
                UseLeverRpc(target.NetworkObject);
        }

        public void RequestMechanic(int target){if(IsOwner&&IsSpawned&&AnimalMechanicsWorld.Instance!=null)UseMechanicRpc(target,AnimalMechanicsWorld.Instance.State.Revision);}
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]
        private void UseMechanicRpc(int target,int revision,RpcParams rpc=default)
        {
            if(!IsServer||rpc.Receive.SenderClientId!=OwnerClientId||!NetworkPuzzleOccupancy.IsCurrentPlayer(NetworkManager,player))return;
            var world=AnimalMechanicsWorld.Instance;if(world==null)return;
            if (revision != world.State.Revision) { MechanicFeedbackRpc("Target changed. Try again."); return; }
            world.TryUse(GetComponent<AnimalMechanicsActor>(),target,out string message);MechanicFeedbackRpc(message);
        }
        [Rpc(SendTo.Owner,InvokePermission=RpcInvokePermission.Server)]
        private void MechanicFeedbackRpc(string message)=>GetComponent<AnimalMechanicsActor>().Feedback(message);
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
