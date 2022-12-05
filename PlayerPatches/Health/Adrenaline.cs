using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT;
using MTGA.Core;

// Credit goes to Kobrakon
// https://hub.sp-tarkov.com/files/file/860-adrenaline/

namespace MTGA.Core.PlayerPatches.Health
{
    internal class Adrenaline : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod("ReceiveDamage", BindingFlags.Instance | BindingFlags.NonPublic);
        }
        [PatchPostfix]
        public static void Postfix(ref Player __instance, EDamageType type)
        {
            bool flag = type is (EDamageType)512 or (EDamageType)4 or (EDamageType)4096 or (EDamageType)2048 or (EDamageType)32;
            if (flag)
            {
                bool flag2 = __instance.ActiveHealthController.BodyPartEffects.Effects[0].Any((KeyValuePair<string, float> v) => v.Key == "PainKiller");
                if (flag2)
                {
                    (typeof(GHealthController).GetMethod("FindActiveEffect", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(new Type[]
                    {
                typeof(GHealthController).GetNestedType("PainKiller", BindingFlags.Instance | BindingFlags.NonPublic)
                    }).Invoke(__instance.ActiveHealthController, new object[]
                    {
                0
                    }) as GHealthController.StatusEffect).AddWorkTime(new float?(30f), true);
                } // GClass1908 IEffect (StatusEffect)
                else
                { //protected T method_13<T>(EBodyPart bodyPart, float? delayTime = null, float? workTime = null, float? residueTime = null, float? strength = null, Action<T> initCallback = null)
                    MethodInfo method = typeof(GHealthController).GetMethod("method_13", BindingFlags.Instance | BindingFlags.NonPublic);
                    MethodBase methodBase = method.MakeGenericMethod(new Type[]
                    {
                typeof(GHealthController).GetNestedType("PainKiller", BindingFlags.Instance | BindingFlags.NonPublic)
                    });
                    object activeHealthController = __instance.ActiveHealthController;
                    object[] array = new object[6];
                    array[0] = 0;
                    array[1] = 0f;
                    array[2] = 30f;
                    array[3] = 5f;
                    array[4] = 1f;
                    methodBase.Invoke(activeHealthController, array);
                }
            }
        }
    }
}
