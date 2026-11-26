using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace TwoTails.Prototype
{
 public sealed partial class AnimalMechanicsWorld:MonoBehaviour
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
  [SerializeField, Tooltip("Optional replacement prefab per installed-log slot. Keep slot order stable across peers.")]
  private GameObject[] slotPrefabs;
  private DamSlotPresentation[] slots;
  private Mesh outlineMesh;
  private Material outlineMaterial;
  private PrototypeNightEnvironment night;
  internal PrototypeNightEnvironment Night => night;
  public const int BuildTarget = 20;
  private const int ReclaimStart = 40;
  public int RequiredLogs => slots?.Length ?? 0;
  public float WaterTarget => RequiredLogs > 0 && Progress == RequiredLogs ? reservoir.TargetLevel : reservoir.InitialLevel;
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
  private ulong displayedSlots = ulong.MaxValue;
  public AnimalMechanicsState State {get;private set;}
  public bool Authority=>!networked||(net!=null&&net.IsSpawned&&net.IsServer);
  public double Now=>networked&&net!=null&&net.IsSpawned?net.NetworkManager.ServerTime.Time:Time.timeAsDouble;
  public int Progress => State.Progress;
  public static int Count(int mask){int n=0;for(;mask!=0;mask&=mask-1)n++;return n;}
  private void Awake(){colors=new MaterialPropertyBlock();if(!(Application.isEditor||Debug.isDebugBuild)&&resetPoint!=null)resetPoint.gameObject.SetActive(false);Instance=this;net=GetComponent<AnimalMechanicsNetwork>();startA=displays[0].position;startB=displays[1].position;ConfigureExtendedCourse();ConfigureSlots();if(!networked)Initialize();}
  private void OnDestroy()
  {
   if (Instance == this) Instance = null;
   if (courseBlue != null) Destroy(courseBlue);
   if (outlineMesh != null) Destroy(outlineMesh);
   if (outlineMaterial != null) Destroy(outlineMaterial);
  }

  private void ConfigureSlots()
  {
   night = GetComponentInParent<PrototypeNightEnvironment>() ?? FindAnyObjectByType<PrototypeNightEnvironment>();
   outlineMesh = DamSlotPresentation.CreateBoxOutline();
   outlineMaterial = new Material(damMarker.sharedMaterial) { name = "Dam slot outline" };
   outlineMaterial.SetColor("_BaseColor", Color.white);
   damMarker.gameObject.SetActive(false);
   var configured = new List<DamSlotPresentation>();
   for (int index = 0; index < installedLogs.Length && configured.Count < AnimalMechanicsState.SlotLimit; index++)
   {
    var anchor = installedLogs[index];
    if (anchor == null) continue;
    var prefab = slotPrefabs != null && index < slotPrefabs.Length ? slotPrefabs[index] : null;
    configured.Add(new DamSlotPresentation(anchor, prefab, outlineMaterial, outlineMesh));
   }
   slots = configured.ToArray();
   if (slots.Length > Mathf.Min(logs.Length, AnimalMechanicsState.LogLimit))
    Debug.LogWarning("The dam has more slots than available logs.", this);
   ApplySlots();
  }

  private void ApplySlots()
  {
   if (displayedSlots == State.Slots) return;
   ulong previous = displayedSlots;
   displayedSlots = State.Slots;
   for (int index = 0; index < slots.Length; index++)
   {
    int shift = index * 4;
    if (((previous >> shift) & 15UL) != ((displayedSlots >> shift) & 15UL))
     slots[index].Apply(State.GetSlot(index) != 0);
   }
   damLabel.text = Progress == RequiredLogs ? "Dam complete" : $"Dam ({Progress}/{RequiredLogs})";
  }
  public void Initialize(){if(!Authority)return;Apply(new AnimalMechanicsState{SpeciesA=(byte)PrototypeSpecies.Otter,SpeciesB=(byte)PrototypeSpecies.Beaver,PositionA=startA,PositionB=startB,SpeciesC=(byte)PrototypeSpecies.Rabbit,SpeciesD=(byte)PrototypeSpecies.Elephant,SpeciesE=(byte)PrototypeSpecies.Monkey,PositionC=startC,PositionD=startD,PositionE=startE,WaterLevel=reservoir.InitialLevel});Publish();}
  private bool Valid(AnimalMechanicsActor a)=>a!=null&&a.isActiveAndEnabled&&(networked?(NetworkManager.Singleton!=null&&(NetworkManager.Singleton.IsServer?NetworkPuzzleOccupancy.IsCurrentPlayer(NetworkManager.Singleton,a.GetComponent<NetworkPlayerPrototype>()):a.GetComponent<NetworkPlayerPrototype>().IsSpawned&&a.GetComponent<NetworkPlayerPrototype>().IsOwner)):a.GetComponent<PrototypeMotor>().isActiveAndEnabled);
  private const int ExtraLogStart = 60;
  private int LogIndex(int id) => id >= ExtraLogStart ? id - ExtraLogStart + 10 : id >= 10 && id < 20 ? id - 10 : -1;
  private bool IsLog(int id) => LogIndex(id) >= 0 && LogIndex(id) < Mathf.Min(logs.Length, AnimalMechanicsState.LogLimit);
  private bool IsReclaim(int id) => id >= ReclaimStart && id < ReclaimStart + RequiredLogs;
  public bool IsPickup(int id) => IsLog(id) || IsReclaim(id);
  private bool IsBeaver(AnimalMechanicsActor actor) => actor != null &&
   actor.GetComponent<PrototypeAnimal>().Species == PrototypeSpecies.Beaver;

  private Transform Target(int id)
  {
   if (id >= 0 && id < displays.Length) return displays[id];
   if (id == StrikeTarget) return strikeBoard;
   if (id == BuildTarget) return dam;
   if (id == 30) return Application.isEditor || Debug.isDebugBuild ? resetPoint : null;
   if (IsReclaim(id)) return slots[id - ReclaimStart].Piece;
   if (IsLog(id)) return logs[LogIndex(id)];
   return null;
  }

  private Vector3 Point(int id)
  {
   if (id == StrikeTarget) return strikeBoard.position;
   if (IsReclaim(id)) return slots[id - ReclaimStart].Anchor.position;
   return Target(id).position + Vector3.up * (IsLog(id) ? .5f : 1f);
  }

  public int FindTarget(AnimalMechanicsActor actor)
  {
   if (!Valid(actor)) return -1;
   int best = -1;
   float nearest = float.PositiveInfinity;
   Vector3 origin = actor.transform.position + Vector3.up;
   bool beaver = IsBeaver(actor);
   for (int id = 0; id < ExtraLogStart + AnimalMechanicsState.LogLimit - 10; id++)
   {
    if (id == BuildTarget || (!beaver && IsPickup(id))) continue;
    var target = Target(id);
    if (target == null || !target.gameObject.activeInHierarchy) continue;
    float distance = (Point(id) - origin).sqrMagnitude;
    if (distance >= nearest || Check(actor, id) != null) continue;
    nearest = distance;
    best = id;
   }
   return best;
  }

  private string Check(AnimalMechanicsActor actor, int id)
  {
   var target = Target(id);
   if (!Valid(actor) || target == null || !target.gameObject.activeInHierarchy) return "Interaction unavailable";
   if (actor.Locked) return "Please wait";
   if (id == StrikeTarget && actor.GetComponent<PrototypeAnimal>().Species != PrototypeSpecies.Elephant) return "Only Elephant can strike";
   Vector3 origin = actor.transform.position + Vector3.up;
   Vector3 offset = Point(id) - origin;
   if (offset.magnitude > interactionDistance) return "Move closer";
   if (Vector3.Angle(Vector3.ProjectOnPlane(actor.GetComponent<PrototypeMotor>().Facing.forward, Vector3.up),
       Vector3.ProjectOnPlane(offset, Vector3.up)) > interactionAngle) return "Face the target";
   if (PrototypeInteractor.IsBlocked(origin, offset, offset.magnitude, actor.transform, target, ~0, sight))
    return "Interaction blocked";
   return null;
  }

  public bool GetBuildStatus(AnimalMechanicsActor actor, out string reason)
  {
   reason = null;
   if (!IsBeaver(actor) || !Valid(actor) ||
       (Point(BuildTarget) - (actor.transform.position + Vector3.up)).sqrMagnitude > interactionDistance * interactionDistance)
    return false;
   reason = BuildRejection(actor);
   return true;
  }

  private string BuildRejection(AnimalMechanicsActor actor)
  {
   if (!IsBeaver(actor)) return "Only Beaver can build the dam";
   if (Progress >= RequiredLogs) return "Dam complete";
   if (actor.HeldLogs == 0) return "Collect a log first";
   return Check(actor, BuildTarget);
  }

  public string Prompt(AnimalMechanicsActor actor, int id)
  {
   if (id >= 0 && id < displays.Length) return "Swap to " + DisplaySpecies(State,id);
   if (id == StrikeTarget) return "Strike";
   if (IsLog(id)) return "Pick up wood";
   if (IsReclaim(id)) return "Reclaim dam wood";
   return "Reset animal test";
  }
  public bool TryUse(AnimalMechanicsActor actor,int id,out string message)
  {
   if(!Authority){message="Waiting for host";return false;} Physics.SyncTransforms(); message=Check(actor,id);if(message!=null)return false;
   if(cooldowns.TryGetValue(actor,out double until)&&Now<until){message="Please wait";return false;}cooldowns[actor]=Now+.15;
   var species=actor.GetComponent<PrototypeAnimal>().Species;var next=State;
   if(id<displays.Length)
   {
    if(Now<DisplayBusy(next,id)){message="Display is busy";return false;}
    if(actor.Inventory>0){message="Use your logs before swapping";return false;}
    Vector3 from=actor.transform.position,to=displays[id].position;
    if(!lobby.bounds.Contains(from+Vector3.up)||!lobby.bounds.Contains(to+Vector3.up)){message="Swap only in the lobby";return false;}
    if(!Safe(to,actor.transform,displays[id])||!Safe(from,actor.transform,displays[id])){message="Swap position is blocked";return false;}
    var replacement=DisplaySpecies(next,id);
    if (replacement == species) { message = "Already playing " + species; return false; }
    foreach (var other in FindObjectsByType<AnimalMechanicsActor>())
     if (other != actor && Valid(other) && other.GetComponent<PrototypeAnimal>().Species == replacement)
     { message = replacement + " is in use"; return false; }
    SetDisplay(ref next,id,species,from,Now+swapDuration);
    if(networked)actor.GetComponent<NetworkPlayerPrototype>().Exchange(replacement,to);else{actor.GetComponent<PrototypeAnimal>().Apply(replacement);actor.Transition(to);}
    message="Now playing "+replacement;
   }
   else if (id == StrikeTarget)
   {
    next.BoardBroken = true;
    message = "Board broken";
   }
   else if (IsLog(id))
   {
    if (species != PrototypeSpecies.Beaver) { message = "Only Beaver can collect wood"; return false; }
    int bit = 1 << LogIndex(id);
    if ((next.Picked & bit) != 0) { message = "Wood already collected"; return false; }
    next.Picked |= bit;
    actor.SetHeldLogs(actor.HeldLogs | bit);
    message = "Wood collected";
   }
   else if (id == BuildTarget)
   {
    message = BuildRejection(actor);
    if (message != null) return false;
    int inventory = actor.HeldLogs;
    if (!next.TryBuild(RequiredLogs, ref inventory)) { message = "No available dam slot"; return false; }
    actor.SetHeldLogs(inventory);
    message = $"Built wood ({next.Progress}/{RequiredLogs})";
   }
   else if (IsReclaim(id))
   {
    if (species != PrototypeSpecies.Beaver) { message = "Only Beaver can reclaim wood"; return false; }
    int inventory = actor.HeldLogs;
    if (!next.TryReclaim(id - ReclaimStart, ref inventory)) { message = "Slot already empty"; return false; }
    actor.SetHeldLogs(inventory);
    message = $"Wood reclaimed ({next.Progress}/{RequiredLogs})";
   }   else{ResetTest();message="Animal test reset";return true;}
   next.Revision++;Apply(next);Publish();
   if(species==PrototypeSpecies.Beaver && (IsPickup(id) || id==BuildTarget))
   {
    var motion=id==BuildTarget ? PrototypeMotion.Wood_Place : PrototypeMotion.Wood_Pickup;
    if(networked)actor.GetComponent<PrototypeNetworkInteraction>().PlayWoodActionRpc(motion);
    else actor.GetComponent<PrototypeMotor>().ShowWoodAction(motion);
   }
   return true;
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
   // Player identities must be unique. A lobby display can show a species already
   // controlled by the other player; the server rejects exchanging into it.
   int used = 0;
   foreach(var player in FindObjectsByType<NetworkPlayerPrototype>())if(player!=joining&&player.IsSpawned)used|=1<<(int)player.GetComponent<PrototypeAnimal>().Species;
   if((used&(1<<(int)preferred))==0)return preferred;
   for(int i=0;i<6;i++)if((used&(1<<i))==0)return (PrototypeSpecies)i;
   return preferred;
  }
  public void ReturnLogs(int bits){if(!Authority)return;var s=State;if(bits==0)return;s.Picked&=~bits;s.Revision++;Apply(s);Publish();}
  public void ResetTest()
  {
   if(!Authority)return;var next=State;next.Picked=0;next.Slots=0;next.HeavyOffset=0;next.HeavyDrop=0;next.BoardBroken=false;pushActor=null;heavyVerticalVelocity=0;heavyFellAt=-1;next.WaterLevel=reservoir.InitialLevel;next.Revision++;cooldowns.Clear();
   int index=0;foreach(var actor in FindObjectsByType<AnimalMechanicsActor>())if(Valid(actor))
   {actor.SetHeldLogs(0);Vector3 safe=new Vector3(-1.5f+3*index++,.1f,-4);if(networked)actor.GetComponent<NetworkPlayerPrototype>().Exchange(actor.GetComponent<PrototypeAnimal>().Species,safe);else actor.Transition(safe);}
   Apply(next);Publish();
  }
  private void FixedUpdate()
  {
   if (!Authority) return;
   StepHeavyPush();
   StepHeavyFall();
   if (State.WaterLevel == WaterTarget) return;
   var next = State;
   next.WaterLevel = Mathf.MoveTowards(next.WaterLevel, WaterTarget, riseSpeed * Time.fixedDeltaTime);
   Apply(next);
   Publish();
  }
  private void Publish(){if(networked)net.Publish(State);}
  public void Apply(AnimalMechanicsState state)
  {
   var previous = State;
   State = state;
   ApplyCourseState();
   reservoir.SetLevel(state.WaterLevel);
   if (previous.Revision == state.Revision && previous.WaterLevel != state.WaterLevel &&
       previous.PositionA == state.PositionA && previous.PositionB == state.PositionB &&
       previous.SpeciesA == state.SpeciesA && previous.SpeciesB == state.SpeciesB &&
       previous.Picked == state.Picked && previous.Slots == state.Slots &&
       previous.PositionC == state.PositionC && previous.PositionD == state.PositionD &&
       previous.SpeciesC == state.SpeciesC && previous.SpeciesD == state.SpeciesD &&
       previous.PositionE == state.PositionE && previous.SpeciesE == state.SpeciesE) return;
   displays[0].position=state.PositionA;displays[1].position=state.PositionB;displays[2].position=state.PositionC;displays[3].position=state.PositionD;
   if(displays.Length>4)displays[4].position=state.PositionE;
   for(int i=0;i<displays.Length;i++){MonkeyPresentation.Show(displays[i].gameObject, displayVisuals[i].transform, displayVisuals[i], DisplaySpecies(state,i), PrototypeMotion.Idle, true);displayVisuals[i].GetPropertyBlock(colors);colors.SetColor("_BaseColor",PrototypeAnimal.ColorFor(DisplaySpecies(state,i)));displayVisuals[i].SetPropertyBlock(colors);var text=displays[i].GetComponentInChildren<TextMesh>();text.text=(DisplaySpecies(state,i)).ToString();}
   for(int i=0;i<Mathf.Min(logs.Length, AnimalMechanicsState.LogLimit);i++)if(logs[i]!=null)logs[i].gameObject.SetActive((state.Picked&(1<<i))==0);
   ApplySlots();
  }  private void Update(){for(int i=0;i<displays.Length;i++){double remaining=DisplayBusy(State,i)-Now;displayVisuals[i].transform.localScale=Vector3.one*Mathf.Lerp(1,.25f,Mathf.Clamp01((float)(remaining/swapDuration)));}}
 }
}
