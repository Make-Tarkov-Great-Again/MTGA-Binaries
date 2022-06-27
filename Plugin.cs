using BepInEx;
using EFT;
using SIT.A.Tarkov.Core.SP;
using SIT.A.Tarkov.Core.SP.Raid;
using SIT.Tarkov.Core;
using SIT.Tarkov.Core.AI;
using SIT.Tarkov.Core.Bundles;
using SIT.Tarkov.Core.Menus;
using SIT.Tarkov.Core.PlayerPatches;
using SIT.Tarkov.Core.PlayerPatches.Health;
using SIT.Tarkov.Core.Raid;
using SIT.Tarkov.Core.Raid.Aki;
using SIT.Tarkov.Core.SP;
using SIT.Tarkov.Core.SP.Raid;
using SIT.Tarkov.Core.SP.ScavMode;
using SIT.Tarkov.SP;
using SIT.Tarkov.SP.Raid;
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
            var enableBundles = Config.Bind("Bundles", "Enable", true);
            if (enableBundles != null && enableBundles.Value == true)
            {
                BundleSetup.Init();
                BundleManager.GetBundles();
                new EasyAssetsPatch().Enable();
                new EasyBundlePatch().Enable();
            }

            // --------- Container Id Debug ------------
            new LootableContainerInteractPatch().Enable();

            // --------- PMC Dogtags -------------------
            new UpdateDogtagPatch().Enable();

            // --------- On Dead -----------------------
            new OnDeadPatch(Config).Enable();

            // --------- Player Init -------------------
            new PlayerInitPatch().Enable();

            // --------- SCAV MODE ---------------------
            new DisableScavModePatch().Enable();
            new ForceLocalGamePatch().Enable();

            //new FilterProfilesPatch().Enable();
            //new BossSpawnChancePatch().Enable();
            //new LocalGameStartingPatch().Enable();
            //LocalGameStartingPatch.LocalGameStarted += LocalGameStartingPatch_LocalGameStarted;

            // --------- Airdrop -----------------------
            new AirdropBoxPatch().Enable();
            new AirdropPatch(Config).Enable();

            // --------- AI -----------------------
            new IsEnemyPatch().Enable();
            new IsPlayerEnemyPatch().Enable();
            new IsPlayerEnemyByRolePatch().Enable();

            // -------------------------------------
            // Matchmaker
            new AutoSetOfflineMatch().Enable();
            new BringBackInsuranceScreen().Enable();
            new DisableReadyButtonOnFirstScreen().Enable();
            new DisableReadyButtonOnSelectLocation().Enable();

            // -------------------------------------
            // Progression
            new OfflineSaveProfile().Enable();
            new ExperienceGainFix().Enable();

            // -------------------------------------
            // Quests
            new ItemDroppedAtPlace_Beacon().Enable();

            // -------------------------------------
            // Raid
            new LoadBotDifficultyFromServer().Enable();
            new SpawnPointPatch().Enable();

            // --------------------------------------
            // Health stuff
            new ReplaceInPlayer().Enable();

            new ChangeHealthPatch().Enable();
            new ChangeEnergyPatch().Enable();
            new ChangeHydrationPatch().Enable();









            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;

        }

        //private void LocalGameStartingPatch_LocalGameStarted()
        //{
        //    Logger.LogInfo($"Local Game Started");
        //    new LocalGameSpawnAICoroutinePatch().Enable();
        //}

        private void SceneManager_sceneUnloaded(Scene arg0)
        {
            
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
                new RemoveUsedBotProfile().Enable();
                //new CreateFriendlyAIPatch().Enable();

            }




        }

        private Type ConstructedBundleAndPoolManagerSingletonType { get; set; }
        public static object BundleAndPoolManager { get; set; }

        public static Type poolsCategoryType { get; set; }
        public static Type assemblyTypeType { get; set; }

        public static MethodInfo LoadBundlesAndCreatePoolsMethod { get; set; }

        public static Task LoadBundlesAndCreatePools(ResourceKey[] resources)
        {
            try
            {
                if(BundleAndPoolManager == null)
                {
                    PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: BundleAndPoolManager is missing");
                    return null;
                }
                var task = LoadBundlesAndCreatePoolsMethod.Invoke(BundleAndPoolManager,
                    new object[] {
                    Enum.Parse(poolsCategoryType, "Raid")
                    , Enum.Parse(assemblyTypeType, "Local")
                    , resources
                    , PatchConstants.GetPropertyFromType(PatchConstants.JobPriorityType, "General").GetValue(null, null)
                    , null
                    , default(CancellationToken)
                    }
                    );
                //PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: task is " + task.GetType());

                if (task != null) // && task.GetType() == typeof(Task))
                {
                    var t = task as Task;
                    //PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: t is " + t.GetType());
                    return t;
                }
            }
            catch (Exception ex)
            {
                PatchConstants.Logger.LogInfo(ex.ToString());
            }
            return null;
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
