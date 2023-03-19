using System;
using System.Reflection;
using EFT.Bots;
using EFT.UI;
using EFT;
using MTGA.Utilities.Core;
using Newtonsoft.Json;
using MTGA_Request = MTGA.Utilities.Core.Request;

namespace MTGA.Patches.Menus
{
    public class AutoSetRaidConfig : ModulePatch
    {
        private static DefaultRaidSettings raidSettings = null;

        [PatchPostfix]
        public static void PatchPostfix(
            DropDownBox ____aiAmountDropdown,
            DropDownBox ____aiDifficultyDropdown,
            UpdatableToggle ____enableBosses,
            UpdatableToggle ____scavWars,
            UpdatableToggle ____taggedAndCursed,
            UpdatableToggle ____coopModeToggle,
            UiElementBlocker ____coopModeBlocker
            )
        {
            Logger.LogInfo("AutoSetRaidConfig.PatchPostfix");
            ____enableBosses.isOn = true;
            ____aiAmountDropdown.UpdateValue((int)EBotAmount.AsOnline, false);
            ____aiDifficultyDropdown.UpdateValue((int)EBotDifficulty.AsOnline, false);

            Request();
            if (raidSettings == null)
            {
                Logger.LogInfo("AutoSetRaidConfig.PatchPostfix : Raid Settings are Null!");
                return;
            }

            ____aiAmountDropdown.UpdateValue((int)raidSettings.AiAmount, false);
            ____aiDifficultyDropdown.UpdateValue((int)raidSettings.AiDifficulty, false);
            ____enableBosses.isOn = raidSettings.BossEnabled;
            ____scavWars.isOn = raidSettings.ScavWars;
            ____taggedAndCursed.isOn = raidSettings.TaggedAndCursed;

        }
        
        protected override MethodBase GetTargetMethod()
        {
            return typeof(EFT.UI.Matchmaker.RaidSettingsWindow).GetMethod(nameof(EFT.UI.Matchmaker.RaidSettingsWindow.Show));
        }
        public static DefaultRaidSettings Request()
        {
            try
            {
                //if (raidSettings == null)
                //{
                //Logger.LogInfo("AutoSetRaidConfig.Request");

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
}
