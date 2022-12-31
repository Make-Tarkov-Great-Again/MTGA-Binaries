using Comfort.Common;
using EFT;
using EFT.Airdrop;
using MTGA.Core;
using MTGA.Utilities.Airdrops;
using System.Linq;
using System.Reflection;
/***
 * Full Credit for this patch goes to SPT-AKI team. Specifically CWX!
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules. 
*/
namespace MTGA.Patches.Raid.Airdrops
{
    public class AirdropPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod("OnGameStarted", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        public static void PatchPostFix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var points = LocationScene.GetAll<AirdropPoint>().Any();

            if (gameWorld != null && points)
            {
                gameWorld.gameObject.AddComponent<AirdropsManager>();
            }
        }
    }
}
