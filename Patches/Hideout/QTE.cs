using System.Reflection;
using EFT;
using MTGA.Utilities.Core;

using MTGA_Request = MTGA.Utilities.Core.Request;


// Credit goes to Kiobu & SamSwat
// https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/development/project/Aki.Custom/Patches/QTEPatch.cs

namespace MTGA.Patches.Hideout
{
    public class QTE : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(HideoutPlayerOwner).GetMethod(nameof(HideoutPlayerOwner.StopWorkout));
        [PatchPostfix]
        private static void PatchPostfix(HideoutPlayerOwner __instance)
        {

            MTGA_Request.Instance.PostJson("/client/hideout/workout", new
            {
                skills = __instance.HideoutPlayer.Skills,
                effects = __instance.HideoutPlayer.HealthController.BodyPartEffects
            }.MTGAToJson()
            , true);
        }
    }
}
