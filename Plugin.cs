using BepInEx;
using Comfort.Common;
using EFT;
using Microsoft.Win32;
using SIT.A.Tarkov.Core.Hideout;
using SIT.A.Tarkov.Core.Menus;
using SIT.A.Tarkov.Core.Misc;
using SIT.A.Tarkov.Core.PlayerPatches;
using SIT.A.Tarkov.Core.SP;
using SIT.A.Tarkov.Core.SP.Raid;
using SIT.Tarkov.Core;
using SIT.Tarkov.Core.AI;
using SIT.Tarkov.Core.Bundles;
using SIT.Tarkov.Core.Menus;
using SIT.Tarkov.Core.Misc;
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SIT.A.Tarkov.Core
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            PatchConstants.GetBackendUrl();

            // - Check to ensure an up to date Server is running this library
            //new Request().PostJson("/client/sit-validator", null);

            // - TURN OFF BS Checkers, FileChecker and BattlEye doesn't work BSG, I see cheaters ALL the time -----
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

            // --------- Airdrop -----------------------
            new AirdropBoxPatch().Enable();
            new AirdropPatch(Config).Enable();

            // --------- AI -----------------------
            var enableSITAISystem = Config.Bind("AI", "Enable SIT AI", true).Value;
            if (enableSITAISystem)
            {
                //new IsEnemyPatch().Enable();
                new IsPlayerEnemyPatch().Enable();
                new IsPlayerEnemyByRolePatch().Enable();
                new BotBrainActivatePatch().Enable();
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
            //new SpawnPointPatch().Enable();
            //new BossSpawnChancePatch().Enable();

            // --------------------------------------
            // Health stuff
            new ReplaceInPlayer().Enable();

            new ChangeHealthPatch().Enable();
            new ChangeEnergyPatch().Enable();
            new ChangeHydrationPatch().Enable();

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
                    PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Local, resources, GJobYield.General, null, CancellationToken.None);

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


        internal static bool LegalityCheck()
        {
            byte[] w1 = new byte[198] { 79, 102, 102, 105, 99, 105, 97, 108, 32, 71, 97, 109, 101, 32, 110, 111, 116, 32, 102, 111, 117, 110, 100, 44, 32, 119, 101, 32, 119, 105, 108, 108, 32, 98, 101, 32, 112, 114, 111, 109, 112, 116, 105, 110, 103, 32, 116, 104, 105, 115, 32, 109, 101, 115, 115, 97, 103, 101, 32, 101, 97, 99, 104, 32, 108, 97, 117, 110, 99, 104, 44, 32, 117, 110, 108, 101, 115, 115, 32, 121, 111, 117, 32, 103, 101, 116, 32, 111, 102, 102, 105, 99, 105, 97, 108, 32, 103, 97, 109, 101, 46, 32, 87, 101, 32, 108, 111, 118, 101, 32, 116, 111, 32, 115, 117, 112, 112, 111, 114, 116, 32, 111, 102, 102, 105, 99, 105, 97, 108, 32, 99, 114, 101, 97, 116, 111, 114, 115, 32, 115, 111, 32, 109, 97, 107, 101, 32, 115, 117, 114, 101, 32, 116, 111, 32, 103, 101, 116, 32, 111, 102, 102, 105, 99, 105, 97, 108, 32, 103, 97, 109, 101, 32, 97, 108, 115, 111, 46, 32, 74, 117, 115, 116, 69, 109, 117, 84, 97, 114, 107, 111, 118, 32, 84, 101, 97, 109, 46 };
            byte[] w2 = new byte[23] { 78, 111, 32, 79, 102, 102, 105, 99, 105, 97, 108, 32, 71, 97, 109, 101, 32, 70, 111, 117, 110, 100, 33 };
            try
            {
                List<byte[]> varList = new List<byte[]>() {
                    //Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov
                    new byte[80] { 83, 111, 102, 116, 119, 97, 114, 101, 92, 87, 111, 119, 54, 52, 51, 50, 78, 111, 100, 101, 92, 77, 105, 99, 114, 111, 115, 111, 102, 116, 92, 87, 105, 110, 100, 111, 119, 115, 92, 67, 117, 114, 114, 101, 110, 116, 86, 101, 114, 115, 105, 111, 110, 92, 85, 110, 105, 110, 115, 116, 97, 108, 108, 92, 69, 115, 99, 97, 112, 101, 70, 114, 111, 109, 84, 97, 114, 107, 111, 118 },
                    //InstallLocation
                    new byte[15] { 73, 110, 115, 116, 97, 108, 108, 76, 111, 99, 97, 116, 105, 111, 110 },
                    //DisplayVersion
                    new byte[14] { 68, 105, 115, 112, 108, 97, 121, 86, 101, 114, 115, 105, 111, 110 },
                    //EscapeFromTarkov.exe
                    new byte[20] { 69, 115, 99, 97, 112, 101, 70, 114, 111, 109, 84, 97, 114, 107, 111, 118, 46, 101, 120, 101 }
                };
                //@"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov"
                RegistryKey key = Registry.LocalMachine.OpenSubKey(Encoding.ASCII.GetString(varList[0]));
                if (key != null)
                {
                    //"InstallLocation"
                    object path = key.GetValue(Encoding.ASCII.GetString(varList[1]));
                    //"DisplayVersion"
                    object version = key.GetValue(Encoding.ASCII.GetString(varList[2]));
                    if (path != null && version != null)
                    {
                        var foundGameFiles = path.ToString();
                        var foundGameVersions = version.ToString();
                        string gamefilepath = Path.Combine(foundGameFiles, Encoding.ASCII.GetString(varList[3]));
                        if (LC1A(gamefilepath))
                        {
                            if (LC2B(gamefilepath))
                            {
                                return LC3C(gamefilepath);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            PatchConstants.Logger.LogError("Illegal game found. Buy the fucking game, you cheepskate cunt. Bye!!");
            Application.Quit();
            return false;
        }

        internal static bool LC1A(string gfp)
        {
            var fiGFP = new FileInfo(gfp);
            return (fiGFP.Exists && fiGFP.Length >= 647 * 1000);
        }

        internal static bool LC2B(string gfp)
        {
            var fiBE = new FileInfo(gfp.Replace(".exe", "_BE.exe"));
            return (fiBE.Exists && fiBE.Length >= 1024000);
        }

        internal static bool LC3C(string gfp)
        {
            var diBattlEye = new DirectoryInfo(gfp.Replace("EscapeFromTarkov.exe", "BattlEye"));
            return (diBattlEye.Exists);
        }

        internal static bool IllCatchYouCSRINPeeps()
        {
            byte[] w1 = new byte[198] { 79, 102, 102, 105, 99, 105, 97, 108, 32, 71, 97, 109, 101, 32, 110, 111, 116, 32, 102, 111, 117, 110, 100, 44, 32, 119, 101, 32, 119, 105, 108, 108, 32, 98, 101, 32, 112, 114, 111, 109, 112, 116, 105, 110, 103, 32, 116, 104, 105, 115, 32, 109, 101, 115, 115, 97, 103, 101, 32, 101, 97, 99, 104, 32, 108, 97, 117, 110, 99, 104, 44, 32, 117, 110, 108, 101, 115, 115, 32, 121, 111, 117, 32, 103, 101, 116, 32, 111, 102, 102, 105, 99, 105, 97, 108, 32, 103, 97, 109, 101, 46, 32, 87, 101, 32, 108, 111, 118, 101, 32, 116, 111, 32, 115, 117, 112, 112, 111, 114, 116, 32, 111, 102, 102, 105, 99, 105, 97, 108, 32, 99, 114, 101, 97, 116, 111, 114, 115, 32, 115, 111, 32, 109, 97, 107, 101, 32, 115, 117, 114, 101, 32, 116, 111, 32, 103, 101, 116, 32, 111, 102, 102, 105, 99, 105, 97, 108, 32, 103, 97, 109, 101, 32, 97, 108, 115, 111, 46, 32, 74, 117, 115, 116, 69, 109, 117, 84, 97, 114, 107, 111, 118, 32, 84, 101, 97, 109, 46 };
            byte[] w2 = new byte[23] { 78, 111, 32, 79, 102, 102, 105, 99, 105, 97, 108, 32, 71, 97, 109, 101, 32, 70, 111, 117, 110, 100, 33 };
            try
            {
                List<byte[]> varList = new List<byte[]>() {
                    //Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov
                    new byte[80] { 83, 111, 102, 116, 119, 97, 114, 101, 92, 87, 111, 119, 54, 52, 51, 50, 78, 111, 100, 101, 92, 77, 105, 99, 114, 111, 115, 111, 102, 116, 92, 87, 105, 110, 100, 111, 119, 115, 92, 67, 117, 114, 114, 101, 110, 116, 86, 101, 114, 115, 105, 111, 110, 92, 85, 110, 105, 110, 115, 116, 97, 108, 108, 92, 69, 115, 99, 97, 112, 101, 70, 114, 111, 109, 84, 97, 114, 107, 111, 118 },
                    //InstallLocation
                    new byte[15] { 73, 110, 115, 116, 97, 108, 108, 76, 111, 99, 97, 116, 105, 111, 110 },
                    //DisplayVersion
                    new byte[14] { 68, 105, 115, 112, 108, 97, 121, 86, 101, 114, 115, 105, 111, 110 },
                    //EscapeFromTarkov.exe
                    new byte[20] { 69, 115, 99, 97, 112, 101, 70, 114, 111, 109, 84, 97, 114, 107, 111, 118, 46, 101, 120, 101 }
                };
                //@"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov"
                RegistryKey key = Registry.LocalMachine.OpenSubKey(Encoding.ASCII.GetString(varList[0]));
                if (key != null)
                {
                    //"InstallLocation"
                    object path = key.GetValue(Encoding.ASCII.GetString(varList[1]));
                    //"DisplayVersion"
                    object version = key.GetValue(Encoding.ASCII.GetString(varList[2]));
                    if (path != null && version != null)
                    {
                        var foundGameFiles = path.ToString();
                        var foundGameVersions = version.ToString();
                        string gamefilepath = Path.Combine(foundGameFiles, Encoding.ASCII.GetString(varList[3]));
                        if (File.Exists(gamefilepath))
                        {
                            PatchConstants.Logger.LogDebug("Legal game found. Thank you for supporting BSG!");
                            return true;
                        }
                    }
                }
            }
            catch
            {
            }
            PatchConstants.Logger.LogError("Illegal game found. Buy the fucking game, you cheepskate cunt. Bye!!");
            Application.Quit();
            return false;
        }
    }
}
