using System.Reflection;
using EFT.UI.Matchmaker;
using EFT;
using MTGA.Utilities.Core;
using static MTGA.Patches.Menus.AutoSetOfflineMatch;
using EFT.Bots;

namespace MTGA.Patches.Menus
{
    public class SetRaidSettingsSummary : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MatchmakerRaidSettingsSummaryView).GetMethod("Show");
        }

        //[PatchPrefix]
        //public static void PatchPrefix(
        [PatchPostfix]
        public static void PatchPostfix(
            ref RaidSettings raidSettings,
            MatchmakerRaidSettingView ____aiAmountSettings,
            MatchmakerRaidSettingView ____aiDifficultySettings,
            MatchmakerRaidSettingView ____scavWarSettings,
            MatchmakerRaidSettingView ____cursedSettings,
            MatchmakerRaidSettingView ____bossesEnabledSettings
            )
        {
            Logger.LogInfo("SetRaidSettingsSummary.PatchPostfix");

            var botSettings = raidSettings.BotSettings;
            var waveSettings = raidSettings.WavesSettings;

            if (waveSettings.IsBosses != DefaultRaidSettings.BossEnabled)
            {
                Logger.LogInfo("[SetRaidSettingsSummary.PatchPostfix] IT DONT EQUAL THE SAME BRO WHAAAAAAAAAAAAAAAAAAAAAAAAT");
            }

            string ENABLED = "Enabled";
            string DISABLED = "Disabled";

            string aiAmount = ("BotAmount/" + waveSettings.BotAmount.ToStringNoBox()).Localized();
            string aiDifficulty = ("BotDifficulty/" + waveSettings.BotDifficulty.ToStringNoBox()).Localized();

            ____aiAmountSettings.Refresh(aiAmount, true);
            ____aiDifficultySettings.Refresh(aiDifficulty, true);
            ____bossesEnabledSettings.Refresh(DefaultRaidSettings.BossEnabled ? ENABLED : DISABLED, true);
            ____scavWarSettings.Refresh(botSettings.IsScavWars ?  ENABLED : DISABLED, true);
            ____cursedSettings.Refresh(waveSettings.IsTaggedAndCursed ? ENABLED : DISABLED, true);

        }
    }
}
