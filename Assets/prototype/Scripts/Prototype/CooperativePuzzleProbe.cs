using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace TwoTails.Prototype
{
 // Opt-in development test only. Ordinary builds never create this object.
 public sealed class CooperativePuzzleProbe:MonoBehaviour
 {
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static void Boot(){if(!Debug.isDebugBuild&&!Application.isEditor)return;if(Array.IndexOf(Environment.GetCommandLineArgs(),"-coopProbe")<0)return;var g=new GameObject("Cooperative QA");DontDestroyOnLoad(g);g.AddComponent<CooperativePuzzleProbe>();}
  private IEnumerator Start()
  {
   Application.runInBackground=true;SceneManager.LoadScene(PrototypeTestMenu.OnlineScene);yield return null;
   var session=FindAnyObjectByType<PrototypeNetworkSession>();
   var args=Environment.GetCommandLineArgs();int codeIndex=Array.IndexOf(args,"-joinCode");
   if(codeIndex>=0&&codeIndex+1<args.Length){var task=FindAnyObjectByType<PrototypeOnlineRoom>().JoinAsync(args[codeIndex+1]);while(!task.IsCompleted)yield return null;}
   else session.StartSession(false,"127.0.0.1","7777");
   float end=Time.realtimeSinceStartup+30;while(!session.LocalConnected&&Time.realtimeSinceStartup<end)yield return null;
   if(!session.LocalConnected){Debug.LogError("COOP_QA: Connection failed");yield break;}
   var actor=session.LocalPlayer;var motor=actor.Motor;actor.GetComponent<PrototypeLocalInput>().enabled=false;motor.SetLocalContext(null);
   Debug.Log("COOP_QA: species="+actor.GetComponent<PrototypeAnimal>().Species+" developerLights="+FindAnyObjectByType<PrototypeNightEnvironment>().DeveloperLights);
   foreach(var kind in new[]{CooperativePuzzleKind.Switchback,CooperativePuzzleKind.Bridge,CooperativePuzzleKind.Counterweight})
   {
    var puzzle=FindObjectsByType<CooperativePuzzle>().Single(p=>p.Kind==kind);var origin=puzzle.transform.position;
    Debug.Log("COOP_QA: Waiting "+kind);
    end=Time.realtimeSinceStartup+180;
    while(Time.realtimeSinceStartup<end&&(puzzle.State.Plates&1)==0)yield return null;
    if((puzzle.State.Plates&1)==0){Debug.LogError("COOP_QA: Host plate timeout "+kind);yield break;}
    if(kind==CooperativePuzzleKind.Counterweight)
    {
     for(int k=0;k<2;k++)
     {
      motor.Warp(origin+new Vector3(2.5f+k*2,.1f,-3.2f));motor.Facing.rotation=Quaternion.identity;motor.SetMoveInput(Vector2.up);
      float until=Time.realtimeSinceStartup+3;while(Time.realtimeSinceStartup<until)yield return null;motor.SetMoveInput(Vector2.zero);yield return new WaitForSeconds(.5f);
      Debug.Log("COOP_QA: pushed="+k+" load="+puzzle.State.WeightA+"/"+puzzle.State.WeightB);
     }
    }
    else
    {
     bool bridge=kind==CooperativePuzzleKind.Bridge;
     motor.Warp(origin+(bridge?new Vector3(0,.1f,1):new Vector3(2.5f,.1f,-1)));
     yield return Walk(motor,origin+(bridge?new Vector3(0,0,14):new Vector3(2.5f,0,8)));
    }
    yield return new WaitForSeconds(2);
    Debug.Log("COOP_QA: "+kind+" latched="+puzzle.State.Latched+" A="+puzzle.State.A+" B="+puzzle.State.B+" loads="+puzzle.State.WeightA+"/"+puzzle.State.WeightB+" box1="+puzzle.State.Box1+" box2="+puzzle.State.Box2);
    if(!puzzle.State.Latched){Debug.LogError("COOP_QA: Handoff failed "+kind);yield break;}
    motor.Warp(origin+new Vector3(2,.1f,-4));
   }
   Debug.Log("COOP_QA: All three client operations completed");
  }
  private IEnumerator Walk(PrototypeMotor motor,Vector3 goal)
  {
   float end=Time.realtimeSinceStartup+15;
   while(Time.realtimeSinceStartup<end){var d=goal-motor.transform.position;d.y=0;if(d.magnitude<.2f)break;motor.SetMoveInput(new Vector2(d.x,d.z).normalized);yield return null;}
   motor.SetMoveInput(Vector2.zero);
  }
 }
}
