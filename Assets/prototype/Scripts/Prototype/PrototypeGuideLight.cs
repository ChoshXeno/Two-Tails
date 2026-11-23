using UnityEngine;

namespace TwoTails.Prototype
{
    [RequireComponent(typeof(Light))]
    public sealed class PrototypeGuideLight : MonoBehaviour
    {
        private Light source;
        private GameObject bulb;
        private Material material;
        private void Start()
        {
            source=GetComponent<Light>();
            bulb=GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bulb.name="Guide Light Bulb";
            bulb.transform.SetParent(transform,false);
            bulb.transform.localScale=Vector3.one*.18f;
            Destroy(bulb.GetComponent<Collider>());
            material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            bulb.GetComponent<Renderer>().sharedMaterial=material;
        }
        private void LateUpdate()
        {
            if(bulb==null)return;
            bulb.SetActive(source!=null&&source.isActiveAndEnabled&&source.intensity>0);
            if(source!=null)material.SetColor("_BaseColor",source.color*2f);
        }
        private void OnDestroy(){if(material!=null)Destroy(material);if(bulb!=null)Destroy(bulb);}
    }
}
