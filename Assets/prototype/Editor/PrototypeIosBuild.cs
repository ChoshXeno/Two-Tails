using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TwoTails.Editor
{
    public static class PrototypeIosBuild
    {
        [MenuItem("Tools/Two Tails/Export iOS Prototype")]
        public static void Export()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before exporting.");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetArchitecture(NamedBuildTarget.iOS, 1);
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
                throw new InvalidOperationException("Install iOS Build Support for this Editor.");
            string output = Path.GetFullPath("Builds/iOS/Prototype");
            Directory.CreateDirectory(output);
            var previousTarget = EditorUserBuildSettings.activeBuildTarget;
            var previousGroup = BuildPipeline.GetBuildTargetGroup(previousTarget);
            var preloaded = PlayerSettings.GetPreloadedAssets();
            try
            {
                BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { PrototypeNetworkBuilder.NetworkScene },
                    locationPathName = output,
                    target = BuildTarget.iOS,
                    options = BuildOptions.Development
                });
                Debug.Log($"iOS export: {report.summary.result}, errors: {report.summary.totalErrors}");
                if (report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException("iOS export did not succeed. Inspect the build report.");
            }
            finally
            {
                PlayerSettings.SetPreloadedAssets(preloaded);
                if (EditorUserBuildSettings.activeBuildTarget != previousTarget)
                    EditorUserBuildSettings.SwitchActiveBuildTarget(previousGroup, previousTarget);
            }
        }
    }
}
