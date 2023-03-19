using EFT;
using EFT.Bots;
using EFT.UI;
using EFT.UI.Matchmaker;
using HarmonyLib;
using MTGA.Utilities.Core;
using Newtonsoft.Json;
using System;
using System.Reflection;
using UnityEngine;
using MTGA_Request = MTGA.Utilities.Core.Request;

namespace MTGA.Patches.Menus
{

    /// <summary>
    /// This patch sets matches to offline on screen enter also sets other variables directly from server settings
    /// URL called:"/singleplayer/settings/raid/menu"
    /// </summary>
    public class AutoSetOfflineMatch : ModulePatch
    {
        public AutoSetOfflineMatch()
        {
            _ = typeof(RaidSettings);
        }

        private static DefaultRaidSettings raidSettings = null;

/*        [PatchPrefix]
        public static void PatchPrefix(ref RaidSettings raidSettings)
        {
            //var raidSettings = Traverse.Create(controller).Field<RaidSettings>("RaidSettings").Value;
            //Logger.LogInfo("AutoSetOfflineMatch.PatchPrefix");

            var serverSettings = Request();

            var botSettings = raidSettings.BotSettings;
            var waveSettings = raidSettings.WavesSettings;

            if (raidSettings != null)
            {
                raidSettings.RaidMode = serverSettings.RaidMode;

                //botSettings.IsEnabled = !!(serverSettings.AiAmount != EBotAmount.NoBots);
                botSettings.IsScavWars = serverSettings.ScavWars;
                botSettings.BotAmount = serverSettings.AiAmount;
                //botSettings.BossType = EBossType.AsOnline; //maybe in the future we can use this to adjust bosses to have different difficulties

                waveSettings.BotAmount = serverSettings.AiAmount;
                waveSettings.BotDifficulty = serverSettings.AiDifficulty;
                waveSettings.IsBosses = serverSettings.BossEnabled;
                waveSettings.IsTaggedAndCursed = serverSettings.TaggedAndCursed;

            }
            else
            {
                Logger.LogInfo("AutoSetOfflineMatch.PatchPrefix : Raid Settings are Null!");
            }


        }*/

        public static DefaultRaidSettings Request()
        {
            try
            {
                //if (raidSettings == null)
                //{
                //Logger.LogInfo("AutoSetOfflineMatch.Request");

                var json = MTGA_Request.Instance.GetJson("/singleplayer/settings/raid/menu");

                if (string.IsNullOrWhiteSpace(json))
                {
                    Logger.LogError("Received NULL response for DefaultRaidSettings. Defaulting to fallback.");
                    return null;
                }

                try
                {
                    raidSettings = JsonConvert.DeserializeObject<DefaultRaidSettings>(json);
                    //Logger.LogInfo("Obtained DefaultRaidSettings from Server");
                }
                catch (Exception exception)
                {
                    Logger.LogError("Failed to deserialize DefaultRaidSettings from server. Check your gameplay.json config in your server. Defaulting to fallback. Exception: " + exception);
                    return null;
                }
                //}
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
            finally
            {

            }


            return raidSettings;

        }

        [PatchPostfix]
        public static void PatchPostfix(ref RaidSettings raidSettings, UpdatableToggle ____offlineModeToggle)
        {

            //var raidSettings = Traverse.Create(controller).Field<RaidSettings>("RaidSettings").Value;
            //Logger.LogInfo("AutoSetOfflineMatch.PatchPrefix");

            var serverSettings = Request();

            var botSettings = raidSettings.BotSettings;
            var waveSettings = raidSettings.WavesSettings;

            if (raidSettings != null)
            {
                raidSettings.RaidMode = serverSettings.RaidMode;

                //botSettings.IsEnabled = !!(serverSettings.AiAmount != EBotAmount.NoBots);
                botSettings.IsScavWars = serverSettings.ScavWars;
                botSettings.BotAmount = serverSettings.AiAmount;
                //botSettings.BossType = EBossType.AsOnline; //maybe in the future we can use this to adjust bosses to have different difficulties

                waveSettings.BotAmount = serverSettings.AiAmount;
                waveSettings.BotDifficulty = serverSettings.AiDifficulty;
                waveSettings.IsBosses = serverSettings.BossEnabled;
                waveSettings.IsTaggedAndCursed = serverSettings.TaggedAndCursed;

            }
            else
            {
                Logger.LogInfo("AutoSetOfflineMatch.PatchPrefix : Raid Settings are Null!");
            }

            // Do a force of these, just encase it breaks
            ____offlineModeToggle.isOn = true;
            ____offlineModeToggle.gameObject.SetActive(false);
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
            practiceModeComponent.SetBlock(true, "Raids in SPT are always Offline raids. Don't worry - your progress will be saved!");
        }

        protected override MethodBase GetTargetMethod()
        {
            var type = typeof(MatchmakerOfflineRaidScreen);
            var methods = PatchConstants.GetAllMethodsForType(type);

            foreach (var method in methods)
            {
                if (!method.Name.StartsWith("Show")) continue;

                var parameters = method.GetParameters();
                if (parameters.Length == 2
                && parameters[0].Name == "profileInfo"
                && parameters[0].ParameterType == typeof(InfoClass)
                && parameters[1].Name == "raidSettings"
                && parameters[1].ParameterType == typeof(RaidSettings))
                {
                    Logger.LogInfo(method.Name);
                    return method;
                }
            }
            return null;
        }
    }

    public class DefaultRaidSettings
    {
        public ERaidMode RaidMode;
        public EBotAmount AiAmount;
        public EBotDifficulty AiDifficulty;
        public bool BossEnabled;
        public bool ScavWars;
        public bool TaggedAndCursed;

        public DefaultRaidSettings(ERaidMode raidMode, EBotAmount aiAmount, EBotDifficulty aiDifficulty, bool bossEnabled, bool scavWars, bool taggedAndCursed)
        {
            RaidMode = raidMode;
            AiAmount = aiAmount;
            AiDifficulty = aiDifficulty;
            BossEnabled = bossEnabled;
            ScavWars = scavWars;
            TaggedAndCursed = taggedAndCursed;
        }
    }
}
