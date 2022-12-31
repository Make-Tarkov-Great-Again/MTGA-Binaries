﻿using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using EFT;
using UnityEngine;
using Comfort.Common;
using System.Threading;

//using WaveInfo = GClass984; // not used // search for: Difficulty and chppse gclass with lower number whic hcontains Role and Limit variables
//using BotsPresets = GClass552; // Search for GetNewProfile
//using BotData = GInterface15; // Search for PrepareToLoadBackend
//using JobPriority = GClass2633; // Search for General
using MTGA.Core;
using System;
using System.Collections.Generic;

namespace MTGA.Patches.Raid.FromServer
{
    public class LoadBotTemplatesPatch : ModulePatch
    {
        private static MethodInfo _getNewProfileMethod;

        public static Type BotPresetsType { get; set; }

        public static Type BotDataType { get; set; }

        public static Type PoolManagerType = PatchConstants.PoolManagerType;

        public static Type JobPriorityType = PatchConstants.JobPriorityType;

        public LoadBotTemplatesPatch()
        {
            //Logger.LogInfo("Loading BotPresetsType");
            if (BotPresetsType == null)
            {
                BotPresetsType = PatchConstants.EftTypes.LastOrDefault
                    (x => PatchConstants.GetAllMethodsForType(x).Any(y => y.Name.Contains("GetNewProfile")));
                Logger.LogInfo($"Loading BotPresetsType:{BotPresetsType.FullName}");

            }

            //Logger.LogInfo("Loading BotDataType");
            if (BotDataType == null)
            {
                BotDataType = PatchConstants.EftTypes.LastOrDefault(x
                    => x.IsInterface && PatchConstants.GetAllMethodsForType(x).Any(y => y.Name.Contains("PrepareToLoadBackend")));
                Logger.LogInfo($"Loading BotDataType:{BotDataType.FullName}");

            }

            //Logger.LogInfo("Loading PoolManagerType");
            if (PoolManagerType == null)
            {
                throw new Exception("PoolManagerType is Null");
            }

            if (JobPriorityType == null)
            {
                throw new Exception("JobPriorityType is Null");
            }

            //_ = nameof(BotData.PrepareToLoadBackend);
            //_ = nameof(BotsPresets.GetNewProfile);
            //_ = nameof(PoolManager.LoadBundlesAndCreatePools);
            //_ = nameof(JobPriority.General);

            if (BotPresetsType != null)
            {
                _getNewProfileMethod = BotPresetsType
                    .GetMethod("GetNewProfile", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            }
            else
            {
                throw new Exception("BotPresetsType is Null");
            }
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
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            Task<Profile> res = null;
            var task = Task.Run(() => {

                Task<Profile> taskAwaiter = null;
                var profile = (EFT.Profile)_getNewProfileMethod.Invoke(__instance, parameters: new object[] { data });
                //var profile
                if (profile == null)
                {
                    // load from server
                    //Logger.LogInfo("Loading bot profile from server");

                    if (PrepareToLoadBackendMethod == null)
                        PrepareToLoadBackendMethod = data.GetType().GetMethod("PrepareToLoadBackend", BindingFlags.Instance | BindingFlags.Public);

                    var typeOfThisShit = PatchConstants.EftTypes.Single(x =>
                        PatchConstants.GetFieldFromType(x, "Role") != null
                        && PatchConstants.GetFieldFromType(x, "Limit") != null
                        && PatchConstants.GetFieldFromType(x, "Difficulty") != null
                    );
                    Type genericList = typeof(List<>);
                    Type[] typeArgs = { typeOfThisShit };
                    Type constructed = genericList.MakeGenericType(typeArgs);


                    var source = Activator.CreateInstance(constructed, PrepareToLoadBackendMethod.Invoke(data, new object[] { 1 }));
                    var backendSession = PatchConstants.BackEndSession;
                    if (LoadBotsMethod == null)
                        LoadBotsMethod = PatchConstants.GetMethodForType(backendSession.GetType(), "LoadBots");

                    var botsTask = (Task<Profile[]>)LoadBotsMethod.Invoke(backendSession, new object[] { source });
                    taskAwaiter = botsTask.ContinueWith(GetFirstResult, taskScheduler);
                }
                else
                {
                    // return cached profile
                    //Logger.LogInfo("Loading bot profile from cache");
                    taskAwaiter = Task.FromResult(profile);
                }

                // load bundles for bot profile
                //var continuation = new Continuation(taskScheduler);
                Plugin.LoadBundlesAndCreatePoolsAsync(taskAwaiter.Result.GetAllPrefabPaths(true).ToArray());
                //res = taskAwaiter.ContinueWith(continuation.LoadBundles, taskScheduler).Unwrap();
                res = taskAwaiter;

            });

            task.Wait();
            __result = res;

            return false;
        }

        private static MethodInfo LoadBotsMethod = null;
        private static MethodInfo PrepareToLoadBackendMethod = null;

        [PatchPostfix]
        //public static bool PatchPrefix(ref Task<Profile> __result, object __instance, BotData data)
        public static void PatchPostfix(ref Task<Profile> __result, object __instance, object data)
        {
            //var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            //var taskAwaiter = (Task<Profile>)null;
            //var profile = (EFT.Profile)_getNewProfileMethod.Invoke(__instance, parameters: new object[] { data });
            ////var profile
            //if (profile == null)
            //{
            //    // load from server
            //    //Logger.LogInfo("Loading bot profile from server");

            //    if(PrepareToLoadBackendMethod == null)
            //        PrepareToLoadBackendMethod = data.GetType().GetMethod("PrepareToLoadBackend", BindingFlags.Instance | BindingFlags.Public);

            //    var typeOfThisShit = PatchConstants.EftTypes.Single(x =>
            //        PatchConstants.GetFieldFromType(x, "Role") != null
            //        && PatchConstants.GetFieldFromType(x, "Limit") != null
            //        && PatchConstants.GetFieldFromType(x, "Difficulty") != null
            //    );
            //    Type genericList = typeof(List<>);
            //    Type[] typeArgs = { typeOfThisShit };
            //    Type constructed = genericList.MakeGenericType(typeArgs);


            //    var source = Activator.CreateInstance(constructed, PrepareToLoadBackendMethod.Invoke(data, new object[] { 1 }));
            //    var backendSession = PatchConstants.BackEndSession;
            //    if (LoadBotsMethod == null)
            //        LoadBotsMethod = PatchConstants.GetMethodForType(backendSession.GetType(), "LoadBots");

            //    var botsTask = (Task<Profile[]>)LoadBotsMethod.Invoke(backendSession, new object[] { source });
            //    taskAwaiter = botsTask.ContinueWith(GetFirstResult, taskScheduler);
            //}
            //else
            //{
            //    // return cached profile
            //    //Logger.LogInfo("Loading bot profile from cache");
            //    taskAwaiter = Task.FromResult(profile);
            //}

            //// load bundles for bot profile
            //var continuation = new Continuation(taskScheduler);
            //var r = taskAwaiter.ContinueWith(continuation.LoadBundles, taskScheduler).Unwrap();
            //__result = r;
        }

        public static string requestedRole;

        private static Profile GetFirstResult(Task<Profile[]> task)
        {
            if (task.IsCompleted && task.Result.Any())
            {
                Logger.LogInfo($"Loading bot profile from server. count: {task.Result.Length}");
                var result = task.Result[0];
                Logger.LogInfo($"Loading bot profile from server. role: {result.Info.Settings.Role} side: {result.Side}");
                return result;
            }

            return null;
        }

        private struct Continuation
        {
            Profile Profile;
            TaskScheduler TaskScheduler { get; }


            public Continuation(TaskScheduler taskScheduler)
            {
                Profile = null;
                TaskScheduler = taskScheduler;
            }

            public Task<Profile> LoadBundles(Task<Profile> task)
            {
                Profile = task.Result;
                //Logger.LogInfo($"LoadBotTemplatesPatch.LoadBundles");

                //LoadBundlesAndCreatePoolsMethod.Invoke(BundleManager, Enum.Parse()

                var loadTask = Plugin.LoadBundlesAndCreatePools(Profile.GetAllPrefabPaths(true).ToArray());

                return loadTask.ContinueWith(GetProfile, TaskScheduler);
            }

            private Profile GetProfile(Task task)
            {
                //Logger.LogInfo("LoadBotTemplatesPatch+Continuation.GetProfile");
                return Profile;
            }
        }
    }
}