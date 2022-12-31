using EFT;
using MTGA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MTGA.Patches.Player
{
    internal class OfflineDisplayProgressPatch : ModulePatch
    {
        static OfflineDisplayProgressPatch()
        {
            _ = nameof(MainApplication);
            _ = nameof(EFT.RaidSettings);
        }

        protected override MethodBase GetTargetMethod()
        {
            foreach (var method in PatchConstants.GetAllMethodsForType(PatchConstants.EftTypes.Single(x => x.Name == "MainApplication")))
            {
                if (method.Name.StartsWith("method") &&
                    method.GetParameters().Length >= 5 &&
                    method.GetParameters()[0].Name == "profileId" &&
                    method.GetParameters()[1].Name == "savageProfile" &&
                    method.GetParameters()[2].Name == "location" &&
                    method.GetParameters().Any(x => x.Name == "exitStatus") &&
                    method.GetParameters().Any(x => x.Name == "exitTime")
                    )
                {
                    Logger.LogInfo("OfflineDisplayProgressPatch:Method Name:" + method.Name);
                    return method;
                }
            }
            Logger.Log(BepInEx.Logging.LogLevel.Error, "OfflineDisplayProgressPatch::Method is not found!");

            return null;
        }

        [PatchPrefix]
        public static bool PatchPrefix(
            ref EFT.RaidSettings ____raidSettings
            )
        {
            ____raidSettings.RaidMode = ERaidMode.Online;
            return true;
        }

        [PatchPostfix]
        public static void PatchPostfix(
            ref EFT.RaidSettings ____raidSettings
            )
        {
            ____raidSettings.RaidMode = ERaidMode.Online;
        }
    }
}
