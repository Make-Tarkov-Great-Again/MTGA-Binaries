using System.Linq;
using System.Reflection;
using MTGA.Utilities.Core;

namespace MTGA.Patches.Raid.Menus
{
    public class ExitWhileLootingContainer : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.EftTypes.Single(x => x.Name == "LocalGame").BaseType // BaseLocalGame
                .GetMethod("Stop", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        // Look at BaseLocalGame<TPlayerOwner> and find a method named "Stop"
        // once you find it, there should be a StartBlackScreenShow method with
        // a callback method (on dnspy will be called @class.method_0)
        // Go into that method. This will be part of the code:
        //      if (GClass2505.CheckCurrentScreen(EScreenType.Reconnect))
        //		{
        //			GClass2505.CloseAllScreensForced();
        //		}
        // The code INSIDE the if needs to run
        [PatchPrefix]
        private static bool PatchPrefix()
        {
            //ScreenManager instance = ScreenManager.Instance;
            ScreenManager.Instance.CloseAllScreensForced();
            PatchConstants.Logger.LogDebug("It's working I guess?????");
            return true;
        }
    }
}
