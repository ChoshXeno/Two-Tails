using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace TwoTails.Prototype
{
    internal static class PrototypeOnlineIdentity
    {
        private static Task initialization;
        private static Task signIn;
        private static string initializedEnvironment;
        private static string initializedProfile;

        internal static string Profile
        {
            get
            {
                string[] args = System.Environment.GetCommandLineArgs();
                int index = Array.IndexOf(args, "-authProfile");
                if (index >= 0)
                    return index + 1 < args.Length ? args[index + 1] : "";
                return Application.isEditor ? "twotails_editor" :
                    Debug.isDebugBuild ? "twotails_development" : "twotails";
            }
        }

        internal static Task InitializeAsync(string environment, string profile)
        {
            if (initialization != null && !initialization.IsFaulted && !initialization.IsCanceled)
            {
                if (environment != initializedEnvironment || profile != initializedProfile)
                    throw new InvalidOperationException("Restart to change the services environment or profile.");
                return initialization;
            }

            if (UnityServices.State != ServicesInitializationState.Uninitialized)
                throw new InvalidOperationException("Services were already initialized outside the prototype.");

            var options = new InitializationOptions().SetEnvironmentName(environment).SetProfile(profile);
            initializedEnvironment = environment;
            initializedProfile = profile;
            initialization = UnityServices.InitializeAsync(options);
            return initialization;
        }

        internal static Task SignInAsync()
        {
            if (AuthenticationService.Instance.IsSignedIn)
                return Task.CompletedTask;
            if (signIn == null || signIn.IsCompleted)
                signIn = AuthenticationService.Instance.SignInAnonymouslyAsync();
            return signIn;
        }

        internal static string ShortPlayerId
        {
            get
            {
                string id = AuthenticationService.Instance.PlayerId;
                return string.IsNullOrEmpty(id) ? "" : id.Substring(0, Math.Min(id.Length, 8));
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            initialization = signIn = null;
            initializedEnvironment = initializedProfile = null;
        }
    }
}
