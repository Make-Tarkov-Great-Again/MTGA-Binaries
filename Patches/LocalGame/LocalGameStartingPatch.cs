using MTGA.Utilities.AI;
using MTGA.Utilities.Core;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MTGA.Patches.LocalGame
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
            var t = PatchConstants.EftTypes.FirstOrDefault(x => x.FullName.StartsWith("EFT.LocalGame"));
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
        public static void PatchPrefix(
            object __instance
            , Task __result
            )
        {
            Logger.LogInfo($"LocalGameStartingPatch:PatchPrefix");
            LocalGameInstance = __instance;
            LocalGameStarted?.Invoke();

            BotSystemHelpers.BotControllerInstance = PatchConstants.GetFieldOrPropertyFromInstance<object>(__instance, BotSystemHelpers.BotControllerType.Name.ToLower() + "_0", false);
            Logger.LogInfo($"LocalGameStartingPatch:BotSystemInstance:" + BotSystemHelpers.BotControllerInstance.GetType().Name);
        }
    }
}
