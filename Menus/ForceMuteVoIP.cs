using System.Linq;
using System.Reflection;

//Credit to kiobu

namespace MTGA.Core.Menus
{
    public class ForceMuteVoIP_0 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // Class with the method.
            var desiredType = PatchConstants.EftTypes.Single(x => x.Name == "ForceMuteVoIPToggler");

            const BindingFlags methodFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            const string methodName = "method_0";

            // method_0
            var desiredMethod = desiredType.GetMethod(methodName, methodFlags);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }
        [PatchPrefix]
        private static bool PatchPrefix()
        {
            return false;
        }
    }

    public class ForceMuteVoIP_1 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // Class with the method.
            var desiredType = PatchConstants.EftTypes.Single(x => x.Name == "ForceMuteVoIPToggler");

            const BindingFlags methodFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            const string methodName = "method_1";

            // method_1
            var desiredMethod = desiredType.GetMethod(methodName, methodFlags);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }
        [PatchPrefix]
        private static bool PatchPrefix()
        {
            return false;
        }
    }
}
