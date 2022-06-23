using EFT;
using SIT.Tarkov.Core;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SIT.Tarkov.Core.Raid
{
    public class EndByTimer : ModulePatch
    {
        private static PropertyInfo _profileIdProperty;
        private static MethodInfo _stopRaidMethod;

        static EndByTimer()
        {
            _profileIdProperty = Constants.Instance.LocalGameType
                .BaseType
                .GetProperty("ProfileId", Constants.Instance.NonPublicInstanceFlag)
                ?? throw new InvalidOperationException("'ProfileId' property not found");

            // find method
            // protected void method_11(string profileId, ExitStatus exitStatus, string exitName, float delay = 0f)
            _stopRaidMethod = Constants.Instance.LocalGameType
                .BaseType
                .GetMethods(Constants.Instance.NonPublicInstanceDeclaredOnlyFlag)
                .SingleOrDefault(IsStopRaidMethod)
                ?? throw new InvalidOperationException("Method not found");
        }

        private static bool IsStopRaidMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            if (parameters.Length != 4
            || parameters[0].ParameterType != typeof(string)
            || parameters[0].Name != "profileId"
            || parameters[1].ParameterType != typeof(ExitStatus)
            || parameters[1].Name != "exitStatus"
            || parameters[2].ParameterType != typeof(string)
            || parameters[2].Name != "exitName"
            || parameters[3].ParameterType != typeof(float)
            || parameters[3].Name != "delay")
            {
                return false;
            }

            return true;
        }

        public EndByTimer()  { }

        protected override MethodBase GetTargetMethod()
        {
            return Constants.Instance.LocalGameType
                .BaseType
                .GetMethods(Constants.Instance.NonPublicInstanceDeclaredOnlyFlag)
                .Single(x => x.Name.EndsWith("StopGame"));  // find explicit interface implementation
        }

        [PatchPrefix]
        private static bool PrefixPatch(object __instance)
        {
            var profileId = _profileIdProperty.GetValue(__instance) as string;
            var enabled = Request();

            if (!enabled)
            {
                return true;
            }

            _stopRaidMethod.Invoke(__instance, new object[] { profileId, ExitStatus.MissingInAction, null, 0f });
            return false;
        }

        private static bool Request()
        {
            return true;
        }
    }
}
