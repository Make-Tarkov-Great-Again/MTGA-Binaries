using EFT;
using MTGA.Utilities.Core;
using MTGA.Utilities.Player.Health;
using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine.Profiling;

namespace MTGA.Patches.Player.Fixes
{
    internal class PlayerInitPatch : ModulePatch
    {
        public static event Action<EFT.LocalPlayer> OnPlayerInit;

        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.GetMethodForType(typeof(EFT.LocalPlayer), "Init");
        }

        [PatchPostfix]
        public static async void PatchPostfix(Task __result, LocalPlayer __instance, Profile profile)
        {

            if (__instance is HideoutPlayer)
                return;

            OnPlayerInit?.Invoke(__instance);

            await __result;

            var listener = HealthListener.Instance;
            if (profile?.PetId != null && __instance.IsYourPlayer)
            {
                Logger.LogInfo($"Hooking up health listener to profile: {profile.Id}");
                listener.Init(__instance.HealthController, true);
                return;
                //Logger.LogInfo($"HealthController instance: {__instance.HealthController.GetHashCode()}");
            }

            //Logger.LogInfo($"Skipped on HealthController instance: {__instance.HealthController.GetHashCode()} for profile id: {profile?.Id}");

        }
    }
}
