using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.PlayerPatches
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
            if(OnPlayerInit != null)
                OnPlayerInit(__instance);

            //PatchConstants.DisplayMessageNotification($"{__instance.Profile.Nickname}:{__instance.Side}:{__instance.Profile.Info.Settings.Role} has spawned");
        }
    }
}
