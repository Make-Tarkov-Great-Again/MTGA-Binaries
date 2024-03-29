﻿using EFT;
using MTGA.Utilities.Core;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MTGA.Patches.Raid.Menus
{
    internal class ForceLocalGamePatch : ModulePatch
    {
        public ForceLocalGamePatch()
        {

        }

        /// <summary>
        /// This should be "method_28"
        /// </summary>
        /// <returns></returns>
        protected override MethodBase GetTargetMethod() => PatchConstants.GetAllMethodsForType(typeof(TarkovApplication))
           .Single(x => x.GetParameters().Length >= 1
               && x.GetParameters()[0].Name == "reconnectAction"
               && x.GetParameters()[0].ParameterType == typeof(Action));

        [PatchPrefix]
        public static bool PatchPrefix(
           TarkovApplication __instance
            , bool ____localGame
           , Task __result
           )
        {
            ____localGame = true;
            return true;
        }
    }
}
