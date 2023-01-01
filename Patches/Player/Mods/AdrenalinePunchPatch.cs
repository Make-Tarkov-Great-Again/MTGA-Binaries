using System;
using System.Linq;
using System.Reflection;
using EFT;
using MTGA.Utilities.Core;
namespace MTGA.Patches.Player.Mods
{
    /// <summary>
    /// Adrenaline punch when getting shot at. Made initially by "Kobrakon"
    /// https://hub.sp-tarkov.com/files/file/860-adrenaline/
    /// </summary>
    internal class AdrenalinePunchPatch : ModulePatch
    {

        protected override MethodBase GetTargetMethod() => PatchConstants.GetAllMethodsForType(typeof(EFT.Player))
        .Single(IsTargetMethod);

        private bool IsTargetMethod(MethodInfo method)
        {
            var parameters = method.GetParameters();

            if (parameters.Length != 5
            || parameters[0].ParameterType != typeof(float)
            || parameters[0].Name != "damage"
            || parameters[1].ParameterType != typeof(EBodyPart)
            || parameters[1].Name != "part"
            || parameters[2].ParameterType != typeof(EDamageType)
            || parameters[2].Name != "type"
            || parameters[3].ParameterType != typeof(float)
            || parameters[3].Name != "absorbed"
            || parameters[4].ParameterType != typeof(EFT.Ballistics.MaterialType)
            || parameters[4].Name != "special")
            {
                return false;
            }

            return true;
        }


        [PatchPostfix]
        public static void PatchPostfix(EFT.Player __instance, EDamageType type)
        {
            if (type == EDamageType.Bullet || type == EDamageType.Explosion || type == EDamageType.Sniper || type == EDamageType.Landmine || type == EDamageType.GrenadeFragment)
            {
                try
                {
                    if (__instance.ActiveHealthController.BodyPartEffects.Effects[0].Any(v => v.Key == "PainKiller"))
                    {
                        GHealthController.StatusEffect pk = typeof(GHealthController)
                            .GetMethod("FindActiveEffect", BindingFlags.Instance | BindingFlags.Public)
                            .MakeGenericMethod(typeof(GHealthController)
                            .GetNestedType("PainKiller", BindingFlags.Instance | BindingFlags.NonPublic))
                            .Invoke(__instance.ActiveHealthController, new object[] { EBodyPart.Head }) as GHealthController.StatusEffect;
                        if (pk.TimeLeft < 30) pk.AddWorkTime(30f, false);
                        return;
                    }
                    MethodInfo method = typeof(GHealthController)
                        .GetMethod("method_13", BindingFlags.Instance | BindingFlags.NonPublic);
                    method.MakeGenericMethod(typeof(GHealthController)
                        .GetNestedType("PainKiller", BindingFlags.Instance | BindingFlags.NonPublic))
                        .Invoke(__instance.ActiveHealthController, new object[] { EBodyPart.Head, 0f, 30f, 5f, 1f, null });
                }
                catch (Exception) { }
            }
        }
    }
}
