using UnityEngine;
namespace TwoTails.Prototype
{
 [RequireComponent(typeof(BoxCollider))]
 public sealed class PrototypeClimbSurface : MonoBehaviour
 {
  [SerializeField, Tooltip("Outward front normal in local coordinates.")] private Vector3 front=Vector3.back;
  [SerializeField, Tooltip("Feet landing point on the top platform.")] private Transform landing;
  public Vector3 Normal => transform.TransformDirection(front).normalized;
  public Vector3 Landing => landing.position;
  public bool HasLanding => landing!=null;
  public Vector3 ConstrainWallDelta(Vector3 feet, Vector3 desired, float radius)
  {
   var box=GetComponent<BoxCollider>();
   Vector3 local=transform.InverseTransformPoint(feet+desired);
   Vector3 current=transform.InverseTransformPoint(feet);
   Vector3 normal=front.normalized;
   bool alongX=Mathf.Abs(normal.z)>=Mathf.Abs(normal.x);
   float half=(alongX?box.size.x:box.size.z)*.5f;
   float center=alongX?box.center.x:box.center.z;
   float localRadius=radius/Mathf.Max(.0001f,Mathf.Abs(alongX?transform.lossyScale.x:transform.lossyScale.z));
   float clamped=Mathf.Clamp(alongX?local.x:local.z,center-half+localRadius,center+half-localRadius);
   if(alongX)local.x=current.x+(clamped-current.x);else local.z=current.z+(clamped-current.z);
   return transform.TransformPoint(local)-feet;
  }
 }
}
