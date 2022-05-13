using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Communications;
using FilesChecker;
using UnityEngine;

namespace SIT.Tarkov.Core
{
    public static class PatchConstants
    {
        public static BindingFlags PrivateFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private static Type[] _eftTypes;
        public static Type[] EftTypes 
        { 
            get 
            { 
                if( _eftTypes == null)
                {
                    _eftTypes = typeof(AbstractGame).Assembly.GetTypes();
                }

                return _eftTypes; 
            } 
        }
        public static Type[] FilesCheckerTypes { get; private set; }
        public static Type LocalGameType { get; private set; }
        public static Type ExfilPointManagerType { get; private set; }
        public static Type BackendInterfaceType { get; private set; }
        public static Type SessionInterfaceType { get; private set; }

        /// <summary>
        /// A Key/Value dictionary of storing & obtaining an array of types by name
        /// </summary>
        public static readonly Dictionary<string, Type[]> TypesDictionary = new Dictionary<string, Type[]>();

        /// <summary>
        /// A Key/Value dictionary of storing & obtaining a type by name
        /// </summary>
        public static readonly Dictionary<string, Type> TypeDictionary = new Dictionary<string, Type>();

        /// <summary>
        /// A Key/Value dictionary of storing & obtaining a method by type and name
        /// </summary>
        public static readonly Dictionary<(Type, string), MethodInfo> MethodDictionary = new Dictionary<(Type, string), MethodInfo>();

        private static string _backendUrl;
        /// <summary>
        /// Method that returns the Backend Url (Example: https://127.0.0.1)
        /// </summary>
        public static string GetBackendUrl()
        {
            //return GClassXXX.Config.BackendUrl;
            if (_backendUrl == null)
            {
                try
                {
                    var ConfigInstance = EftTypes
                        .Where(type => type.GetField("DEFAULT_BACKEND_URL", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) != null)
                        .FirstOrDefault().GetProperty("Config", BindingFlags.Static | BindingFlags.Public).GetValue(null);
                    _backendUrl = HarmonyLib.Traverse.Create(ConfigInstance).Field("BackendUrl").GetValue() as string;
                }
                catch (Exception e)
                {
                }
                Logger.LogInfo(_backendUrl);
            }
            if (_backendUrl == null)
            {
                _backendUrl = "https://127.0.0.1";
            }
            return _backendUrl;
        }

        public static void DisplayMessageNotification(string message)
        {
            if (MessageNotificationType == null)
            {
                Logger.LogError("MessageNotificationType not found");
                return;
            }


            var o = MessageNotificationType.GetMethod("DisplayMessageNotification", BindingFlags.Static | BindingFlags.Public);
            if (o != null)
            { 
                o.Invoke("DisplayMessageNotification", new object[] { message, ENotificationDurationType.Default, ENotificationIconType.Default, null });
            }

        }

        public static ManualLogSource Logger { get; private set; }

        public static Type MessageNotificationType { get; private set; }

        static PatchConstants()
        {
            if(Logger == null)  
                Logger = BepInEx.Logging.Logger.CreateLogSource("SIT.Tarkov.Core.PatchConstants");

            //_ = nameof(ISession.GetPhpSessionId);

            //EftTypes = typeof(AbstractGame).Assembly.GetTypes();
            TypesDictionary.Add("EftTypes", EftTypes);

            FilesCheckerTypes = typeof(ICheckResult).Assembly.GetTypes();
            LocalGameType = EftTypes.Single(x => x.Name == "LocalGame");
            ExfilPointManagerType = EftTypes.Single(x => x.GetMethod("InitAllExfiltrationPoints") != null);
            BackendInterfaceType = EftTypes.Single(x => x.GetMethods().Select(y => y.Name).Contains("CreateClientSession") && x.IsInterface);
            SessionInterfaceType = EftTypes.Single(x => x.GetMethods().Select(y => y.Name).Contains("GetPhpSessionId") && x.IsInterface);
            MessageNotificationType = EftTypes.Single(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public).Select(y => y.Name).Contains("DisplayMessageNotification"));
            if(MessageNotificationType == null)
            {
                Logger.LogInfo("SIT.Tarkov.Core:PatchConstants():MessageNotificationType:Not Found");
            }

            //GetBackendUrl();
        }
    }
}
