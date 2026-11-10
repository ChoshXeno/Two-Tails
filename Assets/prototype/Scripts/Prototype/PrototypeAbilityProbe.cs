#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace TwoTails.Prototype
{
 // Explicitly opted-in, reproducible headless development probe. Never runs in normal play.
 public sealed class PrototypeAbilityProbe : MonoBehaviour
 {
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static void Boot()
  {
   if(Array.IndexOf(Environment.GetCommandLineArgs(),"-abilityProbe")<0)return;
   var obj=new GameObject("Ability QA");DontDestroyOnLoad(obj);obj.AddComponent<PrototypeAbilityProbe>();
  }
  private IEnumerator Start()
  {
   Application.runInBackground=true;
   SceneManager.LoadScene(PrototypeTestMenu.OnlineScene);
   yield return null;
   var session=FindAnyObjectByType<PrototypeNetworkSession>();
   session.StartSession(false,"127.0.0.1","7777");
   float deadline=Time.realtimeSinceStartup+20;
   while(!session.LocalConnected && Time.realtimeSinceStartup<deadline)yield return null;
   if(!session.LocalConnected){Debug.LogError("ABILITY_QA: connection failed");yield break;}
   bool air=Array.IndexOf(Environment.GetCommandLineArgs(),"-airCatchProbe")>=0;
   var actor=session.LocalPlayer;
   actor.GetComponent<PrototypeLocalInput>().enabled=false;
   var controller=actor.GetComponent<CharacterController>();
   controller.enabled=false;actor.transform.position=air?new Vector3(0,0.1f,-0.9f):new Vector3(-6,0.1f,0);controller.enabled=true;
   actor.Motor.Facing.rotation=Quaternion.identity;actor.Motor.SetLocalContext(null);
   actor.Motor.SetMoveInput(Vector2.zero);
   yield return new WaitForSeconds(0.5f);
   if(air)actor.Motor.RequestJump();
   actor.Motor.SetMoveInput(Vector2.up);
   bool entered=false;float highest=actor.transform.position.y;float end=Time.realtimeSinceStartup+6;
   while(Time.realtimeSinceStartup<end)
   {
    entered |= actor.Motor.IsClimbing;highest=Mathf.Max(highest,actor.transform.position.y);
    if(entered && !actor.Motor.IsClimbing && actor.transform.position.y>(air?3.9f:2.9f))break;
    yield return null;
   }
   Debug.Log("ABILITY_QA: entered="+entered+" maximumY="+highest);
   actor.Motor.SetMoveInput(Vector2.zero);
   Debug.Log("ABILITY_QA: species="+actor.GetComponent<PrototypeAnimal>().Species+" y="+actor.transform.position.y+" climbing="+actor.Motor.IsClimbing);
  }
 }
}
#endif

