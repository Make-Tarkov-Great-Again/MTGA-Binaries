using MTGA.Utilities.AI;
using MTGA.Utilities.Core;
using System.Reflection;

namespace MTGA.Patches.AI
{
    /// <summary>
    /// Reenable BotBrain - not sure what it should do exacly... ~mao
    /// </summary>
    internal class BotBrainActivatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.GetMethodForType(BotSystemHelpers.TypeDictionary["BotBrain"], "Activate");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return true;
        }

        [PatchPostfix]
        public static void PatchPostfix()
        {
        }
    }
}
