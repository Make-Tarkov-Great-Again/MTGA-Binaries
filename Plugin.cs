using BepInEx;
using EFT;
using SIT.A.Tarkov.Core.LocalGame;
using SIT.A.Tarkov.Core.SP;
using SIT.A.Tarkov.Core.SP.Raid;
using SIT.Tarkov.Core;
using SIT.Tarkov.Core.AI;
using SIT.Tarkov.Core.Bundles;
using SIT.Tarkov.Core.PlayerPatches;
using SIT.Tarkov.Core.PlayerPatches.Health;
using SIT.Tarkov.Core.SP;
using SIT.Tarkov.Core.SP.Raid;
using SIT.Tarkov.Core.SP.ScavMode;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace SIT.A.Tarkov.Core
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            PatchConstants.GetBackendUrl();

            // - TURN OFF BS Checkers, FileChecker and BattlEye doesn't work BSG, I see cheaters ALL the time -----
            new ConsistencySinglePatch().Enable();
            new ConsistencyMultiPatch().Enable();
            new BattlEyePatch().Enable();
            new SslCertificatePatch().Enable();
            new UnityWebRequestPatch().Enable();
            new WebSocketPatch().Enable();

            // - Loading Bundles from Server. Working Aki version with some tweaks by me -----
            BundleSetup.Init();
            BundleManager.GetBundles();
            new EasyAssetsPatch().Enable();
            new EasyBundlePatch().Enable();

            // --------- Container Id Debug ------------
            new LootableContainerInteractPatch().Enable();

            // --------- PMC Dogtags -------------------
            new UpdateDogtagPatch().Enable();

            // --------- On Dead -----------------------
            new OnDeadPatch().Enable();

            // --------- Player Init -------------------
            new PlayerInitPatch().Enable();

            // --------- SCAV MODE ---------------------
            new DisableScavModePatch().Enable();
            new ForceLocalGamePatch().Enable();

            //new FilterProfilesPatch().Enable();
            //new BossSpawnChancePatch().Enable();
            //new LocalGameStartingPatch().Enable();
            //LocalGameStartingPatch.LocalGameStarted += LocalGameStartingPatch_LocalGameStarted;

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;

        }

        private void LocalGameStartingPatch_LocalGameStarted()
        {
            Logger.LogInfo($"Local Game Started");
            new LocalGameSpawnAICoroutinePatch().Enable();
        }

        private void SceneManager_sceneUnloaded(Scene arg0)
        {
            //if (LocalGamePatches.LocalGameInstance == null)
            //    new LocalGameStartBotSystemPatch().Disable();

            
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (PatchConstants.PoolManagerType == null)
            {
                PatchConstants.PoolManagerType = PatchConstants.EftTypes.Single(x => PatchConstants.GetAllMethodsForType(x).Any(x => x.Name == "LoadBundlesAndCreatePools"));
                Logger.LogInfo($"Loading PoolManagerType:{ PatchConstants.PoolManagerType.FullName}");

                Logger.LogInfo($"Getting PoolManager Instance");
                Type generic = typeof(Comfort.Common.Singleton<>);
                Type[] typeArgs = { PatchConstants.PoolManagerType };
                ConstructedBundleAndPoolManagerSingletonType = generic.MakeGenericType(typeArgs);
                Logger.LogInfo(PatchConstants.PoolManagerType.FullName);
                Logger.LogInfo(ConstructedBundleAndPoolManagerSingletonType.FullName);

                new LoadBotTemplatesPatch().Enable();
                //new LoadBotProfileFromServerPatch().Enable();

            }




        }

        private Type ConstructedBundleAndPoolManagerSingletonType { get; set; }
        public static object BundleAndPoolManager { get; set; }

        public static Type poolsCategoryType { get; set; }
        public static Type assemblyTypeType { get; set; }

        public static MethodInfo LoadBundlesAndCreatePoolsMethod { get; set; }

        public static Task LoadBundlesAndCreatePools(ResourceKey[] resources)
        {
            return LoadBundlesAndCreatePoolsMethod.Invoke(BundleAndPoolManager,
                new object[] {
                    Enum.Parse(poolsCategoryType, "Raid")
                    , Enum.Parse(assemblyTypeType, "Local")
                    , resources
                    , PatchConstants.GetPropertyFromType(PatchConstants.JobPriorityType, "General").GetValue(null, null)
                    , null
                    , default(CancellationToken)
                }
                ) as Task;
        }

        void FixedUpdate()
        {
            if(PatchConstants.PoolManagerType != null && ConstructedBundleAndPoolManagerSingletonType != null && BundleAndPoolManager == null)
            {
                BundleAndPoolManager = PatchConstants.GetPropertyFromType(ConstructedBundleAndPoolManagerSingletonType, "Instance").GetValue(null, null); //Activator.CreateInstance(PatchConstants.PoolManagerType);
                if (BundleAndPoolManager != null)
                {
                    Logger.LogInfo("BundleAndPoolManager Singleton Instance found: " + BundleAndPoolManager.GetType().FullName);
                    poolsCategoryType = BundleAndPoolManager.GetType().GetNestedType("PoolsCategory");
                    if (poolsCategoryType != null)
                    {
                        Logger.LogInfo(poolsCategoryType.FullName);
                    }
                    assemblyTypeType = BundleAndPoolManager.GetType().GetNestedType("AssemblyType");
                    if (assemblyTypeType != null)
                    {
                        Logger.LogInfo(assemblyTypeType.FullName);
                    }
                    LoadBundlesAndCreatePoolsMethod = PatchConstants.GetMethodForType(BundleAndPoolManager.GetType(), "LoadBundlesAndCreatePools");
                    if (LoadBundlesAndCreatePoolsMethod != null)
                    {
                        Logger.LogInfo(LoadBundlesAndCreatePoolsMethod.Name);
                    }
                }
            }
        }
    }
}
