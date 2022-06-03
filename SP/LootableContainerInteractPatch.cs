using EFT.Interactive;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.A.Tarkov.Core.SP
{
    internal class LootableContainerInteractPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.GetAllMethodsForType(PatchConstants.EftTypes.Single(x => x.Name == "LootableContainer")).Single(x => x.Name == "Interact");
        }

        [PatchPostfix]
        public static void PatchPostfix(LootableContainer __instance, object interactionResult)
        {
            Logger.LogInfo($"LootableContainerInteractPatch:PatchPostfix");
            Logger.LogInfo($"{__instance.Id}");
            Logger.LogInfo($"{__instance.ItemOwner.Name}");
        }
    }
}
