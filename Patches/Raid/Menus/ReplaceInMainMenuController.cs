//using MainMenuController = GClass1504; // SelectedDateTime
//using IHealthController = GInterface195; // CarryingWeightAbsoluteModifier
using System.Reflection;
using System.Linq;
using MTGA.Utilities.Core;
using MTGA.Utilities.Player.Health;

namespace MTGA.Patches.Raid.Menus
{
    public class ReplaceInMainMenuController : ModulePatch
    {
        static ReplaceInMainMenuController()
        {
            //_ = nameof(IHealthController.HydrationChangedEvent);
            //_ = nameof(MainMenuController.HealthController);
        }

        public ReplaceInMainMenuController() { }

        protected override MethodBase GetTargetMethod()
        {
            var mmc = PatchConstants.EftTypes.Single(x =>
                (PatchConstants.GetFieldFromType(x, "HealthController") != null
                || PatchConstants.GetPropertyFromType(x, "HealthController") != null)
                &&
                 (PatchConstants.GetFieldFromType(x, "SelectedDateTime") != null
                || PatchConstants.GetPropertyFromType(x, "SelectedDateTime") != null)
                );
            var m = mmc.GetMethod("method_1", BindingFlags.NonPublic | BindingFlags.Instance);
            return m;

        }

        [PatchPostfix]
        public static void PatchPostfix(object __instance)
        {
            //Logger.LogInfo("ReplaceInMainMenuController.PatchPostfix type");
            //Logger.LogInfo(__instance.GetType().FullName);

            //var healthController = __instance.GetType().GetProperty("HealthController", BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);
            var healthController = PatchConstants.GetFieldOrPropertyFromInstance<object>(__instance, "HealthController", false);// __instance.HealthController;
            if (healthController != null)
            {
                //Logger.LogInfo("ReplaceInMainMenuController.PatchPostfix.HealthController found!");
                Logger.LogInfo(healthController.GetType().FullName);

                //Logger.LogInfo("HealthController props!");
                //foreach(var p in PatchConstants.GetAllPropertiesForObject(healthController))
                //{
                //    Logger.LogInfo(p.Name);
                //}
                //Logger.LogInfo("HealthController fields!");
                //foreach (var p in PatchConstants.GetAllFieldsForObject(healthController))
                //{
                //    Logger.LogInfo(p.Name);
                //}

                var listener = HealthListener.Instance;
                listener.Init(healthController, false);
            }
            else
            {
                Logger.LogInfo("ReplaceInMainMenuController.PatchPostfix.HealthController not found!");
            }
        }
    }
}
