using EFT;
using System.Linq;
using System.Reflection;

namespace MTGA.Core.PlayerPatches
{
    public class WeaponDrawSpeed : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // need to find GClass in Player.FirearmController that has method Start with parameters (onHidden, fastDrop, nextControllerItem)
            //Current GClass is GClass1425 that has Start in it that we need
            Logger.LogInfo("Getting Target Method for Changing Weapon Swap Speed");
            var playerTypes = typeof(Player.FirearmController).Assembly.GetTypes().Where(x => x.FullName.Contains("EFT.Player+FirearmController+GClass")).OrderBy(t => t.FullName).ToArray();
            Logger.LogInfo(playerTypes);
            foreach (var method in playerTypes)
            {
                Logger.LogInfo("Method: " + method.FullName);
                if (method.GetMethod("Start", BindingFlags.Instance | BindingFlags.Public) != null)
                {
                    var balle = method.GetMethod("Start", BindingFlags.Instance | BindingFlags.Public);
                    Logger.LogInfo("getMethod: " + balle.Name);

                    if (balle.GetParameters().IsNullOrEmpty() == false)
                    {
                        //Logger.LogInfo("Method Parameters: " + balle.GetPr());
                        ////we're getting here and it's crashing
                        //if (balle.GetParameters().Length != 3 &&
                        //    balle.GetParameters()[0].Name != "onHidden" &&
                        //    balle.GetParameters()[1].Name != "fastDrop" &&
                        //    balle.GetParameters()[2].Name != "nextControllerItem")
                        //{
                        //    Logger.LogInfo("Found: " + balle.Name + ", returning!");
                        //    return balle;
                        //}
                        Logger.LogInfo("Method: " + method.FullName + " does not have correct Start, continuing");
                        continue;
                    }
                    Logger.LogInfo("Method: " + method.FullName + " does not have correct Parameters, continuing");
                    continue;
                }
                Logger.LogInfo("Method: " + method.FullName + " does not have Start, continuing");
                continue;
            }
            Logger.LogInfo("fat faggot retard");
            return null;
        }


        [PatchPrefix]
        private static bool PatchPrefix(Player __instance)
        {
            Logger.LogInfo("yes3");
            return true;
        }
    }
}
