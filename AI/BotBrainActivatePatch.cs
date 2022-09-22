using System.Reflection;

namespace MTGA.Core.AI
{
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
