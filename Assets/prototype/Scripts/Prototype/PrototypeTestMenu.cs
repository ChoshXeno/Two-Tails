using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
namespace TwoTails.Prototype
{
    public sealed class PrototypeTestMenu : MonoBehaviour
    {
        public const string MenuScene = "Prototype_MainMenu";
        public const string LocalScene = "Prototype_SplitScreen";
        public const string OnlineScene = "Prototype_OnlineTest";
        public static bool LocalAllowed => Application.isEditor || Debug.isDebugBuild;
        [SerializeField, Tooltip("Show the full main menu; otherwise provide a clean return action.")] private bool mainMenu = true;
        private GameObject page;
        private bool leaving;
        private void Start()
        {
            if (EventSystem.current == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            EventSystem.current.sendNavigationEvents = false;
            if (mainMenu) ShowMain();
            else
            {
                page = CanvasRoot("Return Menu");
                ButtonAt(page.transform, "Main Menu", new Vector2(0.92f, 0.94f), () => { if (!leaving) StartCoroutine(Return()); }, new Vector2(220,70));
            }
        }
        public void ShowMain()
        {
            if (page != null) Destroy(page);
            page = CanvasRoot("Main Menu");
            TextAt(page.transform, "Two Tails", new Vector2(0.5f,0.8f), new Vector2(800,100), 48);
            if (LocalAllowed) ButtonAt(page.transform,"Local Play (Split Screen)",new Vector2(0.5f,0.62f),ShowWarning);
            ButtonAt(page.transform,"Online Play",new Vector2(0.5f,0.45f),()=>SceneManager.LoadScene(OnlineScene));
            ButtonAt(page.transform,"Quit",new Vector2(0.5f,0.28f),Application.Quit);
            TextAt(page.transform,"Build " + Application.version,new Vector2(0.5f,0.08f),new Vector2(800,60),22);
        }
        public void ShowWarning()
        {
            if (!LocalAllowed) return;
            Destroy(page); page=CanvasRoot("Local Notice");
            TextAt(page.transform,"For a better experience, please use a tablet.\nCurrent developer test: shared keyboard.",new Vector2(0.5f,0.65f),new Vector2(1400,200),32);
            ButtonAt(page.transform,"Continue",new Vector2(0.5f,0.43f),EnterLocal);
            ButtonAt(page.transform,"Back",new Vector2(0.5f,0.26f),ShowMain);
        }
        public void EnterLocal() { if(LocalAllowed) SceneManager.LoadScene(LocalScene); }
        private IEnumerator Return()
        {
            leaving=true;
            var online=FindAnyObjectByType<PrototypeOnlineRoom>();
            var network=FindAnyObjectByType<PrototypeNetworkSession>();
            if(network!=null && network.CanDisconnect) network.Disconnect();
            if(online!=null)
            {
                if(online.CanCancel) online.Cancel();
                if(online.CanLeave) { var task=online.LeaveAsync(); while(!task.IsCompleted) yield return null; }
                while(!online.CanSwitchMode) yield return null;
            }
            if(network!=null) { network.Disconnect(); while(!network.NetworkIdle) yield return null; }
            var manager=Unity.Netcode.NetworkManager.Singleton;
            if(manager!=null) Destroy(manager.gameObject);
            yield return null;
            SceneManager.LoadScene(MenuScene);
        }
        public static GameObject CanvasRoot(string name)
        {
            var obj=new GameObject(name,typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var c=obj.GetComponent<Canvas>(); c.renderMode=RenderMode.ScreenSpaceOverlay; c.sortingOrder=20;
            var s=obj.GetComponent<CanvasScaler>(); s.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution=new Vector2(1920,1080); s.matchWidthOrHeight=0.5f;
            return obj;
        }
        public static RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 size)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=anchor; r.sizeDelta=size; return r;
        }
        public static Text TextAt(Transform parent,string text,Vector2 anchor,Vector2 size,int fontSize=26)
        {
            var t=Rect(text,parent,anchor,size).gameObject.AddComponent<Text>();
            t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=text;t.fontSize=fontSize;
            t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;return t;
        }
        public static void ButtonAt(Transform parent,string text,Vector2 anchor,UnityEngine.Events.UnityAction action,Vector2? size=null)
        {
            var r=Rect(text,parent,anchor,size??new Vector2(600,120));
            r.gameObject.AddComponent<Image>().color=new Color(0.1f,0.25f,0.4f,0.95f);
            var b=r.gameObject.AddComponent<Button>(); b.onClick.AddListener(action);b.navigation=new Navigation{mode=Navigation.Mode.None};
            var label=TextAt(r,text,Vector2.one*0.5f,r.sizeDelta); label.rectTransform.anchorMin=Vector2.zero;label.rectTransform.anchorMax=Vector2.one;label.rectTransform.sizeDelta=Vector2.zero;
        }
    }
}
