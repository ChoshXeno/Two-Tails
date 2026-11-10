using UnityEngine;
namespace TwoTails.Prototype
{
 public enum PrototypeMotion { Idle, Run, Jump, Fall, Climb, Interact, Ability }
 public enum PrototypeSpecies { Fox, Monkey }
 public sealed class PrototypeAnimal : MonoBehaviour
 {
  [SerializeField, Tooltip("Offline identity; online identity is assigned by the server slot.")] private PrototypeSpecies species;
  public PrototypeSpecies Species => species;
  public int Weight => 2;
  public PrototypeMotion Motion { get; private set; }
  public event System.Action<PrototypeMotion> MotionChanged;
  public void ApplyMotion(PrototypeMotion value) { if(Motion==value)return;Motion=value;MotionChanged?.Invoke(value); }
  public void Apply(PrototypeSpecies value) => species=value;
 }
}
