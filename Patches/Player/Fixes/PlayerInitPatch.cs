using MTGA.Utilities.Core;
using System;
using System.Reflection;

namespace MTGA.Patches.Player.Fixes
{
    internal class PlayerInitPatch : ModulePatch
    {
        public static event Action<EFT.LocalPlayer> OnPlayerInit;

        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.GetMethodForType(typeof(EFT.LocalPlayer), "Init");
        }

        [PatchPostfix]
        public static void PatchPostfix(EFT.LocalPlayer __instance)
        {
            if (OnPlayerInit != null)
                OnPlayerInit(__instance);

            //if (__instance.IsAI)
            //{
            //    BotSystemHelpers.AddActivePlayer(__instance);
            //}

            //PatchConstants.DisplayMessageNotification($"{__instance.Profile.Nickname}:{__instance.Side}:{__instance.Profile.Info.Settings.Role} has spawned");
        }
    }
}
