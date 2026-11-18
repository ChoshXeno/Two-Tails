#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace TwoTails.Prototype
{
 // Explicit opt-in QA driver. Uses the owner motor and production request RPC, never writes server state.
 public sealed class AnimalMechanicsProbe:MonoBehaviour
 {
  private NetworkPlayerPrototype actor;private PrototypeMotor motor;private bool failed;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static void Boot(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-animalProbe")<0)return;var g=new GameObject("Animal QA");DontDestroyOnLoad(g);g.AddComponent<AnimalMechanicsProbe>();}
  private bool Check(bool condition,string message){Debug.Log("ANIMAL_QA: "+(condition?"PASS ":"FAIL ")+message);failed|=!condition;return condition;}
  private IEnumerator Start()
  {
   Application.runInBackground=true;SceneManager.LoadScene(PrototypeTestMenu.OnlineScene);yield return null;
   var session=FindAnyObjectByType<PrototypeNetworkSession>();var args=Environment.GetCommandLineArgs();int code=Array.IndexOf(args,"-joinCode");
   if(code>=0&&code+1<args.Length){var task=FindAnyObjectByType<PrototypeOnlineRoom>().JoinAsync(args[code+1]);while(!task.IsCompleted)yield return null;}else session.StartSession(false,"127.0.0.1","7777");
   float deadline=Time.realtimeSinceStartup+40;while(!session.LocalConnected&&Time.realtimeSinceStartup<deadline)yield return null;
   if(!Check(session.LocalConnected,"connected independent client"))yield break;
   actor=session.LocalPlayer;motor=actor.Motor;actor.GetComponent<PrototypeLocalInput>().enabled=false;motor.SetLocalContext(null);yield return new WaitForSeconds(1);
   if(Array.IndexOf(args,"-animalObserve")>=0){Debug.Log("ANIMAL_QA: REJOIN progress="+AnimalMechanicsWorld.Instance.Progress+" water="+AnimalMechanicsWorld.Instance.State.WaterLevel+" inventory="+actor.HeldLogs);yield break;}
   yield return Approach(new Vector3(5,.1f,-6.1f),Vector3.forward,1);
   if(!Check(actor.GetComponent<PrototypeAnimal>().Species==PrototypeSpecies.Beaver,"client exchanged to Beaver"))yield break;
   if(Array.IndexOf(args,"-animalSwapOnly")>=0){yield return Approach(new Vector3(5,.1f,-5),Vector3.back,1);Check(actor.GetComponent<PrototypeAnimal>().Species==PrototypeSpecies.Monkey,"swap back isolated");yield break;}
   yield return Walk(new Vector3(5,0,-11));yield return Walk(new Vector3(12,0,-12));
   for(int i=0;i<3;i++)
   {
    yield return Walk(new Vector3(12.1f,0,-16-3*i));motor.Facing.rotation=Quaternion.LookRotation(Vector3.right);yield return new WaitForSeconds(.7f);
    actor.GetComponent<PrototypeNetworkInteraction>().RequestMechanic(10+i);actor.GetComponent<PrototypeNetworkInteraction>().RequestMechanic(10+i);yield return new WaitForSeconds(.8f);
    if(!Check(AnimalMechanicsWorld.Count(actor.HeldLogs)==i+1,"unique pickup "+i+" duplicate request did not duplicate inventory"))yield break;
   }
   yield return Walk(new Vector3(12,0,-11.15f));motor.Facing.rotation=Quaternion.LookRotation(Vector3.forward);yield return new WaitForSeconds(.7f);
   for(int i=0;i<3;i++){actor.GetComponent<PrototypeNetworkInteraction>().RequestMechanic(20);yield return new WaitForSeconds(.8f);if(!Check(AnimalMechanicsWorld.Instance.Progress==i+1,"deposit "+(i+1)))yield break;}
   Check(actor.HeldLogs==0,"inventory consumed once");
   yield return new WaitForSeconds(15);
   Check(Mathf.Abs(AnimalMechanicsWorld.Instance.State.WaterLevel)<.01f,"authoritative target water level");
   var world = AnimalMechanicsWorld.Instance;
   var mechanics = actor.GetComponent<AnimalMechanicsActor>();
   int reclaim = world.FindTarget(mechanics);
   if (!Check(world.IsPickup(reclaim), "valid filled slot Pickup target")) yield break;
   ulong completedSlots = world.State.Slots;
   float fullWater = world.State.WaterLevel;
   var interaction = actor.GetComponent<PrototypeNetworkInteraction>();
   interaction.RequestMechanic(reclaim);
   interaction.RequestMechanic(reclaim);
   yield return new WaitForSeconds(2);
   Check(world.Progress == 2 && mechanics.Inventory == 1 && world.State.WaterLevel < fullWater,
       "duplicate reclaim returns exactly one log and starts drainage");
   mechanics.RequestBuild();
   yield return new WaitForSeconds(4);
   Check(world.State.Slots == completedSlots && mechanics.Inventory == 0 &&
       Mathf.Abs(world.State.WaterLevel - fullWater) < .01f, "rebuild restores the exact slot and raises water");
   yield return Approach(new Vector3(5,.1f,-5),Vector3.back,1);
   Check(actor.GetComponent<PrototypeAnimal>().Species==PrototypeSpecies.Monkey,"exchange back to original animal");
   yield return Approach(new Vector3(-5,.1f,-6.1f),Vector3.forward,0);
   if(!Check(actor.GetComponent<PrototypeAnimal>().Species==PrototypeSpecies.Otter,"client exchanged to Otter"))yield break;
   motor.Warp(new Vector3(-14,-1.4f,-23));yield return new WaitForSeconds(1);var swimmer=actor.GetComponent<PrototypeSwimmer>();
   Check(swimmer.IsSwimming,"automatic surface swim");swimmer.SetInput(true,false);yield return new WaitForSeconds(3);swimmer.ClearInput();float depth=actor.transform.position.y;
   Check(depth>=-3.91f&&depth<=-3.7f,"Dive respects floor clearance");yield return new WaitForSeconds(1);Check(Mathf.Abs(actor.transform.position.y-depth)<.04f,"released depth held");
   swimmer.SetInput(true,true);yield return new WaitForSeconds(1);Check(Mathf.Abs(actor.transform.position.y-depth)<.04f,"Dive plus Surface neutral");
   swimmer.SetInput(false,true);yield return new WaitForSeconds(3);swimmer.ClearInput();Check(Mathf.Abs(actor.transform.position.y+1.4f)<.1f,"Surface returns to water line");
   motor.Warp(new Vector3(12,-.8f,-29));yield return new WaitForSeconds(1);yield return Walk(new Vector3(12,0,-38.5f));
   Check(!swimmer.IsSwimming&&actor.transform.position.y>-.1f,"swim to opposite bank via exit ramp");
   if(Array.IndexOf(args,"-animalCycle")>=0&&code>=0)
   {
    var room=FindAnyObjectByType<PrototypeOnlineRoom>();var leaving=room.LeaveAsync();while(!leaving.IsCompleted)yield return null;yield return new WaitForSeconds(2);
    var joining=room.JoinAsync(args[code+1]);while(!joining.IsCompleted)yield return null;yield return new WaitForSeconds(1);
    if(!Check(session.LocalConnected,"normal leave and rejoin"))yield break;
    actor=session.LocalPlayer;motor=actor.Motor;actor.GetComponent<PrototypeLocalInput>().enabled=false;motor.SetLocalContext(null);
    Check(actor.GetComponent<PrototypeAnimal>().Species==PrototypeSpecies.Otter,"rejoin retains available animal without duplicate display");
    Check(AnimalMechanicsWorld.Instance.Progress==3&&Mathf.Abs(AnimalMechanicsWorld.Instance.State.WaterLevel)<.01f,"late join dam and water persistence");
    for(int i=0;i<2;i++){yield return Approach(new Vector3(0,.1f,-4.8f),Vector3.back,30);Check(AnimalMechanicsWorld.Instance.Progress==0&&AnimalMechanicsWorld.Instance.State.Picked==0&&Mathf.Abs(AnimalMechanicsWorld.Instance.State.WaterLevel+4)<.01f&&actor.HeldLogs==0&&!actor.GetComponent<PrototypeSwimmer>().IsSwimming,"synchronized reset "+i);}
    yield return Approach(new Vector3(5,.1f,-6.1f),Vector3.forward,1);Check(actor.GetComponent<PrototypeAnimal>().Species==PrototypeSpecies.Beaver,"Beaver for inventory disconnect test");
    yield return Approach(new Vector3(12.2f,-.8f,-16),Vector3.right,10);Check(actor.HeldLogs==1,"unspent log acquired");
    leaving=room.LeaveAsync();while(!leaving.IsCompleted)yield return null;yield return new WaitForSeconds(2);joining=room.JoinAsync(args[code+1]);while(!joining.IsCompleted)yield return null;yield return new WaitForSeconds(1);
    if(!Check(session.LocalConnected,"second normal rejoin"))yield break;
    actor=session.LocalPlayer;motor=actor.Motor;actor.GetComponent<PrototypeLocalInput>().enabled=false;motor.SetLocalContext(null);
    Check(actor.HeldLogs==0&&AnimalMechanicsWorld.Instance.State.Picked==0,"disconnect returns unspent log and clears inventory");
   }   Debug.Log("ANIMAL_QA: COMPLETE failed="+failed+" progress="+AnimalMechanicsWorld.Instance.Progress+" water="+AnimalMechanicsWorld.Instance.State.WaterLevel+" position="+actor.transform.position);
  }
  private IEnumerator Approach(Vector3 position,Vector3 facing,int id){motor.Warp(position);motor.Facing.rotation=Quaternion.LookRotation(facing);yield return new WaitForSeconds(.8f);actor.GetComponent<PrototypeNetworkInteraction>().RequestMechanic(id);yield return new WaitForSeconds(1.2f);Debug.Log("ANIMAL_QA: exchange feedback="+actor.GetComponent<AnimalMechanicsActor>().Prompt+" species="+actor.GetComponent<PrototypeAnimal>().Species+" position="+actor.transform.position+" facing="+motor.Facing.forward);}
  private IEnumerator Walk(Vector3 target)
  {
   float end=Time.realtimeSinceStartup+18;
   while(Time.realtimeSinceStartup<end){var delta=target-actor.transform.position;delta.y=0;if(delta.magnitude<.18f)break;motor.SetMoveInput(new Vector2(delta.x,delta.z).normalized);yield return null;}
   motor.SetMoveInput(Vector2.zero);yield return new WaitForSeconds(.2f);var d=target-actor.transform.position;d.y=0;Check(d.magnitude<.3f,"walk to "+target+" actual "+actor.transform.position);
  }
 }
}
#endif
