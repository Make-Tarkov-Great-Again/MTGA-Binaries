using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.A.Tarkov.Core.Hideout
{
    internal class HideoutItemViewFactoryShowPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.GetMethodForType(typeof(EFT.Hideout.HideoutItemViewFactory), "Show");
        }

        [PatchPrefix]
        public static void PatchPrefix(
            object __instance,
            ref EFT.InventoryLogic.Item item, ref object inventoryController, ref EFT.UI.ItemUiContext itemUiContext
            )
        {
            Logger.LogInfo($"HideoutItemViewFactoryShowPatch:PatchPrefix");
            if(item == null)
            {
                Logger.LogInfo($"HideoutItemViewFactoryShowPatch:item is Null");
            }

            if (inventoryController == null)
            {
                Logger.LogInfo($"HideoutItemViewFactoryShowPatch:inventoryController is Null");
            }

            if (itemUiContext == null)
            {
                Logger.LogInfo($"HideoutItemViewFactoryShowPatch:itemUiContext is Null");
            }
        }
    }
}
