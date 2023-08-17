using EFT;
using MTGA.Utilities.Core;
using System.Linq;
using System.Reflection;

namespace MTGA.Patches.AI.Fixes
{
    internal class IsPlayerEnemyPatch : ModulePatch
    {
        /*
         * public bool IsPlayerEnemy(GInterface66 requester)
         */
        protected override MethodBase GetTargetMethod()
        {
            return
                PatchConstants.GetMethodForType(
                PatchConstants.EftTypes.Single(x =>
                PatchConstants.GetMethodForType(x, "IsPlayerEnemy") != null), "IsPlayerEnemy");
        }

        [PatchPostfix]
        public static void Postfix(
                    bool __result
                    )
        {
            __result = true;
        }
    }
}
