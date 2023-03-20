using EFT;
using EFT.Bots;
using EFT.UI;
using EFT.UI.Matchmaker;
using MTGA.Utilities.Core;
using Newtonsoft.Json;
using System;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using MTGA_Request = MTGA.Utilities.Core.Request;

namespace MTGA.Patches.Menus
{
    /// <summary>
    /// This patch sets matches to offline on screen enter also sets other variables directly
    /// </summary>
    public class AutoSetOfflineMatch : ModulePatch
    {

        public AutoSetOfflineMatch()
        {
            Request();
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

        [PatchPrefix]
        public static void PatchPrefix(object controller)
        {
            RaidSettings = Traverse.Create(controller).Field<RaidSettings>("RaidSettings").Value;
            if (RaidSettings == null)
            {
                Logger.LogInfo("AutoSetOfflineMatch.PatchPrefix : Raid Settings are Null!");
                return;
            }

            Logger.LogInfo($"AutoSetOfflineMatch.PatchPrefix: START");
            Request();


            RaidSettings.RaidMode = DefaultRaidSettings.RaidMode;
            var botSettings = RaidSettings.BotSettings;
            var waveSettings = RaidSettings.WavesSettings;


            //botSettings.IsEnabled = !!(serverSettings.AiAmount != EBotAmount.NoBots);
            botSettings.IsScavWars = DefaultRaidSettings.ScavWars;
            botSettings.BotAmount = DefaultRaidSettings.AiAmount;

            //botSettings.BossType = EBossType.AsOnline; //maybe in the future we can use this to adjust bosses to have different difficulties

            waveSettings.BotAmount = DefaultRaidSettings.AiAmount;
            waveSettings.BotDifficulty = DefaultRaidSettings.AiDifficulty;
            waveSettings.IsBosses = DefaultRaidSettings.BossEnabled;
            waveSettings.IsTaggedAndCursed = DefaultRaidSettings.TaggedAndCursed;

            Logger.LogInfo($"AutoSetOfflineMatch.PatchPrefix END");
        }

        [PatchPostfix]
        public static void PatchPostfix(
            UpdatableToggle ____offlineModeToggle
            )
        {

            // Do a force of these, just encase it breaks
            ____offlineModeToggle.isOn = true;
            ____offlineModeToggle.enabled = false;
            ____offlineModeToggle.interactable = false;


            // Hide "no progression save" panel
            var offlineRaidScreenContent = GameObject.Find("Matchmaker Offline Raid Screen").transform.Find("Content").transform;
            var warningPanel = offlineRaidScreenContent.Find("WarningPanelHorLayout");
            warningPanel.gameObject.SetActive(false);
            var spacer = offlineRaidScreenContent.Find("Space (1)");
            spacer.gameObject.SetActive(false);

            // Disable "Enable practice mode for this raid" toggle
            var practiceModeComponent = GameObject.Find("SoloModeCheckmarkBlocker").GetComponent<UiElementBlocker>();
            practiceModeComponent.SetBlock(true, "tf you lookin at?");
        }

        protected override MethodBase GetTargetMethod()
        {
            var type = typeof(MatchmakerOfflineRaidScreen);
            var methods = PatchConstants.GetAllMethodsForType(type);

            foreach (var method in methods)
            {
                if (!method.Name.StartsWith("Show")) continue;

                var parameters = method.GetParameters();
                if (parameters.Length == 1
                && parameters[0].Name == "controller")
                {
                    Logger.LogInfo(method.Name);
                    return method;
                }
            }
            return null;
        }
    }
}
