using UnityEngine;
namespace TwoTails.Prototype
{
 [RequireComponent(typeof(BoxCollider))]
 public sealed class CooperativeWeightBox:MonoBehaviour
 {
  [SerializeField,Min(1),Tooltip("Explicit puzzle mass, independent of appearance or collider count.")] private int weight=1;
  public int Weight=>weight;
  public CooperativePuzzle Puzzle {get;private set;}
  public int Index {get;private set;}
  private float nextPush;
  public void Bind(CooperativePuzzle puzzle,int index){Puzzle=puzzle;Index=index;}
  public void Push(PrototypeMotor motor)
  {
   if(Puzzle==null||Time.unscaledTime<nextPush)return;
   nextPush=Time.unscaledTime+.05f;
   if(Puzzle.Networked){var player=motor.GetComponent<NetworkPlayerPrototype>();if(player!=null&&player.IsOwner)Puzzle.Network.RequestPush(Index,player);}
   else Puzzle.TryPush(Index,motor);
  }
 }
}
