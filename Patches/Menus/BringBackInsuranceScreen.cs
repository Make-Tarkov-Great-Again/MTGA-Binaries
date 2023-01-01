using MTGA.Utilities.Core;
using System.Linq;
using System.Reflection;

namespace MTGA.Patches.Menus
{
    /// <summary>
    /// Enables Insurance screen in offline mode.
    /// </summary>
    public class BringBackInsuranceScreen : ModulePatch
    {
        public BringBackInsuranceScreen() { }

        // don't forget 'ref'
        [PatchPrefix]
        static void PrefixPatch(ref bool local)
        {
            local = false;
        }

        [PatchPostfix]
        static void PostfixPatch(ref bool ___bool_0)
        {
            ___bool_0 = true;
        }

        protected override MethodBase GetTargetMethod()
        {
            // find method 
            // private void method_53(bool local, GStruct73 weatherSettings, GStruct177 botsSettings, GStruct74 wavesSettings)
            return Constants.Instance.MenuControllerType
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .FirstOrDefault(IsTargetMethod);    // controller contains 2 methods with same signature. Usually target method is first of them.
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();

            if (parameters.Length != 4
            || parameters[0].ParameterType != typeof(bool)
            || parameters[0].Name != "local"
            || parameters[1].Name != "weatherSettings"
            || parameters[2].Name != "botsSettings"
            || parameters[3].Name != "wavesSettings")
            {
                return false;
            }

            return true;
        }
    }
}