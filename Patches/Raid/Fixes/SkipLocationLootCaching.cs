
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
            if (desiredType == null)
            {
                Logger.LogError("Desired Type not found");
            }

            var methods = PatchConstants.GetAllMethodsForType(desiredType);
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();

                if (parameters.Length == 3
                    && parameters[0].Name == "backendUrl"
                    && parameters[1].Name == "locationId"
                    && parameters[2].Name == "variantId")
                {
                    //Logger.Log(BepInEx.Logging.LogLevel.Info, method.Name);
                    return method;
                }
            }

            Logger.LogError("Method not found");
            return null;
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return false; // skip original
        }
    }
}
