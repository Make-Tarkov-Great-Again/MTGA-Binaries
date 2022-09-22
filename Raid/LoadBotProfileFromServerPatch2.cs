﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MTGA.Core.SP.Raid
{
    internal class LoadBotProfileFromServerPatch2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            LoadBotProfileFromServerPatch.GetTargetType().GetMethods(LoadBotProfileFromServerPatch.BindingFlags).FirstOrDefault(x =>
            LoadBotProfileFromServerPatch.IsTargetMethod(x));


        [PatchPrefix]
        public static bool PatchPrefix(object __instance, object data, EFT.Profile __result, List<EFT.Profile> ___list_0)
        {
            return LoadBotProfileFromServerPatch.ActualPatch(__instance, data, __result, ___list_0);
        }
    }
}
