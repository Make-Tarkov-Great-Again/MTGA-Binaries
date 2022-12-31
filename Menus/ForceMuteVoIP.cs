using System.Linq;
using System.Reflection;
using EFT;

//Credit to kiobu, Terkoiz && SamSwat

namespace MTGA.Core.Menus
{
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
