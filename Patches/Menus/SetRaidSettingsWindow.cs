using System.Reflection;
using EFT;
using EFT.Bots;
using EFT.UI;
using EFT.UI.Matchmaker;
using MTGA.Utilities.Core;
using static MTGA.Patches.Menus.SetMatchmakerOfflineRaidScreen;

namespace MTGA.Patches.Menus
{
    public class SetRaidSettingsWindow : ModulePatch
    {

        //[PatchPrefix]
        //public static void PatchPrefix(
        [PatchPostfix]
        public static void PatchPostfix(
            RaidSettings raidSettings,
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
            Logger.LogInfo("SetRaidSettingsWindow.PatchPostfix");

            ____randomTimeToggle.interactable = false;
            ____randomTimeToggle.enabled = false;
            ____randomWeatherToggle.interactable = false;
            ____randomWeatherToggle.enabled = false;


            if (DefaultRaidSettings == null)
            {
                Logger.LogInfo("SetRaidSettingsWindow.PatchPostfix : DefaultRaidSettings are Null!");
                return;
            }

            /**/
            var botSettings = raidSettings.BotSettings;
            var waveSettings = raidSettings.WavesSettings;

            EBotAmount amount =   DefaultRaidSettings.AiAmount; //botSettings.BotAmount;
            EBotDifficulty difficulty = DefaultRaidSettings.AiDifficulty; //waveSettings.BotDifficulty; //

            bool bossEnabled =  DefaultRaidSettings.BossEnabled; //waveSettings.IsBosses;
            bool scavWar = DefaultRaidSettings.ScavWars; //botSettings.IsScavWars;
            bool tagged = DefaultRaidSettings.TaggedAndCursed; // waveSettings.IsTaggedAndCursed;

            if (____aiAmountDropdown.CurrentValue() != (int)amount) {
                Logger.LogInfo("UPDATED??????!!!!!");
                DefaultRaidSettings.AiAmount = (EBotAmount)____aiAmountDropdown.CurrentValue();
            }
                

            ____aiAmountDropdown.UpdateValue((int)amount, false);
            ____aiDifficultyDropdown.UpdateValue((int)difficulty, false);

            ____enableBosses.isOn = bossEnabled;
            ____enableBosses.UpdateValue(bossEnabled, false);

            ____scavWars.UpdateValue(scavWar, false);
            ____taggedAndCursed.UpdateValue(tagged, false);

        }

        [PatchPrefix]
        public static void PatchPrefix(RaidSettings raidSettings)
        {
            var botSettings = raidSettings.BotSettings;
            var waveSettings = raidSettings.WavesSettings;

            if (DefaultRaidSettings.AiAmount != botSettings.BotAmount)
            {
                Logger.LogInfo("DefaultRaidSettings.AiAmount != botSettings.BotAmount");
                //DefaultRaidSettings.AiAmount = botSettings.BotAmount;
            }

            if (DefaultRaidSettings.AiDifficulty != waveSettings.BotDifficulty)
            {
                Logger.LogInfo("DefaultRaidSettings.AiDifficulty != waveSettings.BotDifficulty");
                //DefaultRaidSettings.AiDifficulty = waveSettings.BotDifficulty;
            }

            if (DefaultRaidSettings.BossEnabled != waveSettings.IsBosses)
            {
                Logger.LogInfo("DefaultRaidSettings.BossEnabled != waveSettings.IsBosses");
                //DefaultRaidSettings.BossEnabled = waveSettings.IsBosses;
            }
            if (DefaultRaidSettings.ScavWars != botSettings.IsScavWars)
            {
                Logger.LogInfo("DefaultRaidSettings.ScavWars != botSettings.IsScavWars");
                //DefaultRaidSettings.ScavWars = botSettings.IsScavWars;
            }
            if (DefaultRaidSettings.TaggedAndCursed != waveSettings.IsTaggedAndCursed)
            {
                Logger.LogInfo("DefaultRaidSettings.TaggedAndCursed != waveSettings.IsTaggedAndCursed");
                //DefaultRaidSettings.TaggedAndCursed = waveSettings.IsTaggedAndCursed;
            }
        }

        protected override MethodBase GetTargetMethod()
        {
            var type = typeof(RaidSettingsWindow);
            var method = type.GetMethod(nameof(RaidSettingsWindow.Show));
            return method;
        }
    }
}
