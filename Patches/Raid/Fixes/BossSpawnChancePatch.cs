using MTGA.Utilities.Core;
using System.Linq;
using System.Reflection;

namespace MTGA.Patches.Raid.Fixes
{
    /// <summary>
    /// This patch will enable usage of location bos spawn chances in offline matches - by default bosses will spawn 100% or 0% depends on option selected in menu
    /// </summary>
    internal class BossSpawnChancePatch : ModulePatch
    {
        private static float[] bossSpawnPercent;

        public BossSpawnChancePatch() { }

        [PatchPrefix]
        static void PrefixPatch(BossLocationSpawn[] bossLocationSpawn)
        {
            bossSpawnPercent = bossLocationSpawn.Select(s => s.BossChance).ToArray();
        }

        [PatchPostfix]
        static void PostfixPatch(ref BossLocationSpawn[] __result)
        {
            if (__result.Length != bossSpawnPercent.Length)
            {
                return;
            }

            for (var i = 0; i < bossSpawnPercent.Length; i++)
            {
                __result[i].BossChance = bossSpawnPercent[i];
            }
        }

        protected override MethodBase GetTargetMethod()
        {
            return Constants.Instance.LocalGameType.BaseType
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .SingleOrDefault(m => IsTargetMethod(m));
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return parameters.Length != 2 || parameters[0].Name != "wavesSettings" || parameters[1].Name != "bossLocationSpawn" ? false : true;
        }
    }
}
