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

        [PatchPrefix]
        public static bool PatchPrefix(
        //[PatchPostfix]
        //public static void PatchPostfix(
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

            ____randomTimeToggle.isOn = false;
            ____randomTimeToggle.interactable = false;
            ____randomTimeToggle.enabled = false;

            ____randomWeatherToggle.isOn = false;
            ____randomWeatherToggle.interactable = false;
            ____randomWeatherToggle.enabled = false;


            if (DefaultRaidSettings == null)
            {
                Logger.LogInfo("SetRaidSettingsWindow.PatchPostfix : DefaultRaidSettings are Null!");
                return false;
            }

            /*
             * class2729_0
             */

            EBotAmount amount = DefaultRaidSettings.AiAmount; //botSettings.BotAmount;
            EBotDifficulty difficulty = DefaultRaidSettings.AiDifficulty; //waveSettings.BotDifficulty; //

            bool bossEnabled = DefaultRaidSettings.BossEnabled; //waveSettings.IsBosses;
            bool scavWar = DefaultRaidSettings.ScavWars; //botSettings.IsScavWars;
            bool tagged = DefaultRaidSettings.TaggedAndCursed; // waveSettings.IsTaggedAndCursed;

            ____aiAmountDropdown.UpdateValue((int)amount);
            ____aiDifficultyDropdown.UpdateValue((int)difficulty);

            ____enableBosses.isOn = bossEnabled;
            ____enableBosses.UpdateValue(bossEnabled);

            ____scavWars.isOn = scavWar;
            ____scavWars.UpdateValue(scavWar);

            ____taggedAndCursed.isOn = tagged;
            ____taggedAndCursed.UpdateValue(tagged);
            return true;
        }

        /*        [PatchPrefix]
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
                }*/

        protected override MethodBase GetTargetMethod()
        {
            var type = typeof(RaidSettingsWindow);

            var methods = PatchConstants.GetAllMethodsForType(type);

            foreach (var method in methods)
            {
                if (!method.Name.StartsWith("method_")) continue;
                var param = method.GetParameters();
                if (param.Length != 1) continue;

                if (param[0].ParameterType != typeof(bool)) continue;
                if (param[0].Name != "value") continue;

                if (method.MetadataToken != 0x0600E53B) continue;
                Logger.LogInfo($"[RaidSettingsWindow] {method.Name} found!");
                return method;
            }
            return null;
        }
    }
}
