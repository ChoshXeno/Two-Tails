using UnityEngine;
namespace TwoTails.Prototype
{
 [RequireComponent(typeof(BoxCollider))]
 public sealed class PrototypeWaterVolume:MonoBehaviour
 {
  [SerializeField,Tooltip("World Y of the solid basin floor.")] private float bottom=-4;
  [SerializeField,Tooltip("Authoritative initial world water height.")] private float initialLevel=-.6f;
  [SerializeField,Tooltip("Authoritative target world water height.")] private float targetLevel=0;
  [SerializeField,Tooltip("Blue transparent volume; no solid collider.")] private Transform waterVisual;
  [SerializeField,Tooltip("Safe dry recovery point for non-swimming animals.")] private Transform safeShore;
  public static readonly System.Collections.Generic.List<PrototypeWaterVolume> Active=new();
  private void OnEnable()=>Active.Add(this); private void OnDisable()=>Active.Remove(this);
  public float Bottom=>bottom;public float InitialLevel=>initialLevel;public float TargetLevel=>targetLevel;
  public float Level {get;private set;}
  public Vector3 SafeShore=>safeShore.position;
  public Bounds Bounds=>GetComponent<BoxCollider>().bounds;
  private void Awake()=>SetLevel(initialLevel);
  public bool ContainsXZ(Vector3 p,float inset=0){var b=Bounds;return p.x>b.min.x+inset&&p.x<b.max.x-inset&&p.z>b.min.z+inset&&p.z<b.max.z-inset;}
  public void SetLevel(float level){Level=Mathf.Clamp(level,bottom+.05f,targetLevel);if(waterVisual==null)return;var p=waterVisual.position;p.y=(bottom+Level)*.5f;waterVisual.position=p;var size=waterVisual.localScale;size.y=Mathf.Max(.05f,Level-bottom);waterVisual.localScale=size;}
 }
}

