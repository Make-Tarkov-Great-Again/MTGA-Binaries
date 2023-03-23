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

        [PatchPostfix]
        public static void PatchPostfix(
        //[PatchPostfix]
        //public static void PatchPostfix(
            MatchmakerRaidSettingView ____aiAmountSettings,
            MatchmakerRaidSettingView ____aiDifficultySettings,
            MatchmakerRaidSettingView ____scavWarSettings,
            MatchmakerRaidSettingView ____cursedSettings,
            MatchmakerRaidSettingView ____bossesEnabledSettings,
            MatchmakerRaidSettingView ____randomTimeSettings,
            MatchmakerRaidSettingView ____randomWeatherSettings,
            MatchmakerRaidSettingView ____FoodAndWaterSetting,
            MatchmakerRaidSettingView ____weatherSettings,
            MatchmakerRaidSettingView ____playerSpawnSetting
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

    }
}
