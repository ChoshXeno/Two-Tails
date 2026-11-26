using UnityEngine;
namespace TwoTails.Prototype
{
 [RequireComponent(typeof(PrototypeMotor),typeof(PrototypeAnimal))]
 public sealed class PrototypeSwimmer:MonoBehaviour
 {
  [SerializeField,Min(.1f),Tooltip("Horizontal swimming speed in metres per second.")] private float swimSpeed=3;
  [SerializeField,Min(.1f),Tooltip("Held Dive descent speed in metres per second.")] private float diveSpeed=2;
  [SerializeField,Min(.1f),Tooltip("Held Surface ascent speed in metres per second.")] private float surfaceSpeed=2;
  [SerializeField,Range(.2f,1.5f),Tooltip("Feet below water surface when floating.")] private float surfaceOffset=.8f;
  [SerializeField,Min(.05f),Tooltip("Minimum feet clearance above basin bottom.")] private float floorClearance=.15f;
  private PrototypeWaterVolume water;private bool dive,surface,floating;private float depth;
  private float entryUntil;
  internal PrototypeMotion Motion(Vector2 movement)
  {
   bool moving=movement.sqrMagnitude>.01f;
   if(!floating)return moving || dive!=surface ? PrototypeMotion.Dive_Loop : PrototypeMotion.Dive_Stop_Idle;
   if(Time.time<entryUntil)return PrototypeMotion.Swim_Start;
   return moving ? PrototypeMotion.Swim_Loop : PrototypeMotion.Swim_Stop_Idle;
  }
  public bool IsSwimming=>water!=null;
  public void SetInput(bool down,bool up){dive=down;surface=up;}
  public void ClearInput(){dive=surface=false;}
  public void ResetState(){water=null;ClearInput();}
  private void OnDisable()=>ResetState();
  public bool Move(Vector3 direction,float dt,CharacterController controller)
  {
   var position=transform.position;
   if(water!=null&&(!water.ContainsXZ(position)||position.y>water.Level-.3f))ResetState();
   if(water==null)foreach(var candidate in PrototypeWaterVolume.Active)
   {
    if(!candidate.ContainsXZ(position)||position.y>candidate.Level-.45f||position.y<candidate.Bottom-.2f)continue;
    if(GetComponent<PrototypeAnimal>().Species!=PrototypeSpecies.Otter)
    {
     if(position.y<candidate.Level-1.35f){GetComponent<PrototypeMotor>().Warp(candidate.SafeShore);GetComponent<AnimalMechanicsActor>()?.Feedback("Deep water: returned to shore");return true;}
     return false;
    }
    water=candidate;floating=true;depth=water.Level-surfaceOffset;entryUntil=Time.time+.32f;break;
   }
   if(water==null)return false;
   if(GetComponent<PrototypeAnimal>().Species!=PrototypeSpecies.Otter){ResetState();return false;}
   if(dive&&!surface){floating=false;depth-=diveSpeed*dt;}
   else if(surface&&!dive)depth+=surfaceSpeed*dt;
   float ceiling=water.Level-surfaceOffset;
   if(floating)depth=ceiling;
   depth=Mathf.Clamp(depth,water.Bottom+floorClearance,Mathf.Max(water.Bottom+floorClearance,ceiling));
   if(surface&&!dive&&depth>=ceiling-.01f)floating=true;
   // Controller collisions provide wall/floor and ramp support; no direct transform write.
   controller.Move(direction*swimSpeed*dt+Vector3.up*Mathf.Clamp(depth-position.y,-surfaceSpeed*dt,surfaceSpeed*dt));
   return true;
  }
 }
}

