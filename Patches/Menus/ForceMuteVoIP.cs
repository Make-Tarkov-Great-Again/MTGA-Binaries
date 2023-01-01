using System.Reflection;
using EFT;
using MTGA.Utilities.Core;

namespace MTGA.Menus
{
    /// <summary>
    /// Disables VoIP - Initially made by "kiobu", "Terkoiz", "SamSwat"
    /// </summary>
    public class ForceMuteVoIP : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ForceMuteVoIPToggler).GetMethod("Awake", PatchConstants.PrivateFlags);
        }
        [PatchPrefix]
        private static bool PatchPrefix()
        {
            return false;
        }
    }
}
