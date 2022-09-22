using System.Linq;
using System.Reflection;

namespace MTGA.Core.AI
{
    public class IsEnemyPatch : ModulePatch
    {
        /*
         * public bool IsEnemy(GInterface66 requester)
         */
        protected override MethodBase GetTargetMethod()
        {
            return
                PatchConstants.GetMethodForType(
                PatchConstants.EftTypes.Single(x =>
                PatchConstants.GetMethodForType(x, "IsEnemy") != null), "IsEnemy");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return false;
        }


        [PatchPostfix]
        public static void PatchPostfix(ref bool __result, object requester)
        {
            //var otherPlayerHealthController = HealthControllerHelpers.GetActiveHealthController(requester);
            //var otherPlayerIsAlive = HealthControllerHelpers.IsAlive(otherPlayerHealthController);
            //__result = otherPlayerIsAlive;
            __result = true;
        }
    }
}
