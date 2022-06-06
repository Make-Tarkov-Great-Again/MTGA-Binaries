using EFT;
using Newtonsoft.Json;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.Health
{
    public class OnDeadPatch : ModulePatch
    {
        public OnDeadPatch()
        {
            
        }

        protected override MethodBase GetTargetMethod() => typeof(Player)
            .GetMethod("OnDead", BindingFlags.NonPublic | BindingFlags.Instance);

        [PatchPostfix]
        public static void PatchPostfix(Player __instance, EDamageType damageType)
        {
            var lastAggressor = PatchConstants.GetFieldOrPropertyFromInstance<Player>(__instance, "LastAggressor", false);

            if (lastAggressor != null)
                PatchConstants.DisplayMessageNotification($"{__instance.Profile.Nickname} has been killed by {lastAggressor.Profile.Info.Nickname}");
            else
                PatchConstants.DisplayMessageNotification($"{__instance.Profile.Nickname} has been killed by {damageType}");

            Dictionary<string, object> map = new Dictionary<string, object>();
            map.Add("diedAID", __instance.Profile.AccountId);
            if (__instance.Profile.Info != null)
            {
                map.Add("diedFaction", __instance.Profile.Info.Side);
                if(__instance.Profile.Info.Settings != null)
                    map.Add("diedWST", __instance.Profile.Info.Settings.Role);
            }
            if (lastAggressor != null) 
            {
                map.Add("killedByAID", lastAggressor.Profile.AccountId);
            }
            new Request().PostJson("/client/raid/person/killed", JsonConvert.SerializeObject(map));
        }
    }
}
