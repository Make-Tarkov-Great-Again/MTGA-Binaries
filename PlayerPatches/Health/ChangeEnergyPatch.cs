using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.PlayerPatches.Health
{
    internal class ChangeEnergyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.EftTypes
                .LastOrDefault(x => x.GetMethod("ChangeEnergy", BindingFlags.Public | BindingFlags.Instance) != null)
                .GetMethod("ChangeEnergy", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        public static void PatchPostfix(
            object __instance
            , float value
            )
        {
            if (__instance == HealthListener.Instance.MyHealthController)
            {
                //Logger.LogInfo("ChangeEnergyPatch:PatchPostfix:Change on my Health Controller: " + value);
                if(HealthListener.Instance.CurrentHealth.Energy + value > 0)
                    HealthListener.Instance.CurrentHealth.Energy += value;
            }
        }
    }
}
