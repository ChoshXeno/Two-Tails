using UnityEngine;
namespace TwoTails.Prototype
{
 public enum PrototypeMotion { Idle, Run, Jump, Fall, Climb, Interact, Ability, Rope, Wall_Hang_Idle, Climb_End, Vine_Climb, Push, Pull, Death, Swim_Start, Swim_Loop, Swim_Stop_Idle, Dive_Loop, Dive_Stop_Idle }
 public enum PrototypeSpecies { Fox, Monkey, Otter, Beaver, Rabbit, Elephant }
 public sealed class PrototypeAnimal : MonoBehaviour
 {
  [SerializeField, Tooltip("Offline identity; online identity is assigned by the server slot.")] private PrototypeSpecies species;
  public PrototypeSpecies Species => species;
  public int Weight => species == PrototypeSpecies.Rabbit ? 1 : species == PrototypeSpecies.Elephant ? 3 : 2;
  public PrototypeMotion Motion { get; private set; }
  public event System.Action<PrototypeMotion> MotionChanged;
  public void ApplyMotion(PrototypeMotion value) { if(Motion==value)return;Motion=value;MotionChanged?.Invoke(value); }
  private void LateUpdate()
  {
   if (!TryGetComponent<PrototypeMotor>(out var motor)) return;
   var mount = motor.Facing;
   MonkeyPresentation.Show(gameObject, mount, mount.GetComponent<Renderer>(), species, Motion);
  }
  private MaterialPropertyBlock colors;
  private void Start()=>Apply(species);
  public static Color ColorFor(PrototypeSpecies value)=>value==PrototypeSpecies.Rabbit?new Color(.95f,.55f,.75f):value==PrototypeSpecies.Elephant?new Color(.45f,.58f,.72f):value==PrototypeSpecies.Fox?new Color(1,.34f,.04f):value==PrototypeSpecies.Monkey?new Color(1,.85f,.08f):value==PrototypeSpecies.Otter?new Color(.36f,.16f,.06f):Color.white;
  public void Apply(PrototypeSpecies value){species=value;var motor=GetComponent<PrototypeMotor>();if(motor==null)return;motor.ApplySpecies(value);var renderer=motor.Facing.GetComponentInChildren<Renderer>();if(renderer==null)return;colors??=new MaterialPropertyBlock();renderer.GetPropertyBlock(colors);colors.SetColor("_BaseColor",ColorFor(value));renderer.SetPropertyBlock(colors);}
 }
}

