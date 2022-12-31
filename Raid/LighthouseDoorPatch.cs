using System.Reflection;
using Comfort.Common;
using EFT;

//Credit to kiobu && debbie_downer
namespace MTGA.Core.Raid
{
    internal class LighthouseDoorPatch : ModulePatch
    {
        private static string LocalPlayer() => GamePlayerOwner.MyPlayer.ProfileId;
        protected override MethodBase GetTargetMethod() => typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted), BindingFlags.Public | BindingFlags.Instance);

        [PatchPrefix]
        private static bool PatchPrefix()
        {
            var doorAccess = Singleton<GameWorld>.Instance?.BufferZoneController;
            //no point in proceeding further if BufferZoneController doesn't exist
            if (doorAccess == null)
            {
                return false;
            }

            doorAccess.SetPlayerAccessStatus(LocalPlayer(), true);

            return true;
        }
    }
}
