using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;

//Credit to kiobu

namespace MTGA.Core.Raid
{
    internal class LighthouseDoorPatch : ModulePatch
    {
        private static string LocalPlayer() => GamePlayerOwner.MyPlayer.ProfileId;
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = PatchConstants.EftTypes.Single(x => x.Name == "GameWorld");
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            var desiredMethod = desiredType.GetMethod("OnGameStarted", flags);


            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }
        [PatchPrefix]
        private static bool PatchPrefix()
        {
            // Local player.
            var player = LocalPlayer();

            // For now, player is always allowed to enter Lightkeeper's room.
            var status = true;

            // Set access status for local player.
            var doorAccess = Singleton<GameWorld>.Instance.BufferZoneController;
            doorAccess.SetPlayerAccessStatus(player, status);

            return true;
        }
    }
}
