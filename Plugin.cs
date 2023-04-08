using BepInEx;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
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
using MTGA.Patches.Web;
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


                // - TURN OFF FileChecker and BattlEye -----
                new ConsistencySinglePatch().Enable();
                new ConsistencyMultiPatch().Enable();
                new BattlEyePatch().Enable();
                new SslCertificatePatch().Enable();
                new UnityWebRequestPatch().Enable();
                new WebSocketPatch().Enable();

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
                new RemoveUsedBotProfile().Enable();

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
                    //new IsEnemyPatch().Enable();
                    new IsPlayerEnemyPatch().Enable();
                    new IsPlayerEnemyByRolePatch().Enable();
                    new BotBrainActivatePatch().Enable();
                    new BotSelfEnemyPatch().Enable();
                }

                new RemoveUsedBotProfile().Enable();
                //new LoadBotTemplatesPatch().Enable();
                //new CreateFriendlyAIPatch().Enable();

                // --------- Matchmaker ----------------
                //new MainMenuControllerRaidSettings().Enable();
                new SetMatchmakerOfflineRaidScreen().Enable();
                new SetRaidSettingsWindow().Enable();
                //new SetRaidSettingsSummary().Enable();
                //new BringBackInsuranceScreen().Enable();
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
                //new SpawnPointPatch().Enable();
                //new BossSpawnChancePatch().Enable();

                // --------------------------------------
                // Health stuff
                new ReplaceInPlayer().Enable();

                new ChangeHealthPatch().Enable();
                new ChangeEnergyPatch().Enable();
                new ChangeHydrationPatch().Enable();

                //new HideoutItemViewFactoryShowPatch().Enable();

                new LootContainerInitPatch().Enable();
                new CollectLootPointsDataPatch().Enable();

                new SetupItemActionsSettingsPatch().Enable();

                /*
                new TransportPrefixCtorPatch().Enable();
                new TransportPrefixPatch().Enable();
                */

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
            GetBackendConfigurationInstance();
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
        void GetBackendConfigurationInstance()
        {
            if (
                PatchConstants.BackendStaticConfigurationType != null &&
                PatchConstants.BackendStaticConfigurationConfigInstance == null)
            {
                PatchConstants.BackendStaticConfigurationConfigInstance = PatchConstants.GetPropertyFromType(PatchConstants.BackendStaticConfigurationType, "Config").GetValue(null);
                //Logger.LogInfo($"BackendStaticConfigurationConfigInstance Type:{ PatchConstants.BackendStaticConfigurationConfigInstance.GetType().Name }");
            }

            if (PatchConstants.BackendStaticConfigurationConfigInstance != null
                && PatchConstants.CharacterControllerSettings.CharacterControllerInstance == null
                )
            {
                PatchConstants.CharacterControllerSettings.CharacterControllerInstance
                    = PatchConstants.GetFieldOrPropertyFromInstance<object>(PatchConstants.BackendStaticConfigurationConfigInstance, "CharacterController", false);
                Logger.LogInfo($"PatchConstants.CharacterControllerInstance Type:{PatchConstants.CharacterControllerSettings.CharacterControllerInstance.GetType().Name}");
            }

            if (PatchConstants.CharacterControllerSettings.CharacterControllerInstance != null
                && PatchConstants.CharacterControllerSettings.ClientPlayerMode == null
                )
            {
                PatchConstants.CharacterControllerSettings.ClientPlayerMode
                    = PatchConstants.GetFieldOrPropertyFromInstance<CharacterControllerSpawner.Mode>(PatchConstants.CharacterControllerSettings.CharacterControllerInstance, "ClientPlayerMode", false);

                PatchConstants.CharacterControllerSettings.ObservedPlayerMode
                    = PatchConstants.GetFieldOrPropertyFromInstance<CharacterControllerSpawner.Mode>(PatchConstants.CharacterControllerSettings.CharacterControllerInstance, "ObservedPlayerMode", false);

                PatchConstants.CharacterControllerSettings.BotPlayerMode
                    = PatchConstants.GetFieldOrPropertyFromInstance<CharacterControllerSpawner.Mode>(PatchConstants.CharacterControllerSettings.CharacterControllerInstance, "BotPlayerMode", false);
            }

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

        public static async void LoadBundlesAndCreatePoolsAsync(ResourceKey[] resources)
        {
            try
            {
                if (BundleAndPoolManager == null)
                {
                    PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: BundleAndPoolManager is missing");
                    return;
                }

                await Singleton<PoolManager>.Instance.LoadBundlesAndCreatePools(
                    PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Local, resources, JobPriority.General, null, CancellationToken.None);

            }
            catch (Exception ex)
            {
                PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools -- ERROR ->>>");
                PatchConstants.Logger.LogInfo(ex.ToString());
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

                var raidE = Enum.Parse(PoolsCategoryType, "Raid");

                var localE = Enum.Parse(AssemblyTypeType, "Local");

                var GenProp = PatchConstants.GetPropertyFromType(PatchConstants.JobPriorityType, "General").GetValue(null, null);

                return PatchConstants.InvokeAsyncStaticByReflection(
                    LoadBundlesAndCreatePoolsMethod,
                    BundleAndPoolManager
                    , raidE
                    , localE
                    , resources
                    , GenProp
                    //, (object o) => { PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: Progressing!"); }
                    , default(CancellationToken)
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
