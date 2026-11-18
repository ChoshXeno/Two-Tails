using UnityEngine;
using UnityEngine.UI;
namespace TwoTails.Prototype
{
 public sealed class PrototypeSwimmingHud:MonoBehaviour
 {
  [SerializeField] private PrototypeNetworkSession session;
  [SerializeField] private Text jumpLabel;
  [SerializeField] private GameObject surface;
  private void Update()
  {
   var player=session!=null?session.LocalPlayer:null;
   bool swimming=player!=null&&player.GetComponent<PrototypeSwimmer>()?.IsSwimming==true;
   if(jumpLabel!=null)jumpLabel.text=swimming?"Dive":"Jump";
   if(surface!=null&&surface.activeSelf!=swimming)surface.SetActive(swimming);
  }
 }
}
