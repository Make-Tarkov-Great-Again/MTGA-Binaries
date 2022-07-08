using SIT.Tarkov.Core;
using System;
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
        static void PrefixPatch(ref bool isOnline)
        {
            isOnline = true;
        }

        [PatchPostfix]
        static void PostfixPatch(ref bool isOnline)
        {
            //isOnline = false;
        }

        protected override MethodBase GetTargetMethod()
        {
            var returnedType = PatchConstants.EftTypes.Single(x =>
                 x.FullName.StartsWith(typeof(EFT.UI.SessionEnd.SessionResultExperienceCount).FullName)
                 && PatchConstants.GetPropertyFromType(x, "KeyScreen") != null
                 );
            //Logger.LogInfo(returnedType.FullName);

            //return PatchConstants.GetAllMethodsForType(returnedType, true).First();
            return returnedType.GetConstructor((new Type[] { typeof(EFT.Profile), typeof(bool), typeof(EFT.ExitStatus) }));
        }

        //private static bool IsTargetMethod(MethodInfo mi)
        //{
        //    var parameters = mi.GetParameters();

        //    return (parameters.Length >= 3
        //        && parameters[0].Name == "profile"
        //        && parameters[1].ParameterType == typeof(bool)
        //        && parameters[1].Name == "isLocal"
        //        && parameters[2].Name == "exitStatus")
        //        ||
        //        (parameters.Length >= 3
        //        && parameters[0].Name == "profile"
        //        && parameters[1].ParameterType == typeof(bool)
        //        && parameters[1].Name == "isOnline"
        //        && parameters[2].Name == "exitStatus");

        //}
    }
}