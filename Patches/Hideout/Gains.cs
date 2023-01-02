using System.Reflection;
using EFT;
using MTGA.Utilities.Core;
using MTGA_Request = MTGA.Utilities.Core.Request;

namespace MTGA.Patches.Hideout
{
    internal class Gains : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(HideoutPlayerOwner).GetMethod(nameof(HideoutPlayerOwner.StopWorkout));


        [PatchPostfix]
        private static void PatchPostfix(HideoutPlayerOwner __instance)
        {
            MTGA_Request.Instance.PutJson("/client/hideout/workout", new
            {
                skills = __instance.HideoutPlayer.Skills,
                effects = __instance.HideoutPlayer.HealthController.BodyPartEffects
            }.MTGAToJson());
        }
    }
}
