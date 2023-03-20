using System.Reflection;
using EFT;
using EFT.UI;
using MTGA.Utilities.Core;
using static MTGA.Patches.Menus.AutoSetOfflineMatch;

namespace MTGA.Patches.Menus
{
    public class SetRaidSettingsWindow : ModulePatch
    {

        //[PatchPrefix]
        //public static void PatchPrefix(
        [PatchPostfix]
        public static void PatchPostfix(
            ref RaidSettings raidSettings,
            DropDownBox ____aiAmountDropdown,
            DropDownBox ____aiDifficultyDropdown,
            UpdatableToggle ____enableBosses,
            UpdatableToggle ____scavWars,
            UpdatableToggle ____taggedAndCursed,
            UpdatableToggle ____coopModeToggle,
            UiElementBlocker ____coopModeBlocker,
            UpdatableToggle ____randomWeatherToggle,
            UpdatableToggle ____randomTimeToggle
            )
        {
            Logger.LogInfo("SetRaidSettingsWindow.PatchPrefix");
            if (DefaultRaidSettings == null)
            {
                Logger.LogInfo("SetRaidSettingsWindow.PatchPrefix : DefaultRaidSettings are Null!");
                return;
            }

            var botSettings = raidSettings.BotSettings;
            var waveSettings = raidSettings.WavesSettings;

            if (waveSettings.IsBosses != DefaultRaidSettings.BossEnabled)
            {
                Logger.LogInfo("[SetRaidSettingsWindow.PatchPrefix] IT DONT EQUAL THE SAME BRO WHAAAAAAAAAAAAAAAAAAAAAAAAT");
            }

            ____aiAmountDropdown.UpdateValue((int)botSettings.BotAmount, false);
            ____aiDifficultyDropdown.UpdateValue((int)waveSettings.BotDifficulty, false);

            ____enableBosses.isOn = waveSettings.IsBosses;
            ____enableBosses.UpdateValue(waveSettings.IsBosses, true);

            ____randomTimeToggle.isOn = false;
            ____randomWeatherToggle.isOn = false;

            ____scavWars.UpdateValue(botSettings.IsScavWars, false);
            ____taggedAndCursed.UpdateValue(waveSettings.IsTaggedAndCursed, false);

        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(EFT.UI.Matchmaker.RaidSettingsWindow).GetMethod(nameof(EFT.UI.Matchmaker.RaidSettingsWindow.Show));
        }
    }
}
