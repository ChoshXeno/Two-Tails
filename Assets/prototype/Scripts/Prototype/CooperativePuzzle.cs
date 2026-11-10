using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
namespace TwoTails.Prototype
{
 // One rule engine for offline and server-authoritative online puzzles. No client prediction of puzzle state.
 public sealed class CooperativePuzzle:MonoBehaviour
 {
  [SerializeField,Tooltip("Shared deterministic puzzle rule selection.")] private CooperativePuzzleKind kind;
  [SerializeField,Tooltip("Online scene: only the server samples and advances rules.")] private bool networked;
  [SerializeField] private BoxCollider plateA,plateB,resetPad;
  [SerializeField] private Transform movingA,movingB;
  [SerializeField] private Transform exitGate;
  [SerializeField] private Transform rampA,rampB;
  [SerializeField] private Renderer plateVisualA,plateVisualB;
  [SerializeField] private CooperativeWeightBox[] boxes;
  [SerializeField] private TextMesh status;
  [SerializeField,Min(.05f),Tooltip("Door/bridge travel speed in normalized units per second.")] private float travelSpeed=1.7f;
  [SerializeField,Min(.1f),Tooltip("Counterweight range above/below the boarding decks, metres.")] private float liftRange=.35f;
  [SerializeField,Min(.1f),Tooltip("Controlled platform speed in metres per second.")] private float liftSpeed=.25f;
  [SerializeField,Min(.1f),Tooltip("Stand on the reset tile this long; leave before requesting another reset.")] private float resetHold=2;
  [SerializeField,Min(1),Tooltip("Equal load required on each lift before locking.")] private int targetWeight=4;
  [SerializeField,Min(.1f),Tooltip("Server-authoritative crate push metres/second.")] private float pushSpeed=2;
  public bool Networked=>networked;
  public CooperativePuzzleNetwork Network {get;private set;}
  public CooperativePuzzleState State {get;private set;}
  public CooperativePuzzleKind Kind=>kind;
  private Vector3 restA,restB,restExit; private Vector3[] boxRest;
  private readonly HashSet<PrototypeAnimal> a=new(),b=new(),reset=new();
  private readonly HashSet<PrototypeMotor> carried=new();
  private readonly Collider[] overlaps=new Collider[64];
  private readonly Dictionary<PrototypeMotor,float> pushTimes=new();
  private PrototypeAnimal first;
  private float resetTimer;private bool resetArmed=true;
  private MaterialPropertyBlock color;
  private string lastStatus;
  public bool Authority=>!networked||(Network!=null&&Network.IsSpawned&&Network.IsServer);
  private void Awake()
  {
   Network=GetComponent<CooperativePuzzleNetwork>();restA=movingA.localPosition;restB=movingB.localPosition;if(exitGate!=null)restExit=exitGate.localPosition;
   boxRest=new Vector3[boxes.Length];for(int i=0;i<boxes.Length;i++){boxRest[i]=boxes[i].transform.localPosition;boxes[i].Bind(this,i);}
   color=new MaterialPropertyBlock();if(!networked)ResetPuzzle(false);
  }
  private bool Valid(PrototypeAnimal animal)
  {
   if(animal==null||!animal.isActiveAndEnabled)return false;
   if(networked)return NetworkPuzzleOccupancy.IsCurrentPlayer(NetworkManager.Singleton,animal.GetComponent<NetworkPlayerPrototype>());
   return animal.TryGetComponent<PrototypeMotor>(out var motor)&&motor.isActiveAndEnabled;
  }
  private void Sample(BoxCollider zone,HashSet<PrototypeAnimal> result)
  {
   result.Clear();var center=zone.transform.TransformPoint(zone.center);
   int count=Physics.OverlapBoxNonAlloc(center,Vector3.Scale(zone.size*.5f,zone.transform.lossyScale),overlaps,zone.transform.rotation,~0,QueryTriggerInteraction.Collide);
   for(int i=0;i<count;i++)
   {
    var animal=overlaps[i].GetComponentInParent<PrototypeAnimal>();if(!Valid(animal))continue;
    // Feet, not torso, must occupy the tile. Jumping removes weight immediately.
    Vector3 p=zone.transform.InverseTransformPoint(animal.transform.position)-zone.center;
    Vector3 h=zone.size*.5f;
    if(Mathf.Abs(p.x)<=h.x&&Mathf.Abs(p.z)<=h.z&&Mathf.Abs(p.y)<=h.y)result.Add(animal);
   }
  }
  private void FixedUpdate()
  {
   if(!Authority)return;
   Physics.SyncTransforms();Sample(resetPad,reset);
   if(reset.Count==0){resetArmed=true;resetTimer=0;}else if(resetArmed){resetTimer+=Time.fixedDeltaTime;if(resetTimer>=resetHold){ResetPuzzle(true);resetArmed=false;return;}}
   Sample(plateA,a);Sample(plateB,b);
   var next=State;next.Plates=(byte)((a.Count>0?1:0)|(b.Count>0?2:0));
   if(first!=null&&!Valid(first)){ResetPuzzle(true);return;}
   if(kind!=CooperativePuzzleKind.Counterweight)
   {
    if(first==null&&a.Count>0)foreach(var player in a){first=player;break;}
    if(!next.Latched&&first!=null&&a.Contains(first))foreach(var player in b)if(player!=first)next.Latched=true;
    float targetA=kind==CooperativePuzzleKind.Switchback?(next.Latched?1:0):(next.Latched||a.Count>0?1:0);
    float targetB=next.Latched?1:(kind==CooperativePuzzleKind.Switchback&&a.Count>0?1:0);
    next.A=Advance(State.A,targetA,movingA);next.B=Advance(State.B,targetB,movingB);
   }
   else
   {
    next.WeightA=Weight(a,plateA);next.WeightB=Weight(b,plateB);
    bool different=false;foreach(var x in a)foreach(var y in b)if(x!=y)different=true;
    if(next.WeightA==targetWeight&&next.WeightB==targetWeight&&different)next.Latched=true;
    float target=next.Latched?0:Mathf.Sign(next.WeightB-next.WeightA)*liftRange;
    next.A=Mathf.MoveTowards(State.A,target,liftSpeed*Time.fixedDeltaTime);next.B=-next.A;
   }
   // Disconnect invalidates a latched cooperative attempt; never retain stale occupants.
   if(networked&&next.Latched&&NetworkManager.Singleton.ConnectedClients.Count<2){ResetPuzzle(true);return;}
   Apply(next);SettleBoxes();next=State;ReadBoxes(ref next);State=next;
   if(networked)Network.Publish(State);
  }
  private float Advance(float value,float target,Transform panel)
  {
   if(target<value&&kind==CooperativePuzzleKind.Switchback)
   {
    // Keep the full gate corridor open while any valid player occupies it.
    var col=panel.GetComponent<BoxCollider>();var bounds=col.bounds;
    int n=Physics.OverlapBoxNonAlloc(new Vector3(bounds.center.x,transform.position.y+1,bounds.center.z),new Vector3(bounds.extents.x+.2f,1.1f,.8f),overlaps,Quaternion.identity,~0,QueryTriggerInteraction.Collide);
    for(int i=0;i<n;i++)if(Valid(overlaps[i].GetComponentInParent<PrototypeAnimal>()))return value;
   }
   if(kind==CooperativePuzzleKind.Bridge&&target<value)
   {
    var bounds=panel.GetComponent<BoxCollider>().bounds;
    int n=Physics.OverlapBoxNonAlloc(bounds.center+Vector3.up, bounds.extents+Vector3.up,overlaps,Quaternion.identity,~0,QueryTriggerInteraction.Collide);
    for(int i=0;i<n;i++)if(Valid(overlaps[i].GetComponentInParent<PrototypeAnimal>()))return value;
   }
   return Mathf.MoveTowards(value,target,travelSpeed*Time.fixedDeltaTime);
  }
  private int Weight(HashSet<PrototypeAnimal> players,BoxCollider zone)
  {
   int total=0;foreach(var p in players)total+=p.Weight;
   foreach(var box in boxes){var p=zone.transform.InverseTransformPoint(box.transform.position);var h=zone.size*.5f;if(Mathf.Abs(p.x-zone.center.x)<h.x&&Mathf.Abs(p.z-zone.center.z)<h.z&&Mathf.Abs(p.y-.5f)<.22f)total+=box.Weight;}
   return total;
  }
  public void Apply(CooperativePuzzleState value)
  {
   Vector3 pa=restA+Vector3.up*(kind==CooperativePuzzleKind.Switchback?value.A*4:kind==CooperativePuzzleKind.Bridge?(value.A-1)*5:value.A);
   Vector3 pb=restB+Vector3.up*(kind==CooperativePuzzleKind.Switchback?value.B*4:kind==CooperativePuzzleKind.Bridge?(value.B-1)*5:value.B);
   if(kind==CooperativePuzzleKind.Counterweight){Carry(movingA,pa);Carry(movingB,pb);}
   movingA.localPosition=pa;movingB.localPosition=pb;if(kind==CooperativePuzzleKind.Counterweight){Ramp(rampA,value.A);Ramp(rampB,value.B);}if(exitGate!=null)exitGate.localPosition=restExit+Vector3.up*(value.Latched?4:0);State=value;
   if(networked&&!Authority){if(boxes.Length>0)boxes[0].transform.localPosition=value.Box0;if(boxes.Length>1)boxes[1].transform.localPosition=value.Box1;if(boxes.Length>2)boxes[2].transform.localPosition=value.Box2;}
   Paint(plateVisualA,(value.Plates&1)!=0);Paint(plateVisualB,(value.Plates&2)!=0);
   string text=kind==CooperativePuzzleKind.Counterweight?$"Load: {value.WeightA} / {value.WeightB}\n{(value.Latched?"Balanced - Locked":"Target: 4 / 4")}":value.Latched?"Handoff locked - Meet at exit":"Hold A while partner reaches B";
   if(text!=lastStatus){status.text=text;lastStatus=text;}
  }
  private void Ramp(Transform ramp,float height){if(ramp==null)return;var p=ramp.localPosition;p.y=height*.5f-.1f;p.z=-.5f;ramp.localPosition=p;ramp.localRotation=Quaternion.Euler(-Mathf.Atan2(height,2)*Mathf.Rad2Deg,0,0);ramp.localScale=new Vector3(5,.2f,Mathf.Sqrt(4+height*height)+.2f);}
  private void Carry(Transform deck,Vector3 target)
  {
   var bounds=deck.GetComponent<BoxCollider>().bounds;Vector3 delta=deck.parent.TransformVector(target-deck.localPosition);
   int n=Physics.OverlapBoxNonAlloc(bounds.center+Vector3.up*.5f,bounds.extents+Vector3.up*.5f,overlaps,Quaternion.identity,~0,QueryTriggerInteraction.Collide);
   carried.Clear();
   for(int i=0;i<n;i++){var m=overlaps[i].GetComponentInParent<PrototypeMotor>();if(m!=null&&m.isActiveAndEnabled&&Mathf.Abs(m.transform.position.y-bounds.max.y)<.22f&&carried.Add(m))m.Ride(delta);}
   if(Authority)foreach(var box in boxes)if(Mathf.Abs(box.transform.position.y-.5f-bounds.max.y)<.12f&&Mathf.Abs(box.transform.position.x-bounds.center.x)<bounds.extents.x&&Mathf.Abs(box.transform.position.z-bounds.center.z)<bounds.extents.z)box.transform.position+=delta;
  }
  private void Paint(Renderer r,bool active){if(r==null)return;color.SetColor("_BaseColor",active?Color.green:new Color(1,.7f,.08f));r.SetPropertyBlock(color);}
  private void ReadBoxes(ref CooperativePuzzleState s){if(boxes.Length>0)s.Box0=boxes[0].transform.localPosition;if(boxes.Length>1)s.Box1=boxes[1].transform.localPosition;if(boxes.Length>2)s.Box2=boxes[2].transform.localPosition;}
  public void ClearSession(){first=null;a.Clear();b.Clear();reset.Clear();pushTimes.Clear();}
  public void ResetPuzzle(bool movePlayers)
  {
   if(!Authority&&networked)return;
   if(movePlayers)
   {
    int i=0;foreach(var animal in FindObjectsByType<PrototypeAnimal>())if(Valid(animal)&&Mathf.Abs(animal.transform.position.x-transform.position.x)<9 && animal.transform.position.z-transform.position.z>-7 && animal.transform.position.z-transform.position.z<30)
    {var safe=transform.position+new Vector3(-2+i++*3,.1f,-5);if(networked)Network.Warp(animal.GetComponent<NetworkPlayerPrototype>(),safe);else animal.GetComponent<PrototypeMotor>().Warp(safe);}
   }
   ClearSession();var next=new CooperativePuzzleState{Revision=State.Revision+1};
   for(int i=0;i<boxes.Length;i++)boxes[i].transform.localPosition=boxRest[i];ReadBoxes(ref next);Apply(next);if(networked)Network.Publish(State);
  }
  public void TryPush(int index,PrototypeMotor motor)
  {
   if(!Authority||index<0||index>=boxes.Length||motor==null||!Valid(motor.GetComponent<PrototypeAnimal>()))return;
   if(pushTimes.TryGetValue(motor,out float time)&&Time.time-time<.045f)return;pushTimes[motor]=Time.time;
   var box=boxes[index];Vector3 to=box.transform.position-motor.transform.position;to.y=0;
   if(to.magnitude>1.7f||Vector3.Dot(motor.Facing.forward,to.normalized)<.35f)return;
   Vector3 dir=Vector3.ProjectOnPlane(motor.Facing.forward,Vector3.up).normalized;float step=pushSpeed*.05f;
   var col=box.GetComponent<BoxCollider>();
   var next=box.transform.position+dir*step;
   if(Physics.Raycast(next+dir*.45f+Vector3.up,Vector3.down,out var support,3,~((1<<7)|(1<<2)),QueryTriggerInteraction.Ignore))
   {float supported=support.point.y+.5f;if(supported-next.y>.3f)return;next.y=Mathf.Max(next.y,supported);}
   int contacts=Physics.OverlapBoxNonAlloc(next,col.bounds.extents*.9f,overlaps,box.transform.rotation,~(1<<2),QueryTriggerInteraction.Ignore);
   for(int i=0;i<contacts;i++)
   {var hit=overlaps[i];if(hit==col||hit.transform==rampA||hit.transform==rampB)continue;if(hit.bounds.max.y<=next.y-.45f)continue;return;}   if(Mathf.Abs(next.x-transform.position.x)>10||Mathf.Abs(next.z-transform.position.z)>7)return;
   box.transform.position=next;
  }
  private void SettleBoxes()
  {
   foreach(var box in boxes)
   {
    var p=box.transform.position;
    if(Physics.Raycast(p+Vector3.up,Vector3.down,out var hit,4,~(1<<7),QueryTriggerInteraction.Ignore))
    {float y=hit.point.y+.5f;if(y-p.y<.3f)p.y=Mathf.MoveTowards(p.y,y,3*Time.fixedDeltaTime);}
    else p.y-=3*Time.fixedDeltaTime;
    box.transform.position=p;
    if(p.y<transform.position.y-4)box.transform.localPosition=boxRest[box.Index];
   }
  }
 }
}




