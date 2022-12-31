using EFT.Interactive;
using MTGA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MTGA.Patches.Misc
{

    // EFT.Interactive.Location.CollectLootPointsData
    internal class CollectLootPointsDataPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.GetMethodForType(typeof(EFT.Interactive.Location), "CollectLootPointsData");
        }

        [PatchPostfix]
        public static void PatchPostfix(
            EFT.Interactive.Location __instance
            
            )
        {
            LootPoint[] ___ValidLootPoints = PatchConstants.GetFieldOrPropertyFromInstance<LootPoint[]>(__instance, "ValidLootPoints", false);
            LootPoint[] ___EmptyLootPoints = PatchConstants.GetFieldOrPropertyFromInstance<LootPoint[]>(__instance, "EmptyLootPoints", false);
            LootPoint[] ___DisabledLootPoints = PatchConstants.GetFieldOrPropertyFromInstance<LootPoint[]>(__instance, "DisabledLootPoints", false);
            LootPoint[] ___LootPoints = PatchConstants.GetFieldOrPropertyFromInstance<LootPoint[]>(__instance, "LootPoints", false);
            LootableContainer[] ___ValidLootableContainers = PatchConstants.GetFieldOrPropertyFromInstance<LootableContainer[]>(__instance, "ValidLootableContainers", false);
            LootableContainer[] ___EmptyLootableContainers = PatchConstants.GetFieldOrPropertyFromInstance<LootableContainer[]>(__instance, "EmptyLootableContainers", false);
            LootableContainer[] ___DisabledLootableContainers = PatchConstants.GetFieldOrPropertyFromInstance<LootableContainer[]>(__instance, "DisabledLootableContainers", false);
            LootableContainer[] ___LootableContainers = PatchConstants.GetFieldOrPropertyFromInstance<LootableContainer[]>(__instance, "LootableContainers", false);
           // object DailyQuestZones // this is a GClass =(


            Logger.LogInfo($"Empty Containers : {___EmptyLootableContainers.Length}");
            foreach (var item in ___EmptyLootableContainers)
            {
                Logger.LogInfo($"{item.Id}");
                Logger.LogInfo($"{item.ItemOwner.ID}");
                Logger.LogInfo($"{item.ItemOwner.Name}");
                Logger.LogInfo($"{item.ItemOwner.ContainerName}");
            }

            Logger.LogInfo($"Disabled Containers : {___DisabledLootableContainers.Length}");
            foreach (var item in ___DisabledLootableContainers)
            {
                Logger.LogInfo($"{item.Id}");
                Logger.LogInfo($"{item.ItemOwner.ID}");
                Logger.LogInfo($"{item.ItemOwner.Name}");
                Logger.LogInfo($"{item.ItemOwner.ContainerName}");
            }
        }
    }
}
