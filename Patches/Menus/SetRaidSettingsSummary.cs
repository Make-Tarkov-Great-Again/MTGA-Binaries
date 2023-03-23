using System.Reflection;
using EFT.UI.Matchmaker;
using EFT;
using MTGA.Utilities.Core;
using static MTGA.Patches.Menus.SetMatchmakerOfflineRaidScreen;
using static UnityEngine.Experimental.Rendering.RayTracingAccelerationStructure;

namespace MTGA.Patches.Menus
{
    public class SetRaidSettingsSummary : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MatchmakerRaidSettingsSummaryView).GetMethod("Show");
        }

        [PatchPrefix]
        public static void PatchPrefix(
        //[PatchPostfix]
        //public static void PatchPostfix(
            MatchmakerRaidSettingView ____aiAmountSettings,
            MatchmakerRaidSettingView ____aiDifficultySettings,
            MatchmakerRaidSettingView ____scavWarSettings,
            MatchmakerRaidSettingView ____cursedSettings,
            MatchmakerRaidSettingView ____bossesEnabledSettings,
            MatchmakerRaidSettingView ____randomTimeSettings,
            MatchmakerRaidSettingView ____randomWeatherSettings
            )
        {
            //Logger.LogInfo("SetRaidSettingsSummary.PatchPrefix");

            string ENABLED = "Enabled".Localized(null);
            string DISABLED = "Disabled".Localized(null);

            //string aiAmount = ("BotAmount/" + botSettings.BotAmount.ToStringNoBox()).Localized();
            //string aiDifficulty = ("BotDifficulty/" + waveSettings.BotDifficulty.ToStringNoBox()).Localized();
            bool bossEnabled =  DefaultRaidSettings.BossEnabled; //waveSettings.IsBosses;
            bool scavWar = DefaultRaidSettings.ScavWars; //botSettings.IsScavWars;
            bool tagged = DefaultRaidSettings.TaggedAndCursed; //waveSettings.IsTaggedAndCursed;

            string aiAmount = ("BotAmount/" + DefaultRaidSettings.AiAmount.ToStringNoBox()).Localized();
            string aiDifficulty = ("BotDifficulty/" + DefaultRaidSettings.AiDifficulty.ToStringNoBox()).Localized();

            ____aiAmountSettings.Refresh(aiAmount, true);
            ____aiDifficultySettings.Refresh(aiDifficulty, true);
            ____bossesEnabledSettings.Refresh(bossEnabled ? ENABLED : DISABLED, true);
            ____scavWarSettings.Refresh(scavWar ?  ENABLED : DISABLED, true);
            ____cursedSettings.Refresh(tagged ? ENABLED : DISABLED, true);

            ____randomTimeSettings.Refresh(DISABLED, false);
            ____randomWeatherSettings.Refresh(DISABLED, false);
        }

        [PatchPostfix]
        public static void PatchPostfix(RaidSettings raidSettings)
        {
/*            var botSettings = raidSettings.BotSettings;
            var waveSettings = raidSettings.WavesSettings;

            if(DefaultRaidSettings.AiAmount != botSettings.BotAmount)
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
            }*/
        }

    }
}
