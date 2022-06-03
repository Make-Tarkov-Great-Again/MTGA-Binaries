using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using EFT;
using UnityEngine;
using Comfort.Common;
using System.Threading;

//using WaveInfo = GClass984; // not used // search for: Difficulty and chppse gclass with lower number whic hcontains Role and Limit variables
//using BotsPresets = GClass552; // Search for GetNewProfile
//using BotData = GInterface15; // Search for PrepareToLoadBackend
//using PoolManager = GClass1487; // Search for LoadBundlesAndCreatePools
//using JobPriority = GClass2549; // Search for General
using SIT.Tarkov.Core;
using System;

namespace SIT.Tarkov.Core.SP
{
    public class LoadBotTemplatesPatch : ModulePatch
    {
        private static MethodInfo _getNewProfileMethod;

        public static Type BotPresetsType { get; set; }
        public static Type BotDataType { get; set; }

        public static Type PoolManagerType { get; set; }

        public static Type JobPriorityType { get; set; }

        public LoadBotTemplatesPatch()
        {
            if (BotPresetsType == null)
            {
                BotPresetsType = PatchConstants.EftTypes.LastOrDefault
                    (x => PatchConstants.GetAllMethodsForType(x).Any(y => y.Name.Contains("GetNewProfile")));
            }

            if (BotDataType == null)
            {
                BotDataType = PatchConstants.EftTypes.LastOrDefault(x
                    => x.IsInterface && PatchConstants.GetAllMethodsForType(x).Any(y => y.Name.Contains("PrepareToLoadBackend")));
            }

            if(PoolManagerType == null)
            {
                PoolManagerType = PatchConstants.EftTypes.Single(x => PatchConstants.GetAllMethodsForType(x).Any(x => x.Name == "LoadBundlesAndCreatePools"));
            }

            if(JobPriorityType == null)
            {
                JobPriorityType = PatchConstants.EftTypes.Single(x => 
                    PatchConstants.GetAllMethodsForType(x).Any(x => x.Name == "Priority" && x.IsStatic)
                    && 
                    (PatchConstants.GetFieldFromType(x, "General") != null
                    || PatchConstants.GetPropertyFromType(x, "General") != null)
                    );
            }

            //_ = nameof(BotData.PrepareToLoadBackend);
            //_ = nameof(BotsPresets.GetNewProfile);
            //_ = nameof(PoolManager.LoadBundlesAndCreatePools);
            //_ = nameof(JobPriority.General);

            _getNewProfileMethod = BotPresetsType
                .GetMethod("GetNewProfile", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        }

        protected override MethodBase GetTargetMethod()
        {
            return BotPresetsType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Single(x => IsTargetMethod(x));
        }

        private bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return (parameters.Length == 2
                && parameters[0].Name == "data"
                && parameters[1].Name == "cancellationToken");
        }

        [PatchPrefix]
        //public static bool PatchPrefix(ref Task<Profile> __result, object __instance, BotData data)
        public static bool PatchPrefix(ref Task<Profile> __result, object __instance, object data)
        {
            //    /*
            //        in short when client wants new bot and GetNewProfile() return null (if not more available templates or they don't satisfied by Role and Difficulty condition)
            //        then client gets new piece of WaveInfo collection (with Limit = 30 by default) and make request to server
            //        but use only first value in response (this creates a lot of garbage and cause freezes)
            //        after patch we request only 1 template from server

            //        along with other patches this one causes to call data.PrepareToLoadBackend(1) gets the result with required role and difficulty:
            //        new[] { new WaveInfo() { Limit = 1, Role = role, Difficulty = difficulty } }
            //        then perform request to server and get only first value of resulting single element collection
            //    */
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var taskAwaiter = (Task<Profile>)null;
            //var profile = SIT.Tarkov.Core.PatchConstants.DoSafeConversion<Profile>(_getNewProfileMethod.Invoke(__instance, parameters: new object[] { data }));

            //    if (profile == null)
            //    {
            //        // load from server
            //        Debug.Log("Loading bot profile from server");

            //        var pptlbMethod = data.GetType().GetMethod("PrepareToLoadBackend", BindingFlags.Instance | BindingFlags.Public);

            //        //var source = data.PrepareToLoadBackend(1).ToList();
            //taskAwaiter = SIT.B.Tarkov.SP.PatchConstants.GetClientApp().GetClientBackEndSession().LoadBots(source).ContinueWith(GetFirstResult, taskScheduler);

            //        // =====================================================
            //        // TODO: we havent got this working yet, continue with original
            //        return true;
            //    }
            //    else
            //    {
            //        // return cached profile
            //        Debug.Log("Loading bot profile from cache");
            //        taskAwaiter = Task.FromResult(profile);
            //    }

            //    // load bundles for bot profile
            //    var continuation = new Continuation(taskScheduler);
            //    __result = taskAwaiter.ContinueWith(continuation.LoadBundles, taskScheduler).Unwrap();
            //    return false;
            return true;
        }

        //private static Profile GetFirstResult(Task<Profile[]> task)
        //{
        //    if (task.IsCompleted && task.Result.Any())
        //    {
        //        var result = task.Result[0];
        //        UnityEngine.Debug.LogError($"Loading bot profile from server. role: {result.Info.Settings.Role} side: {result.Side}");
        //        return result;
        //    }

        //    return null;
        //}

        //private struct Continuation
        //{
        //    Profile Profile;
        //    TaskScheduler TaskScheduler { get; }

        //    public Continuation(TaskScheduler taskScheduler)
        //    {
        //        Profile = null;
        //        TaskScheduler = taskScheduler;
        //    }

        //    public Task<Profile> LoadBundles(Task<Profile> task)
        //    {
        //        Profile = task.Result;

        //        Type generic = typeof(Comfort.Common.IResult).Assembly.GetTypes().Single(x => x.FullName == "Comfort.Common.Singleton");
        //        Type[] typeArgs = { PoolManagerType };
        //        Type constructed = generic.MakeGenericType(typeArgs);

        //        var loadTask = Singleton.CreateInstance(constructed);
        //            .LoadBundlesAndCreatePools(PoolManager.PoolsCategory.Raid,
        //                                       PoolManager.AssemblyType.Local,
        //                                       Profile.GetAllPrefabPaths(false).ToArray(),
        //                                       JobPriority.General,
        //                                       null,
        //                                       default(CancellationToken));

        //        return loadTask.ContinueWith(GetProfile, TaskScheduler);
        //    }

        //    private Profile GetProfile(Task task)
        //    {
        //        return Profile;
        //    }
        //}
    }
}