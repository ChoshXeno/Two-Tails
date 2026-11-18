using Unity.Netcode;
using UnityEngine;
namespace TwoTails.Prototype
{
 [RequireComponent(typeof(NetworkObject),typeof(AnimalMechanicsWorld))]
 public sealed class AnimalMechanicsNetwork:NetworkBehaviour
 {
  private readonly NetworkVariable<AnimalMechanicsState> state=new();private AnimalMechanicsWorld world;
  private void Awake()=>world=GetComponent<AnimalMechanicsWorld>();
  public override void OnNetworkSpawn(){if(IsServer){world.Initialize();state.Value=world.State;}else world.Apply(state.Value);state.OnValueChanged+=Changed;}
  public override void OnNetworkDespawn(){state.OnValueChanged-=Changed;}
  private void Changed(AnimalMechanicsState previous,AnimalMechanicsState current){if(!IsServer)world.Apply(current);}
  public void Publish(AnimalMechanicsState value){if(IsSpawned&&IsServer&&!state.Value.Equals(value))state.Value=value;}
 }
}
