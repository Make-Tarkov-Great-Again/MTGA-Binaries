﻿using EFT;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MTGA.Core.SP
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
        protected override MethodBase GetTargetMethod() => PatchConstants.GetAllMethodsForType(typeof(MainApplication))
           .Single(x => x.GetParameters().Length >= 1
               && x.GetParameters()[0].Name == "reconnectAction"
               && x.GetParameters()[0].ParameterType == typeof(Action));

        [PatchPrefix]
        public static bool PatchPrefix(
           MainApplication __instance
            , bool ____localGame
           , Task __result
           )
        {
            ____localGame = true;
            return true;
        }
    }
}
