using MTGA.Utilities.Core;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MTGA.Patches.AntiCheat
{
    /// <summary>
    /// This Patches out Battleye in client so it wont throw message that battleye isn't running and start to closing the game
    /// </summary>
    public class BattlEyePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var methodName = "RunValidation";
            var flags = BindingFlags.Public | BindingFlags.Instance;

            return PatchConstants.EftTypes.Single(x => x.GetMethod(methodName, flags) != null)
                .GetMethod(methodName, flags);
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref Task __result, ref bool ___bool_0)
        {
            ___bool_0 = true;
            __result = Task.CompletedTask;
            return false;
        }
    }
}
