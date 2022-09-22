using System;
using System.Reflection;

namespace MTGA.Core.PlayerPatches
{
    internal class PlayerInitPatch : ModulePatch
    {
        public static event Action<EFT.LocalPlayer> OnPlayerInit;

        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.GetMethodForType(typeof(EFT.LocalPlayer), "Init");
        }

        [PatchPostfix]
        public static
            async
            void
            PatchPostfix(EFT.LocalPlayer __instance)
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
