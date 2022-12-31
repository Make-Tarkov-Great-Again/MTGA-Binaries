using Comfort.Common;
using EFT;
using HarmonyLib;
using Newtonsoft.Json;
using MTGA.Core;
using MTGA.Core.PlayerPatches.Health;
using System;
using System.Linq;
using System.Reflection;
using EFT.UI.Matchmaker;
using static EFTBackendSettings;
using static GClass1720;

namespace MTGA.Core
{
    public class OfflineSaveProfile : ModulePatch
    {
        static OfflineSaveProfile()
        {
            // compile-time check
            //_ = nameof(ClientMetrics.Metrics);

            _ = nameof(TarkovApplication);
            _ = nameof(EFT.RaidSettings);

            //_defaultJsonConverters = Traverse.Create(converterClass).Field<JsonConverter[]>("Converters").Value;
        }

        protected override MethodBase GetTargetMethod()
        {
            var methods = PatchConstants.GetAllMethodsForType(PatchConstants.EftTypes.Single(x => x.Name == "TarkovApplication"));
            foreach (var method in methods)
            {
                var paramameters = method.GetParameters();

                if (method.Name.StartsWith("method")
                    && paramameters.Length == 5
                    && paramameters[0].Name == "profileId"
                    && paramameters[0].ParameterType == typeof(string)
                    && paramameters[1].Name == "savageProfile"
                    && paramameters[1].ParameterType == typeof(EFT.Profile)
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
            Logger.Log(BepInEx.Logging.LogLevel.Error, "OfflineSaveProfile::Method is not found!");

            return null;
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref EFT.RaidSettings ____raidSettings, ref Result<EFT.ExitStatus, TimeSpan, MetricsClass> result)
        {
            Logger.LogInfo("OfflineSaveProfile::PatchPrefix");
            // isLocal = false;
            ____raidSettings.RaidMode = ERaidMode.Online;

            var session = ClientAccesor.GetClientApp().GetClientBackEndSession();
            var profile = (____raidSettings.IsScav && ____raidSettings.Side == ESideType.Savage) ? session.Profile : session.ProfileOfPet;
            var exitStatus = result.Value0.ToString().ToLower();
            var currentHealth = HealthListener.Instance.CurrentHealth;

            SaveProfileRequest request = new()
            {
                exit = exitStatus,
                profile = profile,
                health = currentHealth,
                isPlayerScav = ____raidSettings.IsScav
            };

            var convertedJson = request.MTGAToJson();
            new Request(PatchConstants.GetPHPSESSID(), PatchConstants.GetBackendUrl()).PostJson("/client/raid/profile/save", convertedJson);

            return true;
        }

        public class SaveProfileRequest
        {
            public string exit { get; set; }
            public EFT.Profile profile { get; set; }
            public bool isPlayerScav { get; set; }
            //public PlayerHealth health { get; set; }
            public object health { get; set; }
        }
    }
}
