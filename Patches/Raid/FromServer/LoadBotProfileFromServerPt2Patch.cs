using MTGA.Utilities.Core;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MTGA.Patches.Raid.FromServer
{
    internal class LoadBotProfileFromServerPt2Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            LoadBotProfileFromServerPt1Patch.GetTargetType().GetMethods(LoadBotProfileFromServerPt1Patch.BindingFlags).FirstOrDefault(x =>
            LoadBotProfileFromServerPt1Patch.IsTargetMethod(x));


        [PatchPrefix]
        public static bool PatchPrefix(object __instance, object data, EFT.Profile __result, List<EFT.Profile> ___list_0)
        {
            return LoadBotProfileFromServerPt1Patch.ActualPatch(__instance, data, __result, ___list_0);
        }
    }
}
