using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT;
using MTGA.Core;

// Credit goes to Kobrakon
// https://hub.sp-tarkov.com/files/file/860-adrenaline/
// Adds a 30 sec painkiller effect when the player is shot/caught in an explosion to simulate irl adrenaline spikes so you're not a sudden limping mess anymore

namespace MTGA.Patches.Player.Health
{
    internal class AdrenalineRealismPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(EFT.Player).GetMethod("ReceiveDamage", BindingFlags.Instance | BindingFlags.NonPublic);
        }
        [PatchPostfix]
        public static void PatchPostfix(ref EFT.Player __instance, EDamageType type)
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
                { //private T method_12<T>(EBodyPart bodyPart, Item effectSourceItem, float? strength = null, float? delay = null, float? duration = null, float? residueTime = null, Action<T> initCallback = null)
                    MethodInfo method = typeof(GHealthController).GetMethod("method_12", BindingFlags.Instance | BindingFlags.NonPublic);
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
