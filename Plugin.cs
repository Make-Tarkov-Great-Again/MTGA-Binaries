using BepInEx;
using BepInEx.Configuration;
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
using System.Timers;
using UnityEngine;
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
using MTGA.Patches.Raid.Mods;
using MTGA.Patches.AI.Mods;
using MTGA.Patches.Misc;
using MTGA.Patches.Hideout;
using static MTGA.Plugin;

namespace MTGA
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        // Headlamp Fix by SamSwat
        private bool EnabledHeadLamps { get; set; }
        private static GameObject _flashlight;
        private static GameObject[] _modes;
        private static int _currentMode = 1;
        internal static ConfigEntry<KeyboardShortcut> HeadlightToggleKey;
        internal static ConfigEntry<KeyboardShortcut> HeadlightModeKey;

        // AI Limit by Props
        private bool EnabledAILimit { get; set; }
        public static ConfigEntry<int> BotLimit;
        public static ConfigEntry<float> BotDistance;
        public static ConfigEntry<float> TimeAfterSpawn;
        public static Dictionary<int, Player> playerMapping = new();
        public static Dictionary<int, BotPlayer> botMapping = new();
        public static List<BotPlayer> botList = new();
        public static Player Player { get; set; }
        public static BotPlayer Bot { get; set; }

        // Bush ESP by Props

        public static float TestRayRadius { get; set; }

        public static bool BlockingTypeGoalEnemy { get; set; }
        public static bool BossesStillSee { get; set; }
        public static bool FollowersStillSee { get; set; }
        public static bool PMCsStillSee { get; set; }
        public static bool ScavsStillSee { get; set; }


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
                if (enableBundles != null && enableBundles.Value == true)
                {
                    BundleSetup.Init();
                    BundleManager.GetBundles(); // Crash happens here
                    new EasyAssetsPatch().Enable();
                    new EasyBundlePatch().Enable();
                }

                new QTE().Enable();
                new RemoveUsedBotProfile().Enable();

                // --------- Container Id Debug ------------
                var enableLootableContainerDebug = Config.Bind("Debug", "Lootable Container Debug", false, "Description: Print Lootable Container information").Value;
                if (enableLootableContainerDebug)
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
                var enabledMTGAAISystem = Config.Bind("AI", "AI System", true, "Description: Enable MTGA AI???????").Value;
                if (enabledMTGAAISystem)
                {
                    //new IsEnemyPatch().Enable();
                    new IsPlayerEnemyPatch().Enable();
                    new IsPlayerEnemyByRolePatch().Enable();
                    new BotBrainActivatePatch().Enable();
                    new BotSelfEnemyPatch().Enable();
                }

                EnabledAILimit = Config.Bind("EXPERIEMENTAL AI Limit by Props", "Enable", false, "Description: Disable AI (temporarily) based on distance to the player and user defined bot limit within that distance. Only the closest bots (distance wise) are enabled based on a max value set").Value;
                BotDistance = Config.Bind("EXPERIEMENTAL AI Limit by Props", "Bot Distance", 200f, "Set Max Distance to activate bots");
                BotLimit = Config.Bind("EXPERIEMENTAL AI Limit by Props", "Bot Limit (At Distance)", 10, "Based on your distance selected, limits up to this many # of bots moving at one time");
                TimeAfterSpawn = Config.Bind("EXPERIEMENTAL AI Limit by Props", "Time After Spawn", 10f, "Time (sec) to wait before disabling");
                if (EnabledAILimit)
                {
                    PatchConstants.Logger.LogInfo("Enabling AI Limit");
                    PatchConstants.Logger.LogInfo("AI Limit Enabled");
                }
                new RemoveUsedBotProfile().Enable();
                //new LoadBotTemplatesPatch().Enable();
                //new CreateFriendlyAIPatch().Enable();

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

                new ForceMuteVoIP().Enable();

                var EnabledNoBushESP = Config.Bind("EXPERIEMENTAL No Bush ESP by dvize", "Enable", true, "Description: Tired of AI Looking at your bush and destroying you through it? Now they no longer can.").Value;
                TestRayRadius = Config.Bind("EXPERIEMENTAL No Bush ESP by dvize", "Test Ray Radius", 1f, "Width of the Ray that checks if obstruction. !!DO NOT SET THIS TOO LOW!!").Value;
                BlockingTypeGoalEnemy = Config.Bind("EXPERIEMENTAL No Bush ESP by dvize", "Blocking Type Goal Enemy", true, "Enabled means GoalEnemy Method, Disabled means IsVisible Method. !!DO NOT TOUCH!!").Value;
                BossesStillSee = Config.Bind("EXPERIEMENTAL No Bush ESP by dvize", "BossesStillSee", false, "Allow bosses to see through bushes").Value;
                FollowersStillSee = Config.Bind("EXPERIEMENTAL No Bush ESP by dvize", "FollowersStillSee", false, "Allow boss followers to see through bushes").Value;
                PMCsStillSee = Config.Bind("EXPERIEMENTAL No Bush ESP by dvize", "PMCsStillSee", false, "Allow PMCs to see through bushes").Value;
                ScavsStillSee = Config.Bind("EXPERIEMENTAL No Bush ESP by dvize", "ScavssStillSee", false, "Allow Scavs to see through bushes").Value;
                if (EnabledNoBushESP)
                {
                    PatchConstants.Logger.LogInfo("Enabling No Bush ESP");
                    new NoBushESP().Enable();
                    PatchConstants.Logger.LogInfo("Enabled No Bush ESP");
                }
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
        readonly GameStatus currentGameStatus;
        public void GetGameWorld()
        {
            gameWorld = Singleton<GameWorld>.Instance;
        }

        public void GetPlayer()
        {
            GetGameWorld();
            Player = gameWorld.RegisteredPlayers.Find((Player p) => p.IsYourPlayer);
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

        public static Type poolsCategoryType { get; set; }
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

                var raidE = Enum.Parse(poolsCategoryType, "Raid");

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

        void Update()
        {
            if (EnabledAILimit)
            {
                AILimit();
            }
        }

        void AILimit()
        {
            if (!Singleton<GameWorld>.Instantiated)
            {
                return;
            }
            GetGameWorld();
            try
            {
                UpdateBots(gameWorld);
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
            }
        }

        void UpdateBots(GameWorld gameWorld)
        {

            int botCount = 0;

            for (int i = 0; i < gameWorld.RegisteredPlayers.Count; i++)
            {
                Player = gameWorld.RegisteredPlayers[i];
                if (!Player.IsYourPlayer)
                {
                    if (!botMapping.ContainsKey(Player.Id) && (!playerMapping.ContainsKey(Player.Id)))
                    {
                        playerMapping.Add(Player.Id, Player);
                        var tempbotplayer = new BotPlayer(Player.Id);
                        botMapping.Add(Player.Id, tempbotplayer);
                    }
                    else if (!playerMapping.ContainsKey(Player.Id))
                    {
                        playerMapping.Add(Player.Id, Player);
                    }

                    if (botMapping.ContainsKey(Player.Id))
                    {
                        Bot = botMapping[Player.Id];
                        Bot.Distance = Vector3.Distance(Player.Position, gameWorld.RegisteredPlayers[0].Position);

                        //add bot if eligible
                        if (Bot.EligibleNow && !botList.Contains(Bot))
                        {
                            botList.Add(Bot);
                        }

                        if (!Bot.timer.Enabled && Player.CameraPosition != null)
                        {
                            Bot.timer.Enabled = true;
                            Bot.timer.Start();
                        }
                    }

                }
            }

            //add sort by distance
            if (botList.Count > 1)
            {
                //botList = botList.OrderBy(o => o.Distance).ToList();
                for (int i = 1; i < botList.Count; i++)
                {
                    BotPlayer current = botList[i];
                    int j = i - 1;
                    while (j >= 0 && botList[j].Distance > current.Distance)
                    {
                        botList[j + 1] = botList[j];
                        j--;
                    }
                    botList[j + 1] = current;
                }
            }

            for (int i = 0; i < botList.Count; i++)
            {
                if (botCount < BotLimit.Value && botList[i].Distance < BotDistance.Value)
                {
                    if (playerMapping.ContainsKey(botList[i].Id))
                    {
                        playerMapping[botList[i].Id].enabled = true;
                        //playerMapping[botList[i].Id].gameObject.SetActive(true);

                        botCount++;
                    }
                }
                else
                {
                    if (playerMapping.ContainsKey(botList[i].Id))
                    {
                        playerMapping[botList[i].Id].enabled = false;
                        //playerMapping[botList[i].Id].gameObject.SetActive(false);
                    }
                }
            }
        }

        public static ElapsedEventHandler EligiblePool(BotPlayer botplayer)
        {
            botplayer.timer.Stop();
            botplayer.EligibleNow = true;
            return null;
        }

        public class BotPlayer
        {
            public int Id { get; set; }
            public float Distance { get; set; }
            public bool EligibleNow { get; set; }

            public System.Timers.Timer timer = new (TimeAfterSpawn.Value * 1000);

            public BotPlayer(int newID)
            {
                Id = newID;
                EligibleNow = false;
                timer.Enabled = false;
                timer.AutoReset = false;
                timer.Elapsed += EligiblePool(this);

                playerMapping[Id].OnPlayerDeadOrUnspawn += delegate (Player deadArgs)
                {
                    BotPlayer botPlayer = null;
                    if (botMapping.ContainsKey(deadArgs.Id))
                    {
                        botPlayer = botMapping[deadArgs.Id];
                        botMapping.Remove(deadArgs.Id);
                    }
                    if (botList.Contains(botPlayer))
                    {
                        botList.Remove(botPlayer);
                    }
                    if (playerMapping.ContainsKey(deadArgs.Id))
                    {
                        playerMapping.Remove(deadArgs.Id);
                    }
                };
            }
        }

        void FixedUpdate()
        {
            if (PatchConstants.PoolManagerType != null && ConstructedBundleAndPoolManagerSingletonType != null && BundleAndPoolManager == null)
            {
                BundleAndPoolManager = PatchConstants.GetPropertyFromType(ConstructedBundleAndPoolManagerSingletonType, "Instance").GetValue(null, null); //Activator.CreateInstance(PatchConstants.PoolManagerType);
                if (BundleAndPoolManager != null)
                {
                    poolsCategoryType = BundleAndPoolManager.GetType().GetNestedType("PoolsCategory");
                    if (poolsCategoryType != null)
                    {
                        Logger.LogInfo(poolsCategoryType.FullName);
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
