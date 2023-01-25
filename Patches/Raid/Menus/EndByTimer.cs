using EFT;
using MTGA.Utilities.Core;
using System;
using System.Linq;
using System.Reflection;

namespace MTGA.Patches.Raid.Menus
{
    public class EndByTimer : ModulePatch
    {
        private static PropertyInfo _profileIdProperty;
        //private static MethodInfo _stopRaidMethod;

        static EndByTimer()
        {
            _profileIdProperty = Constants.Instance.LocalGameType
                .BaseType
                .GetProperty("ProfileId", Constants.Instance.NonPublicInstanceFlag)
                ?? throw new InvalidOperationException("'ProfileId' property not found");
        }

        public EndByTimer() { }

        protected override MethodBase GetTargetMethod()
        {
            return Constants.Instance.LocalGameType
                .BaseType
                .GetMethods(Constants.Instance.NonPublicInstanceDeclaredOnlyFlag)
                .Single(x => x.Name.EndsWith("StopGame"));  // find explicit interface implementation
        }

        [PatchPrefix]
        private static bool PrefixPatch(EFT.LocalGame __instance)
        {
            var profileId = _profileIdProperty.GetValue(__instance) as string;
            var enabled = Request();

            if (!enabled)
            {
                return true;
            }

            __instance.Stop(profileId, ExitStatus.MissingInAction, null, 0f);
            return false;
        }

        private static bool Request()
        {
            return true;
        }
    }
}
