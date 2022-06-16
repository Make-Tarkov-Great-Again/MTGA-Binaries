using EFT;
using Newtonsoft.Json;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.PlayerPatches.Health
{
    public class OnDeadPatch : ModulePatch
    {
        public OnDeadPatch()
        {
            
        }

        protected override MethodBase GetTargetMethod() => typeof(Player)
            .GetMethod("OnDead", BindingFlags.NonPublic | BindingFlags.Instance);

        [PatchPostfix]
        public static void PatchPostfix(EFT.Player __instance, EDamageType damageType)
        {
            Player deadPlayer = __instance;

            var killedBy = PatchConstants.GetFieldOrPropertyFromInstance<Player>(deadPlayer, "LastAggressor", false);
            // Untested MF.
            var killedByLastAggressor = PatchConstants.GetFieldOrPropertyFromInstance<Player>(killedBy, "LastAggressor", false);

            if (killedBy != null)
                PatchConstants.DisplayMessageNotification($"{killedBy.Profile.Info.Nickname} killed {deadPlayer.Profile.Nickname}");
            else
                PatchConstants.DisplayMessageNotification($"{deadPlayer.Profile.Nickname} has died by {damageType}");

            Dictionary<string, object> map = new Dictionary<string, object>();
            map.Add("diedAID", __instance.Profile.AccountId);
            if (__instance.Profile.Info != null)
            {
                map.Add("diedFaction", __instance.Side);
                if(__instance.Profile.Info.Settings != null)
                    map.Add("diedWST", __instance.Profile.Info.Settings.Role);
            }
            if (killedBy != null) 
            {
                map.Add("killedByAID", killedBy.Profile.AccountId);
                map.Add("killerFaction", killedBy.Side);
            }
            if(killedByLastAggressor != null)
            {
                map.Add("killedByLastAggressorAID", killedByLastAggressor.Profile.AccountId);
            }
            new Request().PostJson("/client/raid/person/killed", JsonConvert.SerializeObject(map));
        }
    }
}
