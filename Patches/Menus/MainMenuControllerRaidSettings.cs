using System;
using System.Reflection;
using EFT;
using EFT.Bots;
using MTGA.Utilities.Core;
using Newtonsoft.Json;
using MTGA_Request = MTGA.Utilities.Core.Request;


namespace MTGA.Patches.Menus
{
    public class MainMenuControllerRaidSettings : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var type = typeof(EFT.TarkovApplication);
            var methods = PatchConstants.GetAllMethodsForType(type, true);
            foreach (var method in methods)
            {
                if (!method.Name.StartsWith("method_")) continue;

                var parameters = method.GetParameters();
                if (parameters.Length != 4) continue;
                if (parameters[0].ParameterType != typeof(Profile) && parameters[0].Name != "profile") continue;
                if (parameters[1].ParameterType != typeof(ProfileStatusClass) && parameters[1].Name != "profileStatus") continue;
                if (parameters[2].ParameterType != typeof(bool) && parameters[2].Name != "isPet") continue;
                if (parameters[3].ParameterType != typeof(Profile) && parameters[3].Name != "savageProfile") continue;

                Logger.LogInfo($"[MainMenuControllerRaidSettings] {method.Name} found boiiiiii");
                return method;
            }
            Logger.LogInfo($"[MainMenuControllerRaidSettings] method NOT found boiiiiii");
            return null;
        }

        public MainMenuControllerRaidSettings()
        {
            Request();
        }

        public class RaidSettingsTemplate
        {
            public ERaidMode RaidMode;
            public EBotAmount AiAmount;
            public EBotDifficulty AiDifficulty;
            public bool BossEnabled;
            public bool ScavWars;
            public bool TaggedAndCursed;

            public RaidSettingsTemplate(ERaidMode raidMode, EBotAmount aiAmount, EBotDifficulty aiDifficulty, bool bossEnabled, bool scavWars, bool taggedAndCursed)
            {
                RaidMode = raidMode;
                AiAmount = aiAmount;
                AiDifficulty = aiDifficulty;
                BossEnabled = bossEnabled;
                ScavWars = scavWars;
                TaggedAndCursed = taggedAndCursed;
            }
        }

        public static RaidSettingsTemplate DefaultRaidSettings { get; set; }
        public static RaidSettings RaidSettings { get; set; }
        public static RaidSettingsTemplate Request()
        {
            if (DefaultRaidSettings != null) return DefaultRaidSettings;
            try
            {
                var json = MTGA_Request.Instance.GetJson("/singleplayer/settings/raid/menu");

                if (string.IsNullOrWhiteSpace(json))
                {
                    Logger.LogError("Received NULL response for RaidSettingsTemplate. Defaulting to fallback.");
                    return null;
                }

                try
                {
                    DefaultRaidSettings = JsonConvert.DeserializeObject<RaidSettingsTemplate>(json);
                }
                catch (Exception exception)
                {
                    Logger.LogError("Failed to deserialize RaidSettingsTemplate from server. Check your gameplay.json config in your server. Defaulting to fallback. Exception: " + exception);
                    return null;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
            return DefaultRaidSettings;
        }

        //[PatchPrefix]
        //public static void PatchPrefix(ref RaidSettings ____raidSettings)
        [PatchPostfix]
        public static void PatchPostfix(RaidSettings ____raidSettings)
        {
            //Logger.LogInfo($"MainMenuControllerRaidSettings.PatchPrefix: START");
            Logger.LogInfo($"MainMenuControllerRaidSettings.PatchPostfix: START");
            Request();

            ____raidSettings.RaidMode = DefaultRaidSettings.RaidMode;
            //RaidSettings.Side;
            //LocationId
            //RaidSettings.SelectedLocation;
            //RaidSettings.KeyId;
            //MetabolismDisabled
            //PlayersSpawnPlace
            //TimeAndWeatherSettings

            var botSettings = ____raidSettings.BotSettings;
            var waveSettings = ____raidSettings.WavesSettings;

            botSettings.IsScavWars = DefaultRaidSettings.ScavWars;
            botSettings.BotAmount = DefaultRaidSettings.AiAmount;

            //botSettings.BossType = EBossType.AsOnline; //maybe in the future we can use this to adjust bosses to have different difficulties

            waveSettings.BotAmount = DefaultRaidSettings.AiAmount;
            waveSettings.BotDifficulty = DefaultRaidSettings.AiDifficulty;
            waveSettings.IsBosses = DefaultRaidSettings.BossEnabled;
            waveSettings.IsTaggedAndCursed = DefaultRaidSettings.TaggedAndCursed;

            //Logger.LogInfo($"MainMenuControllerRaidSettings.PatchPrefix END");
            Logger.LogInfo($"MainMenuControllerRaidSettings.PatchPostfix END");
        }
    }


}
