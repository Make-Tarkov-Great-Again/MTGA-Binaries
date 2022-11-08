using Aki.Custom.Airdrops.Patches;
using BepInEx;
using Comfort.Common;
using EFT;
using MTGA.Core.AI;
using MTGA.Core.Bundles;
using MTGA.Core.Hideout;
using MTGA.Core.Menus;
using MTGA.Core.Misc;
using MTGA.Core.PlayerPatches;
using MTGA.Core.PlayerPatches.Health;
using MTGA.Core.Raid;
using MTGA.Core.SP;
using MTGA.Core.SP.ScavMode;
using MTGA.SP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace MTGA.Core
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            PatchConstants.GetBackendUrl();


            // - TURN OFF FileChecker and BattlEye -----
            new ConsistencySinglePatch().Enable();
            new ConsistencyMultiPatch().Enable();
            new BattlEyePatch().Enable();
            new SslCertificatePatch().Enable();
            new UnityWebRequestPatch().Enable();
            new WebSocketPatch().Enable();

            // --------- Container Id Debug ------------
            //new LootableContainerInteractPatch().Enable();

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
            var enableMTGAAISystem = Config.Bind("AI", "Enable MTGA AI", true).Value;
            if (enableMTGAAISystem)
            {
                //new IsEnemyPatch().Enable();
                new IsPlayerEnemyPatch().Enable();
                new IsPlayerEnemyByRolePatch().Enable();
                new BotBrainActivatePatch().Enable();
                new BotSelfEnemyPatch().Enable();
            }

            // --------- Matchmaker ----------------
            new AutoSetOfflineMatch().Enable();
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

            // -------------------------------------
            // Raid
            new LoadBotDifficultyFromServer().Enable();

            var enableCultistsDuringDay = Config.Bind("Raid", "Enabled Cultists Spawning During Day", true).Value;
            if (enableCultistsDuringDay)
            { 
                new CultistsSpawnDuringDay().Enable();
            }
            
            //new SpawnPointPatch().Enable();
            //new BossSpawnChancePatch().Enable();

            // --------------------------------------
            // Health stuff
            new ReplaceInPlayer().Enable();

            new ChangeHealthPatch().Enable();
            new ChangeEnergyPatch().Enable();
            new ChangeHydrationPatch().Enable();

            /*
            var enableAdrenaline = Config.Bind("Extras", "Enable Adrenaline", true).Value;
            if (enableAdrenaline) { 
                new Adrenaline().Enable(); 
            };
            */

            // ----------------------------------------------------------------
            // MongoID. This forces bad JET ids to become what BSG Code expects
            if (MongoIDPatch.MongoIDExists)
            {
                new MongoIDPatch().Enable();
            }

            new HideoutItemViewFactoryShowPatch().Enable();
            new ItemRequirementPanelShowPatch().Enable();

            new LootContainerInitPatch().Enable();
            new CollectLootPointsDataPatch().Enable();

            new SetupItemActionsSettingsPatch().Enable();

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;

            // - Loading Bundles from Server. Working Aki version with some tweaks by me -----
            var enableBundles = Config.Bind("Bundles", "Enable", true);
            if (enableBundles != null && enableBundles.Value == true)
            {
                BundleSetup.Init();
                BundleManager.GetBundles(); // Crash happens here
                new EasyAssetsPatch().Enable();
                new EasyBundlePatch().Enable();
            }

            //var enableRagdollBodies = Config.Bind("SERVPH Ragdoll Bodies", "Enable", true);
            //if (enableRagdollBodies != null && enableRagdollBodies.Value == true)
            //{
            //    new PlayerPatches.SERVPH.SERVPHBodyPatch();
            //}

            SetupMoreGraphicsMenuOptions();
            //new WeaponDrawSpeed().Enable();

        }

        private void SceneManager_sceneUnloaded(Scene arg0)
        {

        }

        GameWorld gameWorld = null;


        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            GetPoolManager();
            GetBackendConfigurationInstance();

            gameWorld = Singleton<GameWorld>.Instance;
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
        private void GetBackendConfigurationInstance()
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



        private void GetPoolManager()
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

                //new LoadBotTemplatesPatch().Enable();
                //new RemoveUsedBotProfile().Enable();
                //new CreateFriendlyAIPatch().Enable();
            }
        }

        private Type ConstructedBundleAndPoolManagerSingletonType { get; set; }
        public static object BundleAndPoolManager { get; set; }

        public static Type poolsCategoryType { get; set; }
        public static Type assemblyTypeType { get; set; }

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

                //var raidE = Enum.Parse(poolsCategoryType, "Raid");
                ////PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: raidE is " + raidE.ToString());

                //var localE = Enum.Parse(assemblyTypeType, "Local");
                ////PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: localE is " + localE.ToString());

                //var GenProp = PatchConstants.GetPropertyFromType(PatchConstants.JobPriorityType, "General").GetValue(null, null);
                //PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: GenProp is " + GenProp.ToString());

                await Singleton<PoolManager>.Instance.LoadBundlesAndCreatePools(
                    PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Local, resources, JobPriority.General, null, CancellationToken.None);

                //await PatchConstants.InvokeAsyncStaticByReflection(
                //    LoadBundlesAndCreatePoolsMethod,
                //    BundleAndPoolManager
                //    , raidE
                //    , localE
                //    , resources
                //    , GenProp
                //    , (object o) => { PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: Progressing!"); }
                //    , default(CancellationToken)
                //    );

                //Task task = LoadBundlesAndCreatePoolsMethod.Invoke(BundleAndPoolManager,
                //    new object[] {
                //    Enum.Parse(poolsCategoryType, "Raid")
                //    , Enum.Parse(assemblyTypeType, "Local")
                //    , resources
                //    , PatchConstants.GetPropertyFromType(PatchConstants.JobPriorityType, "General").GetValue(null, null)
                //    , null
                //    , default(CancellationToken)
                //    }
                //    ) as Task;
                ////PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: task is " + task.GetType());

                //if (task != null) // && task.GetType() == typeof(Task))
                //{
                //    task.ContinueWith(t => { PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools loaded"); });
                //    //var t = task as Task;
                //    PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: task is " + task.GetType());
                //    return task;
                //}
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

                var raidE = Enum.Parse(poolsCategoryType, "Raid");
                //PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: raidE is " + raidE.ToString());

                var localE = Enum.Parse(assemblyTypeType, "Local");
                //PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: localE is " + localE.ToString());

                var GenProp = PatchConstants.GetPropertyFromType(PatchConstants.JobPriorityType, "General").GetValue(null, null);
                //PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: GenProp is " + GenProp.ToString());


                return PatchConstants.InvokeAsyncStaticByReflection(
                    LoadBundlesAndCreatePoolsMethod,
                    BundleAndPoolManager
                    , raidE
                    , localE
                    , resources
                    , GenProp
                    , (object o) => { PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: Progressing!"); }
                    , default(CancellationToken)
                    );

                //Task task = LoadBundlesAndCreatePoolsMethod.Invoke(BundleAndPoolManager,
                //    new object[] {
                //    Enum.Parse(poolsCategoryType, "Raid")
                //    , Enum.Parse(assemblyTypeType, "Local")
                //    , resources
                //    , PatchConstants.GetPropertyFromType(PatchConstants.JobPriorityType, "General").GetValue(null, null)
                //    , null
                //    , default(CancellationToken)
                //    }
                //    ) as Task;
                ////PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: task is " + task.GetType());

                //if (task != null) // && task.GetType() == typeof(Task))
                //{
                //    task.ContinueWith(t => { PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools loaded"); });
                //    //var t = task as Task;
                //    PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: task is " + task.GetType());
                //    return task;
                //}
            }
            catch (Exception ex)
            {
                PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools -- ERROR ->>>");
                PatchConstants.Logger.LogInfo(ex.ToString());
            }
            return null;
        }

        //public static void LoadBundlesAndCreatePoolsSync(ResourceKey[] resources)
        //{
        //    try
        //    {
        //        if (BundleAndPoolManager == null)
        //        {
        //            PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: BundleAndPoolManager is missing");
        //            return;
        //        }
        //        var task = (Task)LoadBundlesAndCreatePoolsMethod.Invoke(BundleAndPoolManager,
        //            new object[] {
        //            Enum.Parse(poolsCategoryType, "Raid")
        //            , Enum.Parse(assemblyTypeType, "Local")
        //            , resources
        //            , PatchConstants.GetPropertyFromType(PatchConstants.JobPriorityType, "General").GetValue(null, null)
        //            , null
        //            , default(CancellationToken)
        //            }
        //            );

        //        task.Wait();
        //    }
        //    catch (Exception ex)
        //    {
        //        PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools -- ERROR ->>>");
        //        PatchConstants.Logger.LogInfo(ex.ToString());
        //    }
        //}

        void FixedUpdate()
        {
            if (PatchConstants.PoolManagerType != null && ConstructedBundleAndPoolManagerSingletonType != null && BundleAndPoolManager == null)
            {
                BundleAndPoolManager = PatchConstants.GetPropertyFromType(ConstructedBundleAndPoolManagerSingletonType, "Instance").GetValue(null, null); //Activator.CreateInstance(PatchConstants.PoolManagerType);
                if (BundleAndPoolManager != null)
                {
                    //Logger.LogInfo("BundleAndPoolManager Singleton Instance found: " + BundleAndPoolManager.GetType().FullName);
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
