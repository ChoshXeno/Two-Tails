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
 }
}
