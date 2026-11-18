using UnityEngine;
namespace TwoTails.Prototype
{
 public enum PrototypeMotion { Idle, Run, Jump, Fall, Climb, Interact, Ability }
 public enum PrototypeSpecies { Fox, Monkey, Otter, Beaver }
 public sealed class PrototypeAnimal : MonoBehaviour
 {
  [SerializeField, Tooltip("Offline identity; online identity is assigned by the server slot.")] private PrototypeSpecies species;
  public PrototypeSpecies Species => species;
  public int Weight => 2;
  public PrototypeMotion Motion { get; private set; }
  public event System.Action<PrototypeMotion> MotionChanged;
  public void ApplyMotion(PrototypeMotion value) { if(Motion==value)return;Motion=value;MotionChanged?.Invoke(value); }
  private MaterialPropertyBlock colors;
  private void Start()=>Apply(species);
  public static Color ColorFor(PrototypeSpecies value)=>value==PrototypeSpecies.Fox?new Color(1,.34f,.04f):value==PrototypeSpecies.Monkey?new Color(1,.85f,.08f):value==PrototypeSpecies.Otter?new Color(.36f,.16f,.06f):Color.white;
  public void Apply(PrototypeSpecies value){species=value;var motor=GetComponent<PrototypeMotor>();if(motor==null)return;var renderer=motor.Facing.GetComponentInChildren<Renderer>();if(renderer==null)return;colors??=new MaterialPropertyBlock();renderer.GetPropertyBlock(colors);colors.SetColor("_BaseColor",ColorFor(value));renderer.SetPropertyBlock(colors);}
 }
}

