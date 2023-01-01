using Comfort.Common;
using EFT;
using MTGA.Utilities.Core;
using MTGA.Utilities.Player.Health;
using System;
using System.Reflection;

using MTGA_Request = MTGA.Utilities.Core.Request;

namespace MTGA.Patches.Player
{
    public class OfflineSaveProfile : ModulePatch
    {
        static OfflineSaveProfile()
        {
            _ = nameof(TarkovApplication);
            _ = nameof(EFT.RaidSettings);
        }

        protected override MethodBase GetTargetMethod()
        {
            var methods = PatchConstants.GetAllMethodsForType(typeof(TarkovApplication));
            foreach (var method in methods)
            {
                var paramameters = method.GetParameters();

                if (method.Name.StartsWith("method")
                    && paramameters.Length == 5
                    && paramameters[0].Name == "profileId"
                    && paramameters[0].ParameterType == typeof(string)
                    && paramameters[1].Name == "savageProfile"
                    && paramameters[1].ParameterType == typeof(Profile)
                    && paramameters[2].Name == "location"
                    && paramameters[2].ParameterType == typeof(LocationSettingsClass.SelectedLocation)
                    && paramameters[3].Name == "result"
                    && paramameters[3].ParameterType == typeof(Result<ExitStatus, TimeSpan, MetricsClass>)
                    && paramameters[4].Name == "timeHasComeScreenController"
                    && paramameters[4].ParameterType == typeof(EFT.UI.Matchmaker.MatchmakerTimeHasCome.TimeHasComeScreenController)
                    )
                {
                    Logger.Log(BepInEx.Logging.LogLevel.Info, method.Name);
                    return method;
                }
            }
            Logger.Log(BepInEx.Logging.LogLevel.Error, "OfflineSaveProfile:: Method is not found!");

            return null;
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref RaidSettings ____raidSettings, ref Result<ExitStatus, TimeSpan, MetricsClass> result)
        {
            Logger.LogInfo("OfflineSaveProfile:: PatchPrefix");
            ____raidSettings.RaidMode = ERaidMode.Online;


            var session = ClientAccesor.GetClientApp().GetClientBackEndSession();

            var profile = (____raidSettings.IsScav && ____raidSettings.Side == ESideType.Savage) ? session.Profile : session.ProfileOfPet;
            var exitStatus = result.Value0.ToString().ToLower();
            var currentHealth = PlayerPatches.Health.HealthListener.Instance.CurrentHealth;

            SaveProfileRequest request = new()
            {
                exit = exitStatus,
                profile = profile,
                health = currentHealth,
                isPlayerScav = ____raidSettings.IsScav
            };

            var convertedJson = request.MTGAToJson();
            MTGA_Request.Instance.PostJson("/client/raid/profile/save", convertedJson, true);
            return true;
        }

        public class SaveProfileRequest
        {
            public string exit { get; set; }
            public Profile profile { get; set; }
            public bool isPlayerScav { get; set; }
            //public PlayerHealth health { get; set; }
            public object health { get; set; }
        }
    }
}
