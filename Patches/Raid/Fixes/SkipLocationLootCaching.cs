using System.Linq;
using System.Reflection;
using MTGA.Utilities.Core;

namespace MTGA.Patches.Raid.Fixes
{
    /// BaseLocalGame appears to cache a maps loot data and reuse it when the variantId from method_6 is the same, this patch exits the method early, never caching the data
    public class SkipLocationLootCaching : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = PatchConstants.EftTypes.Single(x => x.Name == "LocalGame").BaseType; // BaseLocalGame
            var desiredMethod = desiredType.GetMethods(PatchConstants.PrivateFlags).Single(x => IsTargetMethod(x)); // method_6

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return parameters.Length == 3
                && parameters[0].Name == "backendUrl"
                && parameters[1].Name == "locationId"
                && parameters[2].Name == "variantId";
        }

        [PatchPrefix]
        private static bool PatchPrefix()
        {
            return false; // skip original
        }
    }
}
