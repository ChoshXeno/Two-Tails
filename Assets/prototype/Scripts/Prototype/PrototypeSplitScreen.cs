using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
namespace TwoTails.Prototype
{
    [DefaultExecutionOrder(-200)]
    public sealed class PrototypeSplitScreen : MonoBehaviour
    {
        [SerializeField, Tooltip("Stable offline slots: index zero is Fox; index one is Monkey.")] private PrototypeMotor[] players;
        [SerializeField, Tooltip("Independent cameras in player-slot order.")] private PrototypeFollowCamera[] cameras;
        [SerializeField, Tooltip("Normalized viewports. UI and input surfaces use the same rectangles. Opposed rotation is deferred.")] private Rect[] viewports={new Rect(0,0,0.5f,1),new Rect(0.5f,0,0.5f,1)};
        [SerializeField, Tooltip("P2 look up key.")] private Key lookUp=Key.Numpad8;
        [SerializeField, Tooltip("P2 look down key.")] private Key lookDown=Key.Numpad2;
        [SerializeField, Tooltip("P2 look left key.")] private Key lookLeft=Key.Numpad4;
        [SerializeField, Tooltip("P2 look right key.")] private Key lookRight=Key.Numpad6;
        [SerializeField, Tooltip("P2 zoom in key.")] private Key zoomIn=Key.Numpad9;
        [SerializeField, Tooltip("P2 zoom out key.")] private Key zoomOut=Key.Numpad3;
        [SerializeField, Min(0), Tooltip("P2 orbit speed in degrees per second.")] private float lookSpeed=90;
        [SerializeField, Min(0), Tooltip("P2 zoom speed in metres per second.")] private float zoomSpeed=3;
        private bool focused,paused;
        private void Awake()
        {
            if(!PrototypeTestMenu.LocalAllowed)
            {
                foreach(var p in players) p.gameObject.SetActive(false);
                UnityEngine.SceneManagement.SceneManager.LoadScene(PrototypeTestMenu.MenuScene); enabled=false; return;
            }
            focused=Application.isFocused;
            for(int i=0;i<2;i++)
            {
                players[i].gameObject.SetActive(true);
                players[i].SetLocalContext(cameras[i].transform);
                cameras[i].Bind(players[i].transform,players[i].Facing);
                cameras[i].GetComponent<Camera>().rect=viewports[i];
                cameras[i].GetComponent<Camera>().ResetAspect();
            }
        }
        private void OnApplicationFocus(bool value){focused=value;}
        private void OnApplicationPause(bool value){paused=value;}
        private void Start()
        {
            if(!enabled)return;
            EventSystem.current.sendNavigationEvents=false;
            var canvas=PrototypeTestMenu.CanvasRoot("Split HUD");
            for(int i=0;i<2;i++)
            {
                var viewport=PrototypeTestMenu.Rect("Player "+(i+1),canvas.transform,Vector2.zero,Vector2.zero);
                viewport.anchorMin=viewports[i].min; viewport.anchorMax=viewports[i].max;viewport.offsetMin=viewport.offsetMax=Vector2.zero;
                viewport.gameObject.AddComponent<RectMask2D>();
                if(i==0)
                {
                    var surface=PrototypeTestMenu.Rect("P1 Orbit Surface",viewport,Vector2.zero,Vector2.zero);
                    surface.anchorMax=Vector2.one;surface.offsetMin=surface.offsetMax=Vector2.zero;
                    surface.gameObject.AddComponent<Image>().color=Color.clear;
                    surface.gameObject.AddComponent<PrototypeOrbitDrag>().Configure(cameras[i]);
                }
                PrototypeTestMenu.TextAt(viewport,i==0?"P1 Fox":"P2 Monkey",new Vector2(0.5f,0.94f),new Vector2(450,60),30);
                var text=PrototypeTestMenu.TextAt(viewport,"",new Vector2(0.5f,0.84f),new Vector2(700,80));
                text.gameObject.AddComponent<PrototypeInteractionPrompt>().Configure(players[i].GetComponent<PrototypeInteractor>(),i==1);
                PrototypeTestMenu.TextAt(viewport,i==0?"WASD / E | Drag / Wheel":"Arrows / 0 | Numpad 8 2 4 6 / 9 3",new Vector2(0.5f,0.07f),new Vector2(700,60),22);
            }
        }
        private void Update()
        {
            if(!focused||paused||!PrototypeTestMenu.LocalAllowed)return;
            if(EventSystem.current?.currentSelectedGameObject?.GetComponent<InputField>()?.isFocused==true)return;
            var keyboard=Keyboard.current;
            if(keyboard==null)return;
            float Held(Key key)=>key!=Key.None && keyboard[key].isPressed?1f:0f;
            cameras[1].AddOrbitDegrees(new Vector2(Held(lookRight)-Held(lookLeft),Held(lookUp)-Held(lookDown))*lookSpeed*Time.deltaTime);
            cameras[1].AddZoomMetres((Held(zoomIn)-Held(zoomOut))*zoomSpeed*Time.deltaTime);
        }
    }
}
