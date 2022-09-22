using EFT;
using EFT.Bots;
using EFT.UI;
using Newtonsoft.Json;
using MTGA.Core;
using System;
using System.Reflection;
using UnityEngine;

namespace MTGA.Core.Menus
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


        [PatchPostfix]
        public static void PatchPostfix(UpdatableToggle ____offlineModeToggle, UpdatableToggle ____botsEnabledToggle,
            DropDownBox ____aiAmountDropdown, DropDownBox ____aiDifficultyDropdown, UpdatableToggle ____enableBosses,
            UpdatableToggle ____scavWars, UpdatableToggle ____taggedAndCursed, UpdatableToggle ____coopModeToggle)
        {

            //Logger.LogInfo("AutoSetOfflineMatch.PatchPostfix");

            var warningPanel = GameObject.Find("Warning Panel");
            UnityEngine.Object.Destroy(warningPanel);

            // Do a force of these, just encase it breaks
            ____offlineModeToggle.isOn = true;
            ____offlineModeToggle.gameObject.SetActive(false);
            ____offlineModeToggle.enabled = false;
            ____offlineModeToggle.interactable = false;
            ____botsEnabledToggle.isOn = true;
            ____enableBosses.isOn = true;

            ____aiAmountDropdown.UpdateValue((int)EBotAmount.Medium, false);
            ____aiDifficultyDropdown.UpdateValue((int)EBotDifficulty.Medium, false);

            Request();

            if (raidSettings != null)
            {
                ____aiAmountDropdown.UpdateValue((int)raidSettings.AiAmount, false);
                ____aiDifficultyDropdown.UpdateValue((int)raidSettings.AiDifficulty, false);
                ____enableBosses.isOn = raidSettings.BossEnabled;
                ____scavWars.isOn = raidSettings.ScavWars;
                ____taggedAndCursed.isOn = raidSettings.TaggedAndCursed;

                ____offlineModeToggle.gameObject.SetActive(false);
                ____offlineModeToggle.enabled = false;
                ____offlineModeToggle.interactable = false;
            }
            else
            {
                Logger.LogInfo("AutoSetOfflineMatch.PatchPostfix : Raid Settings are Null!");
            }

          
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(EFT.UI.Matchmaker.MatchmakerOfflineRaidScreen).GetMethod("Show", BindingFlags.NonPublic | BindingFlags.Instance);
        }


        public static DefaultRaidSettings Request()
        {
            try
            {
                //if (raidSettings == null)
                //{
                //Logger.LogInfo("AutoSetOfflineMatch.Request");

                var json = new Request().GetJson("/singleplayer/settings/raid/menu");

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
    }

    public class DefaultRaidSettings
    {
        public EBotAmount AiAmount;
        public EBotDifficulty AiDifficulty;
        public bool BossEnabled;
        public bool ScavWars;
        public bool TaggedAndCursed;

        public DefaultRaidSettings(EBotAmount aiAmount, EBotDifficulty aiDifficulty, bool bossEnabled, bool scavWars, bool taggedAndCursed)
        {
            AiAmount = aiAmount;
            AiDifficulty = aiDifficulty;
            BossEnabled = bossEnabled;
            ScavWars = scavWars;
            TaggedAndCursed = taggedAndCursed;
        }
    }
}