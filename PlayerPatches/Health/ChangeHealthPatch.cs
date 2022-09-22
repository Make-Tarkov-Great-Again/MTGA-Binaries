using MTGA.Core;
using MTGA.SP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MTGA.Core.PlayerPatches.Health
{
    internal class ChangeHealthPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var t = PatchConstants.EftTypes
                .First(x =>
                    PatchConstants.GetMethodForType(x, "ChangeHealth") != null
                    && PatchConstants.GetMethodForType(x, "Kill") != null
                    && PatchConstants.GetMethodForType(x, "DoPainKiller") != null
                    && PatchConstants.GetMethodForType(x, "DoScavRegeneration") != null
                    && x.GetMethod("GetOverallHealthRegenTime", BindingFlags.Public | BindingFlags.Instance) == null // We don't want this one
                    );
            Logger.LogInfo("ChangeHealth:" + t.FullName);
            var method = PatchConstants.GetMethodForType(t, "ChangeHealth");

            Logger.LogInfo("ChangeHealth:" + method.Name);
            return method;
        }

        [PatchPostfix]
        public static void PatchPostfix(
            object __instance
            , EBodyPart bodyPart
            , float value
            , object damageInfo)
        {
            //if (HealthListener.Instance.MyHealthController == null)
            //{
            //    Logger.LogInfo("ChangeHealthPatch:MyHealthController is NULL");
            //}

            if (__instance == HealthListener.Instance.MyHealthController)
            {
                //Logger.LogInfo("ChangeHealthPatch:PatchPostfix:Change on my Health Controller: " + value);
                HealthListener.Instance.CurrentHealth.Health[bodyPart].ChangeHealth(value);
                //Logger.LogInfo("ChangeHealthPatch:PatchPostfix:Type:" + __instance.GetType());
            }
        }
    }
}
