using BepInEx;
using Comfort.Common;
using EFT;
using MTGA.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using MTGA.Utilities.Bundles;
using MTGA.Patches.FileChecker;
using MTGA.Patches.AntiCheat;
using MTGA.Patches.Bundles;
using MTGA.Patches.Raid.Menus;
using MTGA.Patches.Raid.Fixes;
using MTGA.Patches.Player.Health;
using MTGA.Patches.Player.Fixes;
using MTGA.Patches.ScavMode;
using MTGA.Patches.Raid.Airdrops;
using MTGA.Patches.AI.Fixes;
using MTGA.Patches.AI;
using MTGA.Patches.Menus;
using MTGA.Patches.Player;
using MTGA.Patches.Raid.FromServer;
using MTGA.Patches.Misc;
using MTGA.Patches.Hideout;

namespace MTGA
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        void Awake()
        {

            try
            {
                PatchConstants.GetBackendUrl();
                new Utilities.Core.Request();

                // - TURN OFF FileChecker and BattlEye -----
                new ConsistencySinglePatch().Enable();
                new ConsistencyMultiPatch().Enable();
                new BattlEyePatch().Enable();
                new SslCertificatePatch().Enable();
                new UnityWebRequestPatch().Enable();

                // - Loading Bundles from Server. Working Aki version with some tweaks by me -----
                var enableBundles = Config.Bind("Bundles", "Enable", true);
                if (enableBundles.Value)
                {
                    //BundleSetup.Init();
                    BundleManager.GetBundles(); // Crash happens here
                    new EasyAssetsPatch().Enable();
                    new EasyBundlePatch().Enable();
                }

                new QTE().Enable();

                // --------- Container Id Debug ------------
                var enableLootableContainerDebug = Config.Bind("Debug", "Lootable Container Debug", false, "Description: Print Lootable Container information");
                if (enableLootableContainerDebug.Value)
                    new LootableContainerInteractPatch().Enable();

                // --------- PMC Dogtags -------------------
                new UpdateDogtagPatch().Enable();

                // --------- On Dead -----------------------
                new OnDeadPatch(Config).Enable();

                // --------- Player Init -------------------
                new PlayerInitPatch().Enable();

                // --------- SCAV MODE ---------------------
                new DisableScavModePatch().Enable();

                // --------- Airdrop (THANKS TO AKI) -----------------------
                new AirdropPatch().Enable();
                new AirdropFlarePatch().Enable();

                // --------- AI -----------------------
                var enabledMTGAAISystem = Config.Bind("AI", "AI System", true, "Description: Enable MTGA AI???????");
                if (enabledMTGAAISystem.Value)
                {
                    new IsPlayerEnemyPatch().Enable();
                    new IsPlayerEnemyByRolePatch().Enable();
                    new BotBrainActivatePatch().Enable();
                    new BotSelfEnemyPatch().Enable();
                }

                new RemoveUsedBotProfile().Enable();

                // --------- Matchmaker ----------------
                new DisableReadyButtonOnFirstScreen().Enable();

                // -------------------------------------
                // Progression
                new OfflineSaveProfile().Enable();
                new ExperienceGainFix().Enable();
                new OfflineDisplayProgressPatch().Enable();

                // -------------------------------------
                // Quests
                new ItemDroppedAtPlace_Beacon().Enable();
                new MidRaidQuestChangePatch().Enable();


                // -------------------------------------
                // Raid
                new LoadBotDifficultyFromServer().Enable();
                new ForceMuteVoIP().Enable();
                new SkipLocationLootCaching().Enable();

                // --------------------------------------
                // Health stuff
                new ReplaceInPlayer().Enable();

                new ChangeHealthPatch().Enable();
                new ChangeEnergyPatch().Enable();
                new ChangeHydrationPatch().Enable();

                new LootContainerInitPatch().Enable();
                new CollectLootPointsDataPatch().Enable();

                new InstantItemsMoving().Enable();

                // Plugin startup logic
                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

                SceneManager.sceneLoaded += SceneManager_sceneLoaded;
                SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;

                SetupMoreGraphicsMenuOptions();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }


        }

        void SceneManager_sceneUnloaded(Scene arg0)
        {

        }

        GameWorld gameWorld = null;
        public void GetGameWorld()
        {
            gameWorld = Singleton<GameWorld>.Instance;
        }

        void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            GetPoolManager();
            GetGameWorld();
        }

        public void SetupMoreGraphicsMenuOptions()
        {
            Logger.LogInfo("Adjusting sliders for Overall Visibility and LOD Quality");
            var TypeOfGraphicsSettingsTab = typeof(EFT.UI.Settings.GraphicsSettingsTab);

            var readOnlyCollection_0 = TypeOfGraphicsSettingsTab.GetField(
                "readOnlyCollection_0",
                BindingFlags.Static |
                BindingFlags.NonPublic
                );

            var readOnlyCollection_3 = TypeOfGraphicsSettingsTab.GetField(
                "readOnlyCollection_3",
                BindingFlags.Static |
                BindingFlags.NonPublic
                );

            List<float> overallVisibility = new();
            for (int i = 0; i <= 11; i++)
            {
                overallVisibility.Add(400 + (i * 50));
            }

            for (int i = 0; i <= 4; i++)
            {
                overallVisibility.Add(1000 + (i * 500));
            }


            List<float> lodQuality = new();
            for (int i = 0; i <= 9; i++)
            {
                lodQuality.Add((float)(2 + (i * 0.25)));
            }

            var Collection_0 = Array.AsReadOnly<float>(overallVisibility.ToArray());
            var Collection_3 = Array.AsReadOnly<float>(lodQuality.ToArray());

            readOnlyCollection_0.SetValue(null, Collection_0);
            readOnlyCollection_3.SetValue(null, Collection_3);
            Logger.LogInfo("Adjusted sliders for Overall Visibility and LOD Quality");
        }


        void GetPoolManager()
        {
            if (PatchConstants.PoolManagerType == null)
            {
                PatchConstants.PoolManagerType = PatchConstants.EftTypes.Single(x => PatchConstants.GetAllMethodsForType(x).Any(x => x.Name == "LoadBundlesAndCreatePools"));
                //Logger.LogInfo($"Loading PoolManagerType:{ PatchConstants.PoolManagerType.FullName}");

                //Logger.LogInfo($"Getting PoolManager Instance");
                Type generic = typeof(Comfort.Common.Singleton<>);
                Type[] typeArgs = { PatchConstants.PoolManagerType };
                ConstructedBundleAndPoolManagerSingletonType = generic.MakeGenericType(typeArgs);
                //Logger.LogInfo(PatchConstants.PoolManagerType.FullName);
                //Logger.LogInfo(ConstructedBundleAndPoolManagerSingletonType.FullName);

            }
        }

        Type ConstructedBundleAndPoolManagerSingletonType { get; set; }
        public static object BundleAndPoolManager { get; set; }

        public static Type PoolsCategoryType { get; set; }
        public static Type AssemblyTypeType { get; set; }

        public static MethodInfo LoadBundlesAndCreatePoolsMethod { get; set; }

        public static async Task LoadBundlesAndCreatePoolsAsync(ResourceKey[] resources)
        {
            try
            {
                if (BundleAndPoolManager == null)
                {
                    PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePoolsAsync: BundleAndPoolManager is missing");
                    return;
                }

                var poolManager = Singleton<PoolManager>.Instance;
                await poolManager.LoadBundlesAndCreatePools(
                    PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Local, resources, JobPriority.General, null, CancellationToken.None);
            }
            catch (Exception ex)
            {
                PatchConstants.Logger.LogError("LoadBundlesAndCreatePoolsAsync - ERROR");
                PatchConstants.Logger.LogError(ex.ToString());
                // You may choose to rethrow the exception here if needed.
            }
        }

        public static Task LoadBundlesAndCreatePools(ResourceKey[] resources)
        {
            try
            {
                if (BundleAndPoolManager == null)
                {
                    PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: BundleAndPoolManager is missing");
                    return null;
                }

                var raidEnum = Enum.Parse(PoolsCategoryType, "Raid");
                var localEnum = Enum.Parse(AssemblyTypeType, "Local");
                var generalProp = PatchConstants.GetPropertyFromType(PatchConstants.JobPriorityType, "General").GetValue(null, null);

                return PatchConstants.InvokeAsyncStaticByReflection(
                    LoadBundlesAndCreatePoolsMethod,
                    BundleAndPoolManager,
                    raidEnum,
                    localEnum,
                    resources,
                    generalProp
                );
            }
            catch (Exception ex)
            {
                PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools -- ERROR ->>>");
                PatchConstants.Logger.LogInfo(ex.ToString());
            }
            return null;
        }

        void FixedUpdate()
        {
            if (PatchConstants.PoolManagerType != null && ConstructedBundleAndPoolManagerSingletonType != null && BundleAndPoolManager == null)
            {
                BundleAndPoolManager = PatchConstants.GetPropertyFromType(ConstructedBundleAndPoolManagerSingletonType, "Instance").GetValue(null, null); //Activator.CreateInstance(PatchConstants.PoolManagerType);
                if (BundleAndPoolManager != null)
                {
                    PoolsCategoryType = BundleAndPoolManager.GetType().GetNestedType("PoolsCategory");
                    if (PoolsCategoryType != null)
                    {
                        Logger.LogInfo(PoolsCategoryType.FullName);
                    }
                    AssemblyTypeType = BundleAndPoolManager.GetType().GetNestedType("AssemblyType");
                    if (AssemblyTypeType != null)
                    {
                        Logger.LogInfo(AssemblyTypeType.FullName);
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
