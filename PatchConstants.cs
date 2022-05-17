using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Communications;
using FilesChecker;
using Newtonsoft.Json;
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
                    var ConfigInstance = Constants.Instance.TargetAssemblyTypes
                        .Where(type => type.GetField("DEFAULT_BACKEND_URL", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) != null)
                        .FirstOrDefault().GetProperty("Config", BindingFlags.Static | BindingFlags.Public).GetValue(null);
                    _backendUrl = HarmonyLib.Traverse.Create(ConfigInstance).Field("BackendUrl").GetValue() as string;
                }
                catch (Exception e)
                {
                    Logger.LogError("GetBackendUrl():" + e);
                }
                Logger.LogInfo(_backendUrl);
            }
            if (_backendUrl == null)
            {
                _backendUrl = "https://127.0.0.1";
                Logger.LogInfo("GetBackendUrl is defaulting to " + _backendUrl);

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
        public static Type GroupingType { get; }

        public static T DoSafeConversion<T>(object o)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(o));
        }

        public static PropertyInfo GetPropertyFromType(Type t, string name)
        {
            var properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo property in properties)
            {
                if (property.Name.ToLower().Contains(name.ToLower()))
                {
                    return property;
                }
            }
            properties = t.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (PropertyInfo property in properties)
            {
                if (property.Name.ToLower().Contains(name.ToLower()))
                {
                    return property;
                }
            }
            return null;
        }

        public static FieldInfo GetFieldFromType(Type t, string name)
        {
            var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                if (field.Name.ToLower().Contains(name.ToLower()))
                {
                    return field;
                }
            }
            fields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                if (field.Name.ToLower().Contains(name.ToLower()))
                {
                    return field;
                }
            }
            return null;
        }

        public static T GetFieldOrPropertyFromInstance<T>(object o, string name)
        {
            var properties = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo property in properties)
            {
                if (property.Name.ToLower().Contains(name.ToLower()))
                {
                    return Tarkov.Core.PatchConstants.DoSafeConversion<T>(property.GetValue(o));
                }
            }
            properties = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (PropertyInfo property in properties)
            {
                if (property.Name.ToLower().Contains(name.ToLower()))
                {
                    return Tarkov.Core.PatchConstants.DoSafeConversion<T>(property.GetValue(o));
                }
            }
            properties = o.GetType().GetProperties(BindingFlags.Static | BindingFlags.Public);
            foreach (PropertyInfo property in properties)
            {
                if (property.Name.ToLower().Contains(name.ToLower()))
                {
                    return Tarkov.Core.PatchConstants.DoSafeConversion<T>(property.GetValue(o));
                }
            }
            var fields = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                if (field.Name.ToLower().Contains(name.ToLower()))
                {
                    return Tarkov.Core.PatchConstants.DoSafeConversion<T>(field.GetValue(o));
                }
            }
            fields = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                if (field.Name.ToLower().Contains(name.ToLower()))
                {
                    return Tarkov.Core.PatchConstants.DoSafeConversion<T>(field.GetValue(o));
                }
            }
            fields = o.GetType().GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                if (field.Name.ToLower().Contains(name.ToLower()))
                {
                    return Tarkov.Core.PatchConstants.DoSafeConversion<T>(field.GetValue(o));
                }
            }
            return default(T);
        }

        public static void SetFieldOrPropertyFromInstance<T>(object o, string name, T v)
        {
            var properties = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo property in properties)
            {
                if (property.Name.ToLower().Contains(name.ToLower()))
                {
                    property.SetValue(o, v);
                }
            }
            properties = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (PropertyInfo property in properties)
            {
                if (property.Name.ToLower().Contains(name.ToLower()))
                {
                    property.SetValue(o, v);
                }
            }
            properties = o.GetType().GetProperties(BindingFlags.Static | BindingFlags.Public);
            foreach (PropertyInfo property in properties)
            {
                if (property.Name.ToLower().Contains(name.ToLower()))
                {
                    property.SetValue(o, v);
                }
            }
            var fields = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                if (field.Name.ToLower().Contains(name.ToLower()))
                {
                    field.SetValue(o, v);
                }
            }
            fields = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                if (field.Name.ToLower().Contains(name.ToLower()))
                {
                    field.SetValue(o, v);
                }
            }
            fields = o.GetType().GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                if (field.Name.ToLower().Contains(name.ToLower()))
                {
                    field.SetValue(o, v);
                }
            }
        }

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
            GroupingType = EftTypes.Single(x => x.GetMethods(BindingFlags.Public | BindingFlags.Static).Select(y => y.Name).Contains("CreateRaidPlayer"));
            if (GroupingType != null)
            {
                Logger.LogInfo("SIT.Tarkov.Core:PatchConstants():Found GroupingType:" + GroupingType.FullName);
            }

            //GetBackendUrl();
        }
    }
}
