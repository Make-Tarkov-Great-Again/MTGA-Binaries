using EFT;
using MTGA.Utilities.Core;
using MTGA.Utilities.Player.Health;
using System;
using System.Reflection;

namespace MTGA.Patches.Raid.Menus
{
    public class ReplaceInPlayer : ModulePatch
    {
        //private static string _playerAccountId;

        protected override MethodBase GetTargetMethod()
        {
            //return typeof(Player).GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            return PatchConstants.GetMethodForType(typeof(LocalPlayer), "Init");
        }

        [PatchPostfix]
        public static void PatchPostfix(
            object __instance
            , object healthController)
        //public static void PatchPostfix(Player __instance, Task __result)
        {


            //if (_playerAccountId == null)
            //{
            //    var backendSession = ClientAccesor.GetClientApp().GetClientBackEndSession();
            //    var profile = backendSession.Profile;
            //    _playerAccountId = profile.AccountId;
            //}

            //Logger.LogInfo($"ReplaceInPlayer:PatchPostfix: {__instance.GetType()}");
            //Logger.LogInfo($"ReplaceInPlayer:PatchPostfix:healthController {healthController.GetType()}");


            var instanceProfile = __instance.GetType().GetProperty("Profile"
                , BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).GetValue(__instance);
            if (instanceProfile == null)
            {
                Logger.LogInfo("ReplaceInPlayer:PatchPostfix: Couldn't find Profile");
                return;
            }
            //Logger.LogInfo($"ReplaceInPlayer:PatchPostfix: found instanceProfile {instanceProfile.GetType()}");
            var instanceAccountProp = instanceProfile.GetType().GetField("AccountId"
            , BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            //Logger.LogInfo($">>>>>>>>>>>>>>>>>>>>>>>> Properties");
            //foreach (var p in PatchConstants.GetAllPropertiesForObject(instanceProfile))
            //{
            //    Logger.LogInfo(p.Name);
            //}
            //Logger.LogInfo($">>>>>>>>>>>>>>>>>>>>>>>> Fields");
            //foreach (var p in PatchConstants.GetAllFieldsForObject(instanceProfile))
            //{
            //    Logger.LogInfo(p.Name);
            //}
            if (instanceAccountProp == null)
            {
                Logger.LogInfo($"ReplaceInPlayer:PatchPostfix: instanceAccountProp not found");
                return;
            }
            var instanceAccountId = instanceAccountProp.GetValue(instanceProfile).ToString();

            // If is Bot Guid, then ignore it
            if (Guid.TryParse(instanceAccountId, out _))
                return;

            if (string.IsNullOrEmpty(instanceAccountId))
                return;

            //Logger.LogInfo($"ReplaceInPlayer:PatchPostfix: instanceAccountId {instanceAccountId}");

            if (instanceAccountId != PatchConstants.GetPHPSESSID())
            {
                //Logger.LogInfo($"ReplaceInPlayer: {instanceAccountId}!={PatchConstants.GetPHPSESSID()}");

                return;
            }

            //Logger.LogInfo($">>>>>>>>>>>>>>>>>>>>>>>> Properties");
            //foreach (var p in PatchConstants.GetAllPropertiesForObject(__instance))
            //{
            //    Logger.LogInfo(p.Name);
            //}
            //Logger.LogInfo($">>>>>>>>>>>>>>>>>>>>>>>> Fields");
            //foreach (var p in PatchConstants.GetAllFieldsForObject(__instance))
            //{
            //    Logger.LogInfo(p.Name);
            //}

            //await __result;

            //var listener = HealthListener.Instance;
            //listener.Init(__instance.HealthController, true);

            var listener = HealthListener.Instance;
            var insthealthController = PatchConstants.GetFieldOrPropertyFromInstance<object>(__instance, "HealthController", false);
            //var healthController = ____healthController;

            //    if (healthController != null)
            //    {
            //        //Logger.LogInfo("ReplaceInPlayer:PatchPostfix: found health controller " + healthController.GetType());
            //    //listener.Init(healthController, true);
            //}
            //var healthController2 = __instance.GetType().GetProperty("HealthController", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).GetValue(__instance);
            ////Logger.LogInfo("ReplaceInPlayer:PatchPostfix: found health controller 2" + healthController2.GetType());

            //var activeHealthController = __instance.GetType().GetProperty("ActiveHealthController", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).GetValue(__instance);
            //Logger.LogInfo("ReplaceInPlayer:PatchPostfix: found active health controller " + activeHealthController.GetType());
            //if (activeHealthController != null)
            if (healthController != null)
            {
                listener.Init(healthController, true); // I changed this from healthController to activehealthcontroller when i moved from SP to Core
                //listener.Init(activeHealthController, true);
            }

            //listener.Init(__instance.HealthController, true);
        }
    }
}
