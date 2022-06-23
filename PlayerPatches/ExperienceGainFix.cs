using SIT.Tarkov.Core;
using System.Linq;
using System.Reflection;

namespace SIT.Tarkov.Core
{
    public class ExperienceGainFix : ModulePatch
    {
        public ExperienceGainFix()
        {

        }

        [PatchPrefix]
        static void PrefixPatch(ref bool isLocal)
        {
            isLocal = false;
        }

        [PatchPostfix]
        static void PostfixPatch(ref bool isLocal)
        {
            isLocal = true;
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(EFT.UI.SessionEnd.SessionResultExperienceCount)
                 .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                 .FirstOrDefault(IsTargetMethod);
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();

            if (parameters.Length != 3
                || parameters[0].Name != "profile"
                || parameters[1].ParameterType != typeof(bool)
                || parameters[1].Name != "isLocal"
                || parameters[2].Name != "exitStatus")
            {
                return false;
            }

            return true;
        }
    }
}