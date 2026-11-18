using System.Collections;
using UnityEngine;
namespace TwoTails.Prototype
{
 [RequireComponent(typeof(PrototypeAnimal),typeof(PrototypeMotor))]
 public sealed class AnimalMechanicsActor:MonoBehaviour
 {
  private int localLogs;
  private float lockedUntil,feedbackUntil;
  private string feedback;
  public bool Locked=>Time.unscaledTime<lockedUntil;
  public int HeldLogs {get=>TryGetComponent<NetworkPlayerPrototype>(out var n)&&n.IsSpawned?n.HeldLogs:localLogs;}
  public int Inventory {get{int n=0;for(int b=HeldLogs;b!=0;b&=b-1)n++;return n;}}
  public void SetHeldLogs(int mask){if(TryGetComponent<NetworkPlayerPrototype>(out var n)&&n.IsSpawned)n.SetHeldLogs(mask);else localLogs=mask;}
  public void Feedback(string text){feedback=text;feedbackUntil=Time.unscaledTime+2.5f;}
  public string Prompt
  {
   get{if(Time.unscaledTime<feedbackUntil)return feedback;var world=AnimalMechanicsWorld.Instance;if(world==null)return "";int id=world.FindTarget(this);return id<0?"":world.Prompt(this,id);}
  }
  public bool IsBeaver => GetComponent<PrototypeAnimal>().Species == PrototypeSpecies.Beaver;
  public bool HasPickupTarget
  {
   get
   {
    var world = AnimalMechanicsWorld.Instance;
    return IsBeaver && world != null && world.IsPickup(world.FindTarget(this));
   }
  }

  public bool GetBuildStatus(out string reason)
  {
   reason = null;
   return AnimalMechanicsWorld.Instance != null &&
          AnimalMechanicsWorld.Instance.GetBuildStatus(this, out reason);
  }

  public void RequestBuild()
  {
   if (!GetBuildStatus(out string reason)) return;
   if (reason != null) { Feedback(reason); return; }
   Dispatch(AnimalMechanicsWorld.BuildTarget);
  }

  public bool TryRequest()
  {
   var world=AnimalMechanicsWorld.Instance;if(world==null)return false;int id=world.FindTarget(this);if(id<0)return false;
   if(Locked){Feedback("Please wait");return true;}
   Dispatch(id);
   return true;
  }

  private void Dispatch(int id)
  {
   var world = AnimalMechanicsWorld.Instance;
   if (world == null) return;
   if(TryGetComponent<NetworkPlayerPrototype>(out var n)&&n.IsSpawned){if(n.IsOwner)GetComponent<PrototypeNetworkInteraction>().RequestMechanic(id);}
   else {world.TryUse(this,id,out string message);Feedback(message);}
  }
  public void Transition(Vector3 position)
  {
   lockedUntil=Time.unscaledTime+.65f;GetComponent<PrototypeMotor>().ClearPendingInput();
   if(TryGetComponent<NetworkPlayerPrototype>(out var n)&&n.IsSpawned&&!n.IsOwner)transform.position=position;
   else GetComponent<PrototypeMotor>().Warp(position);
   StopAllCoroutines();StartCoroutine(Animate());
  }
  private IEnumerator Animate()
  {
   var visual=GetComponent<PrototypeMotor>().Facing;Vector3 normal=Vector3.one;float start=Time.unscaledTime;
   while(Time.unscaledTime-start<.6f){float t=(Time.unscaledTime-start)/.6f;visual.localScale=normal*Mathf.Lerp(.2f,1,Mathf.SmoothStep(0,1,t));yield return null;}
   visual.localScale=normal;
  }
  private void OnDisable(){lockedUntil=0;feedbackUntil=0;StopAllCoroutines();var m=GetComponent<PrototypeMotor>();if(m!=null)m.Facing.localScale=Vector3.one;}
 }
}
