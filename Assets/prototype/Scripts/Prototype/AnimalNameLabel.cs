using UnityEngine;
namespace TwoTails.Prototype
{
 public sealed class AnimalNameLabel:MonoBehaviour
 {
  private PrototypeAnimal animal;private string prefix;private PrototypeSpecies? previous;
  public void Configure(PrototypeAnimal target,string label){animal=target;prefix=label;}
  private void Update(){if(animal==null||previous==animal.Species)return;previous=animal.Species;GetComponent<UnityEngine.UI.Text>().text=prefix+animal.Species;}
 }
}
