using MTGA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MTGA.Core.Misc
{
    internal class LootContainerInitPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.GetMethodForType(typeof(EFT.Interactive.LootableContainer), "Init");
        }

        [PatchPostfix]
        public static void PatchPostfix(
            EFT.Interactive.LootableContainer __instance
            )
        {
            if (__instance.ItemOwner.RootItem == null)
            {
                Logger.LogInfo($"LootContainerInitPatch:PatchPostfix:Root Item is NULL!");
                Logger.LogInfo($"{__instance.Id}");
                Logger.LogInfo($"{__instance.ItemOwner.ID}");
                Logger.LogInfo($"{__instance.ItemOwner.Name}");
                Logger.LogInfo($"{__instance.ItemOwner.ContainerName}");
            }

            if (__instance.ItemOwner.Items.Count() == 0)
            {
                Logger.LogInfo($"LootContainerInitPatch:PatchPostfix:Item Count is 0!");
                Logger.LogInfo($"{__instance.Id}");
                Logger.LogInfo($"{__instance.ItemOwner.ID}");
                Logger.LogInfo($"{__instance.ItemOwner.Name}");
                Logger.LogInfo($"{__instance.ItemOwner.ContainerName}");
            }
        }
    }
}
