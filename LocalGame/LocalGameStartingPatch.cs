using SIT.A.Tarkov.Core.AI;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.A.Tarkov.Core.LocalGame
{
    // <summary>
    /// Target that smethod_3 like
    /// </summary>
    public class LocalGameStartingPatch : ModulePatch
    {
        public static object LocalGameInstance;
        public static event Action LocalGameStarted;
        protected override MethodBase GetTargetMethod()
        {
            var t = SIT.Tarkov.Core.PatchConstants.EftTypes.FirstOrDefault(x => x.FullName.StartsWith("EFT.LocalGame"));
            if (t == null)
                Logger.LogInfo($"LocalGameStartingPatch:Type is NULL");

            var method = PatchConstants.GetAllMethodsForType(t)
                .FirstOrDefault(x => x.GetParameters().Length >= 3
                && x.GetParameters()[0].Name.Contains("botsSettings")
                && x.GetParameters()[1].Name.Contains("entryPoint")
                && x.GetParameters()[2].Name.Contains("backendUrl")
                );

            Logger.LogInfo($"{ThisTypeName}:{t.Name}:{method.Name}");
            return method;
        }

        [PatchPrefix]
        public static async void PatchPrefix(
            object __instance
            , Task __result
            )
        {
            Logger.LogInfo($"LocalGameStartingPatch:PatchPrefix");
            LocalGameInstance = __instance;
            if (LocalGameStarted != null)
                LocalGameStarted();

            BotSystemHelpers.BotSystemInstance = PatchConstants.GetFieldOrPropertyFromInstance<object>(__instance, BotSystemHelpers.BotSystemType.Name.ToLower() + "_0", false);
            Logger.LogInfo($"LocalGameStartingPatch:BotSystemInstance:" + BotSystemHelpers.BotSystemInstance.GetType().Name);
        }
    }
}
