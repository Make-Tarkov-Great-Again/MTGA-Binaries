using EFT;
using EFT.Game.Spawning;
using MTGA.Utilities.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MTGA.Patches.Raid.Fixes
{
    /// <summary>
    /// This patch fixes initial spawn points where there was no differenciation between them and you were spawned in 1-2 places all the time
    /// </summary>
    internal class SpawnPointPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            //search for SelectSpawnPoint in dnSpy to find this one
            return PatchConstants.EftTypes.First(IsTargetType)
                .GetMethods(PatchConstants.PrivateFlags).First(m => m.Name.Contains("SelectSpawnPoint"));
        }

        private static bool IsTargetType(Type type)
        {
            //search for CheckFarthestFromOtherPlayers in dnSpy to find this one
            return type.GetMethods(PatchConstants.PrivateFlags).Any(x => x.Name.IndexOf("CheckFarthestFromOtherPlayers", StringComparison.OrdinalIgnoreCase) != -1)
                && type.IsClass;
        }

        [PatchPrefix]
        private static bool PatchPrefix(
            ref ISpawnPoint __result,
            object __instance,
            ESpawnCategory category,
            EPlayerSide side,
            string groupId,
            object person,
            string infiltration)
        {
            //var spawnPointInterface = Traverse.Create(__instance).Field<ISpawnPoints>("ginterface250_0").Value;
            var spawnPointInterface = PatchConstants.GetFieldOrPropertyFromInstance<IEnumerable<ISpawnPoint>>(__instance, PatchConstants.SpawnPointArrayInterfaceType + "_0", false);// Traverse.Create(__instance).Field<ISpawnPoints>("ginterface250_0").Value;

            var spawnPoints = spawnPointInterface.ToList();
            var unfilteredSpawnPoints = spawnPoints.ToList();

            spawnPoints = spawnPoints.Where(sp => sp?.Infiltration != null && (string.IsNullOrEmpty(infiltration) || sp.Infiltration.Equals(infiltration))).ToList();
            spawnPoints = spawnPoints.Where(sp => sp.Categories.Contain(category)).ToList();
            spawnPoints = spawnPoints.Where(sp => sp.Sides.Contain(side)).ToList();

            __result = spawnPoints.Count == 0 ? GetFallBackSpawnPoint(unfilteredSpawnPoints, category, side, infiltration) : spawnPoints.RandomElement();
            Logger.LogInfo($"PatchPrefix SelectSpawnPoint: {__result.Id}");
            return false;
        }

        private static ISpawnPoint GetFallBackSpawnPoint(List<ISpawnPoint> spawnPoints, ESpawnCategory category, EPlayerSide side, string infiltration)
        {
            Logger.LogWarning($"PatchPrefix SelectSpawnPoint: Couldn't find any spawn points for: {category} | {side} | {infiltration} using random unfiltered spawn instead");
            return spawnPoints.Where(sp => sp.Categories.Contain(ESpawnCategory.Player)).RandomElement();
        }
    }
}
