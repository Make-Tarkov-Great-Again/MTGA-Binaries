using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.A.Tarkov.Core.Hideout
{
    internal class ItemRequirementPanelShowPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.GetMethodForType(typeof(EFT.Hideout.ItemRequirementPanel), "Show");
        }

        [PatchPrefix]
        public static void PatchPrefix(
            object __instance,
            EFT.UI.ItemUiContext itemUiContext, object inventoryController, object requirement, EFT.EAreaType areaType, System.Boolean ignoreFulfillment
            )
        {
            Logger.LogInfo($"ItemRequirementPanelShowPatch:PatchPrefix");
            if (requirement == null)
            {
                Logger.LogInfo($"ItemRequirementPanelShowPatch:requirement is Null");
            }

            if (inventoryController == null)
            {
                Logger.LogInfo($"ItemRequirementPanelShowPatch:inventoryController is Null");
            }

            if (itemUiContext == null)
            {
                Logger.LogInfo($"ItemRequirementPanelShowPatch:itemUiContext is Null");
            }
        }
    }
}

//EFT.Hideout.ItemRequirementPanel.Show