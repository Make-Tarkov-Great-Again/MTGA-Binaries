using SIT.Tarkov.Core;
using SIT.Tarkov.Core.LocalGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.AI
{
    internal class LocalGameSpawnAICoroutinePatch : ModulePatch
    {
        /*
        protected virtual IEnumerator vmethod_3(float startDelay, GStruct252 controllerSettings, GInterface250 spawnSystem, Callback runCallback)
        */

        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.GetAllMethodsForType(LocalGameStartingPatch.LocalGameInstance.GetType())
                .Single(
                m =>
                m.IsVirtual
                && m.GetParameters().Length >= 4
                && m.GetParameters()[0].ParameterType == typeof(float)
                && m.GetParameters()[0].Name == "startDelay"
                && m.GetParameters()[1].Name == "controllerSettings"
                );
        }
        
        [PatchPrefix]
        public static bool PatchPrefix(
        float startDelay, object controllerSettings, object spawnSystem, object runCallback,
        object ___wavesSpawnScenario_0
        )
        {
            return true;
        }
    }

    
}
