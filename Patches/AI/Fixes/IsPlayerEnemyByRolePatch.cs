using EFT;
using MTGA.Utilities.Core;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MTGA.Patches.AI.Fixes
{
    internal class IsPlayerEnemyByRolePatch : ModulePatch
    {
        /*
         * public bool IsPlayerEnemyByRole(WildSpawnType role)
         */
        protected override MethodBase GetTargetMethod()
        {
            return
                PatchConstants.GetMethodForType(
                PatchConstants.EftTypes.Single(x =>
                PatchConstants.GetMethodForType(x, "IsPlayerEnemyByRole") != null), "IsPlayerEnemyByRole");
        }

        [PatchPostfix]
        public static void Postfix(
            bool __result,
            WildSpawnType role,
            BotGroupClass __instance
            )
        {
            __result = true;
        }
    }
}
