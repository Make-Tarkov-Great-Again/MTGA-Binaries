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
                    && method.ReturnType == typeof(void)
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
        public static bool PatchPrefix(ref RaidSettings ____raidSettings, ref LocationSettingsClass.SelectedLocation location, ref Result<ExitStatus, TimeSpan, MetricsClass> result)
        {
            Logger.LogInfo("OfflineSaveProfile:: PatchPrefix");
            ____raidSettings.RaidMode = ERaidMode.Online;

		//public int EscapeTimeLimit;
        
        var session = ClientAccesor.GetClientApp().GetClientBackEndSession();

            var profile = (____raidSettings.IsScav && ____raidSettings.Side == ESideType.Savage) ? session.ProfileOfPet : session.Profile;
            var exitStatus = result.Value0.ToString().ToLower();
            var currentHealth = HealthListener.Instance.CurrentHealth;


            var exitTime = result.Value1.TotalMilliseconds;
            var locationTime = location.EscapeTimeLimit * 60000;
            if (exitTime > locationTime)
            {
                result.Value0 = ExitStatus.MissingInAction;
                exitStatus = ExitStatus.MissingInAction.ToString().ToLower();
                //Logger.LogInfo($"ExitStatus changed to {exitStatus}");
            }

            SaveProfileRequest request = new()
            {
                ExitStatus = exitStatus,
                IsPlayerScav = ____raidSettings.IsScav,
                Profile = profile,
                Health = currentHealth
            };

            var convertedJson = request.MTGAToJson();
            MTGA_Request.Instance.PostJson("/client/raid/profile/save", convertedJson, true);
            return true;
        }

        public class SaveProfileRequest
        {
            public string ExitStatus { get; set; }
            public bool IsPlayerScav { get; set; }
            public Profile Profile { get; set; }
            public object Health { get; set; }
        }
    }
}
