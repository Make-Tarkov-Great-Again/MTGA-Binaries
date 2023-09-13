using HarmonyLib;
using MTGA.Utilities.Core;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MTGA.Patches.Menus
{
    internal class InstantItemsMoving : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(LocalBackendCommands).GetMethod("TrySendCommands");

        [PatchPrefix]
        public static void PatchPrefix(LocalBackendCommands __instance)
        {

            List<object> list_0 = (List<object>)AccessTools.Field(typeof(LocalBackendCommands), "list_0").GetValue(__instance);


            bool isNullOrEmpty = list_0?.Any<object>() != true;

            if (isNullOrEmpty)
            {
                //Logger.LogInfo("[InstantItemsMoving] Current QueueStatus " + __instance.QueueStatus);
                __instance.QueueStatus = EFT.EOperationQueueStatus.Idle;
                //Logger.LogInfo("[InstantItemsMoving] New QueueStatus " + __instance.QueueStatus);

                //Logger.LogInfo("[InstantItemsMoving] Current IsFlushing " + __instance.IsFlushing);
                __instance.IsFlushing = true;
                //Logger.LogInfo("[InstantItemsMoving] New IsFlushing " + __instance.IsFlushing);

            }
        }
    }
}
