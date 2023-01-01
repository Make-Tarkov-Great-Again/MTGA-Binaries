using Comfort.Common;
using EFT;
using EFT.Airdrop;
using MTGA.Utilities.Airdrops;
using MTGA.Utilities.Core;
using System.Linq;
using System.Reflection;
/***
 * Full Credit for this patch goes to SPT-AKI team. Specifically CWX & SamSwat!
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules. 
*/
namespace MTGA.Patches.Raid.Airdrops
{
    /// <summary>
    /// This patch finds all Airdrop points on the map, checks if airdrop point is accessable, then adds AirdropManager to GameWorld gameObject
    /// </summary>
    public class AirdropPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        /*        {
                    return typeof(GameWorld).GetMethod("OnGameStarted", BindingFlags.Public | BindingFlags.Instance);
                }*/

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
