using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEditor.Build.Profile;
using UnityEngine;
using UnityEngine.SceneManagement;
using TwoTails.Prototype;
namespace TwoTails.Editor
{
    public static class PrototypeTestBuild
    {
        public const string Menu="Assets/prototype/Scenes/Prototype_MainMenu.unity";
        public const string Local="Assets/prototype/Scenes/Prototype_SplitScreen.unity";
        public const string Online="Assets/prototype/Scenes/Prototype_OnlineTest.unity";
        static void Set(UnityEngine.Object target,string field,UnityEngine.Object value)
        { var s=new SerializedObject(target);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo(); }
        static void Flag(UnityEngine.Object target,string field,bool value)
        {var s=new SerializedObject(target);s.FindProperty(field).boolValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        [MenuItem("Tools/Two Tails/Create Test Entrances")]
        public static void Create()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save scene and exit Play Mode first.");
            string original=SceneManager.GetActiveScene().path;
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Menu)==null)
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                new GameObject("Main Menu").AddComponent<PrototypeTestMenu>();
                var camera=new GameObject("Menu Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(0.025f,0.04f,0.07f);
                EditorSceneManager.SaveScene(scene,Menu);
            }
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Local)==null)
            {
                AssetDatabase.CopyAsset("Assets/prototype/Scenes/Prototype_Greybox.unity",Local);
                var scene=EditorSceneManager.OpenScene(Local);
                var motors=UnityEngine.Object.FindObjectsByType<PrototypeMotor>(FindObjectsInactive.Include).OrderBy(m=>m.name=="DebugPlayer2"?1:0).ToArray();
                if(motors.Length!=2)throw new InvalidOperationException("Expected exactly two existing local motors.");
                foreach(var debug in UnityEngine.Object.FindObjectsByType<PrototypeDebugPlayers>())UnityEngine.Object.DestroyImmediate(debug);
                foreach(var canvas in UnityEngine.Object.FindObjectsByType<Canvas>())UnityEngine.Object.DestroyImmediate(canvas.gameObject);
                var first=UnityEngine.Object.FindAnyObjectByType<PrototypeFollowCamera>();
                var second=UnityEngine.Object.Instantiate(first); second.name="P2 Camera";second.tag="Untagged";
                var listener=second.GetComponent<AudioListener>();if(listener!=null)UnityEngine.Object.DestroyImmediate(listener);
                motors[0].name="P1 Fox";motors[1].name="P2 Monkey";motors[1].gameObject.SetActive(true);
                var root=new GameObject("Split Screen");var split=root.AddComponent<PrototypeSplitScreen>();
                var so=new SerializedObject(split);
                var ps=so.FindProperty("players");ps.arraySize=2;
                var cs=so.FindProperty("cameras");cs.arraySize=2;
                for(int i=0;i<2;i++){ps.GetArrayElementAtIndex(i).objectReferenceValue=motors[i];cs.GetArrayElementAtIndex(i).objectReferenceValue=i==0?first:second;}
                so.ApplyModifiedPropertiesWithoutUndo();
                Flag(root.AddComponent<PrototypeTestMenu>(),"mainMenu",false);
                EditorSceneManager.SaveScene(scene);
            }
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Online)==null)
            {
                AssetDatabase.CopyAsset("Assets/prototype/Scenes/Prototype_Network.unity",Online);
                var scene=EditorSceneManager.OpenScene(Online);
                Flag(new GameObject("Test Menu Return").AddComponent<PrototypeTestMenu>(),"mainMenu",false);
                EditorSceneManager.SaveScene(scene);
            }
            var profile=AssetDatabase.LoadAssetAtPath<BuildProfile>("Assets/Settings/Build Profiles/Windows Friends Test.asset");
            if(profile==null)
            {
                var t=typeof(BuildPipeline).Assembly.GetType("UnityEditor.BuildTargetDiscovery");
                var m=t.GetMethod("GetGUIDFromBuildTarget",System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static,null,new[]{typeof(BuildTarget)},null);
                profile=BuildProfile.CreateBuildProfile((GUID)m.Invoke(null,new object[]{BuildTarget.StandaloneWindows64}),"Windows Friends Test");
            }
            profile.overrideGlobalScenes=true; profile.scenes=new[]{new EditorBuildSettingsScene(Menu,true),new EditorBuildSettingsScene(Local,true),new EditorBuildSettingsScene(Online,true)};
            var settings=new SerializedObject(profile);settings.FindProperty("m_PlatformBuildProfile.m_Development").boolValue=true;settings.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(profile);AssetDatabase.SaveAssets();
            if(!string.IsNullOrEmpty(original))EditorSceneManager.OpenScene(original);
        }
        [MenuItem("Tools/Two Tails/Build Windows Friends Test")]
        public static void Build()
        {
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Menu,Local,Online},locationPathName="Builds/FriendsTest/TwoTails.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Friends build failed: "+report.summary.result);
            Debug.Log("Friends build succeeded, errors: "+report.summary.totalErrors);
        }
    }
}
