using MTGA.Core;
using MTGA.Core.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
