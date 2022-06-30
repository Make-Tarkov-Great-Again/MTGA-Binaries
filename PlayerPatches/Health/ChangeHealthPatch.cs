using SIT.Tarkov.Core;
using SIT.Tarkov.SP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.PlayerPatches.Health
{
    internal class ChangeHealthPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.EftTypes
                .LastOrDefault(x => x.GetMethod("ChangeHealth", BindingFlags.Public | BindingFlags.Instance) != null)
                .GetMethod("ChangeHealth", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        public static void PatchPostfix(
            object __instance
            , EBodyPart bodyPart
            , float value
            , object damageInfo)
        {
            if (__instance == HealthListener.Instance.MyHealthController)
            {
                //Logger.LogInfo("ChangeHealthPatch:PatchPostfix:Change on my Health Controller: " + value);
                HealthListener.Instance.CurrentHealth.Health[bodyPart].ChangeHealth(value);
                //Logger.LogInfo("ChangeHealthPatch:PatchPostfix:Type:" + __instance.GetType());
            }
        }
    }
}
