using Comfort.Common;
using EFT;
using HarmonyLib;
using Newtonsoft.Json;
using MTGA.Core;
using MTGA.Core.PlayerPatches.Health;
using System;
using System.Linq;
using System.Reflection;

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
            foreach (var method in PatchConstants.GetAllMethodsForType(PatchConstants.EftTypes.Single(x=>x.Name == "TarkovApplication")))
            {
                if (method.Name.StartsWith("method") &&
                    method.GetParameters().Length >= 3 &&
                    method.GetParameters()[0].Name == "profileId" &&
                    method.GetParameters()[1].Name == "savageProfile" &&
                    method.GetParameters()[2].Name == "location" &&
                    method.GetParameters().Any(x => x.Name == "result") &&
                    method.GetParameters()[method.GetParameters().Length-1].Name == "timeHasComeScreenController"
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
        public static bool PatchPrefix(
            ref EFT.RaidSettings ____raidSettings
            , ref Result<EFT.ExitStatus, TimeSpan, object> result
            )
        //    [PatchPostfix]
        //public static void PatchPostfix(ref ESideType ___esideType_0, ref object result)
        {
            Logger.LogInfo("OfflineSaveProfile::PatchPrefix");
            // isLocal = false;
            ____raidSettings.RaidMode = ERaidMode.Online;

            var session = ClientAccesor.GetClientApp().GetClientBackEndSession();
            var isPlayerScav = false;
            var profile = session.Profile;

            if (____raidSettings.Side == ESideType.Savage)
            {
                profile = session.ProfileOfPet;
                isPlayerScav = true;
            }

            var currentHealth = HealthListener.Instance.CurrentHealth;
            
            var beUrl = MTGA.Core.PatchConstants.GetBackendUrl();
            var sessionId = MTGA.Core.PatchConstants.GetPHPSESSID();

            SaveProfileProgress(beUrl
                , sessionId
                , result.Value0
                , profile
                , currentHealth
                , isPlayerScav);

            return true;
        }

        public static void SaveProfileProgress(
            string backendUrl
            , string session, EFT.ExitStatus exitStatus, EFT.Profile profileData, PlayerHealth currentHealth, bool isPlayerScav)
        {
            SaveProfileRequest request = new()
            {
                exit = exitStatus.ToString().ToLower(),
                profile = profileData,
                health = currentHealth,
                //health = profileData.Health,
                isPlayerScav = isPlayerScav
            };

            var convertedJson = request.MTGAToJson();
            new Request(session, backendUrl).PostJson("/client/raid/profile/save", convertedJson);
           
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
