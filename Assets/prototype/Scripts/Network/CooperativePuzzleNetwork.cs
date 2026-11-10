using Unity.Netcode;
using UnityEngine;
namespace TwoTails.Prototype
{
 [RequireComponent(typeof(NetworkObject),typeof(CooperativePuzzle))]
 public sealed class CooperativePuzzleNetwork:NetworkBehaviour
 {
  private readonly NetworkVariable<CooperativePuzzleState> state=new();
  private CooperativePuzzle puzzle;
  private void Awake()=>puzzle=GetComponent<CooperativePuzzle>();
  public override void OnNetworkSpawn(){if(IsServer){puzzle.ResetPuzzle(false);state.Value=puzzle.State;}else puzzle.Apply(state.Value);state.OnValueChanged+=Changed;}
  public override void OnNetworkDespawn(){state.OnValueChanged-=Changed;puzzle.ClearSession();}
  private void Changed(CooperativePuzzleState old,CooperativePuzzleState current){if(!IsServer)puzzle.Apply(current);}
  public void Publish(CooperativePuzzleState current){if(IsSpawned&&IsServer&&!state.Value.Equals(current))state.Value=current;}
  public void RequestPush(int index,NetworkPlayerPrototype player){if(IsSpawned&&player.IsOwner)PushRpc(index);}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Everyone)]
  private void PushRpc(int index,RpcParams rpc=default)
  {
   if(!IsServer||!NetworkManager.ConnectedClients.TryGetValue(rpc.Receive.SenderClientId,out var c)||c.PlayerObject==null)return;
   var p=c.PlayerObject.GetComponent<NetworkPlayerPrototype>();
   if(NetworkPuzzleOccupancy.IsCurrentPlayer(NetworkManager,p))puzzle.TryPush(index,p.Motor);
  }
  public void Warp(NetworkPlayerPrototype player,Vector3 position){if(IsServer)WarpRpc(player.NetworkObject,position);}
  [Rpc(SendTo.Everyone,InvokePermission=RpcInvokePermission.Server)]
  private void WarpRpc(NetworkObjectReference reference,Vector3 position)
  {if(reference.TryGet(out var obj,NetworkManager)&&obj.IsOwner)obj.GetComponent<PrototypeMotor>().Warp(position);}
 }
}
