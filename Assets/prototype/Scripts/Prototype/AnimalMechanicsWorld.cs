using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace TwoTails.Prototype
{
 public sealed class AnimalMechanicsWorld:MonoBehaviour
 {
  public static AnimalMechanicsWorld Instance {get;private set;}
  [SerializeField] private bool networked;
  [SerializeField,Tooltip("Only players and display destinations within this dry lobby can exchange.")] private BoxCollider lobby;
  [SerializeField] private Transform[] displays;
  [SerializeField] private Renderer[] displayVisuals;
  [SerializeField] private Transform[] logs;
  [SerializeField] private Transform dam,resetPoint;
  [SerializeField] private Renderer damMarker;
  [SerializeField] private GameObject[] installedLogs;
  [SerializeField] private PrototypeWaterVolume reservoir;
  [SerializeField] private TextMesh damLabel;
  [SerializeField,Min(1),Tooltip("Number of logs to complete this test dam; must not exceed available logs.")] private int requiredLogs=3;
  [SerializeField,Min(.01f),Tooltip("Authoritative water rise speed in metres per second.")] private float riseSpeed=.3f;
  [SerializeField,Min(.1f),Tooltip("Chest to target interaction distance.")] private float interactionDistance=1.5f;
  [SerializeField,Range(0,89),Tooltip("Horizontal angle from the actual facing Visual.")] private float interactionAngle=45;
  [SerializeField,Min(.1f),Tooltip("Display reservation and transition time.")] private float swapDuration=.8f;
  private AnimalMechanicsNetwork net;
  private readonly Dictionary<AnimalMechanicsActor,double> cooldowns=new();
  private readonly RaycastHit[] sight=new RaycastHit[24];
  private readonly Collider[] clearance=new Collider[32];
  private MaterialPropertyBlock colors;
  private Vector3 startA,startB;
  private int lastProgress=-1;
  public AnimalMechanicsState State {get;private set;}
  public bool Authority=>!networked||(net!=null&&net.IsSpawned&&net.IsServer);
  public double Now=>networked&&net!=null&&net.IsSpawned?net.NetworkManager.ServerTime.Time:Time.timeAsDouble;
  public int Progress=>Count(State.Deposited);
  public static int Count(int mask){int n=0;for(;mask!=0;mask&=mask-1)n++;return n;}
  private void Awake(){colors=new MaterialPropertyBlock();if(!(Application.isEditor||Debug.isDebugBuild)&&resetPoint!=null)resetPoint.gameObject.SetActive(false);Instance=this;net=GetComponent<AnimalMechanicsNetwork>();startA=displays[0].position;startB=displays[1].position;if(!networked)Initialize();}
  private void OnDestroy(){if(Instance==this)Instance=null;}
  public void Initialize(){if(!Authority)return;Apply(new AnimalMechanicsState{SpeciesA=(byte)PrototypeSpecies.Otter,SpeciesB=(byte)PrototypeSpecies.Beaver,PositionA=startA,PositionB=startB,WaterLevel=reservoir.InitialLevel});Publish();}
  private bool Valid(AnimalMechanicsActor a)=>a!=null&&a.isActiveAndEnabled&&(networked?(NetworkManager.Singleton!=null&&(NetworkManager.Singleton.IsServer?NetworkPuzzleOccupancy.IsCurrentPlayer(NetworkManager.Singleton,a.GetComponent<NetworkPlayerPrototype>()):a.GetComponent<NetworkPlayerPrototype>().IsSpawned&&a.GetComponent<NetworkPlayerPrototype>().IsOwner)):a.GetComponent<PrototypeMotor>().isActiveAndEnabled);
  private Transform Target(int id)=>id<2&&id>=0?displays[id]:id>=10&&id<10+logs.Length?logs[id-10]:id==20?dam:id==30&&(Application.isEditor||Debug.isDebugBuild)?resetPoint:null;
  private Vector3 Point(int id)=>Target(id).position+Vector3.up*(id>=10&&id<20?.5f:1);
  public int FindTarget(AnimalMechanicsActor actor)
  {
   int best=-1;float nearest=interactionDistance;Vector3 origin=actor.transform.position+Vector3.up;
   for(int id=0;id<=30;id++){var target=Target(id);if(target==null||!target.gameObject.activeInHierarchy)continue;float d=Vector3.Distance(origin,Point(id));if(d<nearest){nearest=d;best=id;}}
   return best;
  }
  private string Check(AnimalMechanicsActor actor,int id)
  {
   var target=Target(id);if(!Valid(actor)||target==null)return "Interaction unavailable";
   if(actor.Locked)return "Please wait";
   Vector3 origin=actor.transform.position+Vector3.up,offset=Point(id)-origin;
   if(offset.magnitude>interactionDistance)return "Move closer";
   if(Vector3.Angle(Vector3.ProjectOnPlane(actor.GetComponent<PrototypeMotor>().Facing.forward,Vector3.up),Vector3.ProjectOnPlane(offset,Vector3.up))>interactionAngle)return "Face the target";
   if(PrototypeInteractor.IsBlocked(origin,offset,offset.magnitude,actor.transform,target,~0,sight))return "Interaction blocked";
   return null;
  }
  public string Prompt(AnimalMechanicsActor actor,int id)
  {
   var rejection=Check(actor,id);if(rejection!=null)return rejection;
   if(id<2)return "Swap to "+(PrototypeSpecies)(id==0?State.SpeciesA:State.SpeciesB);
   if(id<20)return "Pick up Log";
   if(id==20)return Progress>=requiredLogs?"Dam complete":$"Add Log ({Progress}/{requiredLogs})";
   return "Reset animal test";
  }
  public bool TryUse(AnimalMechanicsActor actor,int id,out string message)
  {
   message=Check(actor,id);if(!Authority){message="Waiting for host";return false;}if(message!=null)return false;
   if(cooldowns.TryGetValue(actor,out double until)&&Now<until){message="Please wait";return false;}cooldowns[actor]=Now+.15;
   var species=actor.GetComponent<PrototypeAnimal>().Species;var next=State;
   if(id<2)
   {
    if(Now<(id==0?next.BusyA:next.BusyB)){message="Display is busy";return false;}
    if(actor.Inventory>0){message="Use your logs before swapping";return false;}
    Vector3 from=actor.transform.position,to=displays[id].position;
    if(!lobby.bounds.Contains(from+Vector3.up)||!lobby.bounds.Contains(to+Vector3.up)){message="Swap only in the lobby";return false;}
    if(!Safe(to,actor.transform,displays[id])||!Safe(from,actor.transform,displays[id])){message="Swap position is blocked";return false;}
    var replacement=(PrototypeSpecies)(id==0?next.SpeciesA:next.SpeciesB);
    if(id==0){next.SpeciesA=(byte)species;next.PositionA=from;next.BusyA=Now+swapDuration;}else{next.SpeciesB=(byte)species;next.PositionB=from;next.BusyB=Now+swapDuration;}
    if(networked)actor.GetComponent<NetworkPlayerPrototype>().Exchange(replacement,to);else{actor.GetComponent<PrototypeAnimal>().Apply(replacement);actor.Transition(to);}
    message="Now playing "+replacement;
   }
   else if(id<20)
   {
    if(species!=PrototypeSpecies.Beaver){message="Only Beaver can collect logs";return false;}
    int bit=1<<(id-10);if((next.Picked&bit)!=0){message="Log already collected";return false;}next.Picked|=bit;actor.SetHeldLogs(actor.HeldLogs|bit);message="Log collected";
   }
   else if(id==20)
   {
    if(species!=PrototypeSpecies.Beaver){message="Only Beaver can build the dam";return false;}
    if(Progress>=requiredLogs){message="Dam complete";return false;}
    if(actor.HeldLogs==0){message="Collect a log first";return false;}
    int bit=actor.HeldLogs&-actor.HeldLogs;actor.SetHeldLogs(actor.HeldLogs&~bit);next.Deposited|=bit;message=$"Log added ({Count(next.Deposited)}/{requiredLogs})";
   }
   else{ResetTest();message="Animal test reset";return true;}
   next.Revision++;Apply(next);Publish();return true;
  }
  private bool Safe(Vector3 feet,Transform actor,Transform display)
  {
   if(!Physics.Raycast(feet+Vector3.up*.3f,Vector3.down,1,~(1<<2),QueryTriggerInteraction.Ignore))return false;
   int count=Physics.OverlapCapsuleNonAlloc(feet+Vector3.up*.46f,feet+Vector3.up*1.6f,.38f,clearance,~0,QueryTriggerInteraction.Ignore);
   for(int i=0;i<count;i++)if(!clearance[i].transform.IsChildOf(actor)&&!clearance[i].transform.IsChildOf(display))return false;
   return true;
  }
  public PrototypeSpecies AvailableSpecies(PrototypeSpecies preferred,NetworkPlayerPrototype joining)
  {
   // A returning connection must not duplicate an animal currently held by a display or another player.
   int used=net!=null&&!net.IsSpawned?(1<<(int)PrototypeSpecies.Otter)|(1<<(int)PrototypeSpecies.Beaver):(1<<State.SpeciesA)|(1<<State.SpeciesB);
   foreach(var player in FindObjectsByType<NetworkPlayerPrototype>())if(player!=joining&&player.IsSpawned)used|=1<<(int)player.GetComponent<PrototypeAnimal>().Species;
   if((used&(1<<(int)preferred))==0)return preferred;
   for(int i=0;i<4;i++)if((used&(1<<i))==0)return (PrototypeSpecies)i;
   return preferred;
  }
  public void ReturnLogs(int bits){if(!Authority)return;var s=State;s.Picked&=~(bits&~s.Deposited);Apply(s);Publish();}
  public void ResetTest()
  {
   if(!Authority)return;var next=State;next.Picked=next.Deposited=0;next.WaterLevel=reservoir.InitialLevel;next.Revision++;cooldowns.Clear();
   int index=0;foreach(var actor in FindObjectsByType<AnimalMechanicsActor>())if(Valid(actor))
   {actor.SetHeldLogs(0);Vector3 safe=new Vector3(-1.5f+3*index++,.1f,-4);if(networked)actor.GetComponent<NetworkPlayerPrototype>().Exchange(actor.GetComponent<PrototypeAnimal>().Species,safe);else actor.Transition(safe);}
   Apply(next);Publish();
  }
  private void FixedUpdate(){if(!Authority||Progress<requiredLogs||State.WaterLevel>=reservoir.TargetLevel)return;var next=State;next.WaterLevel=Mathf.MoveTowards(next.WaterLevel,reservoir.TargetLevel,riseSpeed*Time.fixedDeltaTime);Apply(next);Publish();}
  private void Publish(){if(networked)net.Publish(State);}
  public void Apply(AnimalMechanicsState state)
  {
   State=state;displays[0].position=state.PositionA;displays[1].position=state.PositionB;
   for(int i=0;i<2;i++){displayVisuals[i].GetPropertyBlock(colors);colors.SetColor("_BaseColor",PrototypeAnimal.ColorFor((PrototypeSpecies)(i==0?state.SpeciesA:state.SpeciesB)));displayVisuals[i].SetPropertyBlock(colors);var text=displays[i].GetComponentInChildren<TextMesh>();text.text=((PrototypeSpecies)(i==0?state.SpeciesA:state.SpeciesB)).ToString();}
   for(int i=0;i<logs.Length;i++)logs[i].gameObject.SetActive((state.Picked&(1<<i))==0);
   reservoir.SetLevel(state.WaterLevel);int progress=Progress;
   if(lastProgress!=progress){lastProgress=progress;for(int i=0;i<installedLogs.Length;i++)installedLogs[i].SetActive(i<progress);damMarker.GetPropertyBlock(colors);colors.SetColor("_BaseColor",progress>=requiredLogs?new Color(.1f,1,.2f,.35f):new Color(1,.08f,.05f,.35f));damMarker.SetPropertyBlock(colors);damLabel.text=progress>=requiredLogs?"Dam complete":$"Add Log ({progress}/{requiredLogs})";}
  }
  private void Update(){for(int i=0;i<2;i++){double remaining=(i==0?State.BusyA:State.BusyB)-Now;displayVisuals[i].transform.localScale=Vector3.one*Mathf.Lerp(1,.25f,Mathf.Clamp01((float)(remaining/swapDuration)));}}
 }
}





