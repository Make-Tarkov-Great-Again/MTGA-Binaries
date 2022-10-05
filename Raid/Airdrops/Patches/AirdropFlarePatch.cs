using Comfort.Common;
using EFT;
using EFT.Airdrop;
using MTGA.Core;
using System.Linq;
using System.Reflection;

/***
 * Full Credit for this patch goes to SPT-AKI team. Specifically CWX!
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules. 
*/

namespace Aki.Custom.Airdrops.Patches
{
    public class AirdropFlarePatch : ModulePatch
    {
        private const string RedFlare = "624c09cfbc2e27219346d955";

        protected override MethodBase GetTargetMethod()
        {
            return typeof(FlareCartridge).GetMethods().FirstOrDefault(IsTargetMethod);
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return (parameters.Length == 4
                    && parameters[0].Name == "flareCartridgeSettings"
                    && parameters[1].Name == "player"
                    && parameters[2].Name == "flareCartridge"
                    && parameters[3].Name == "weapon");
        }
        [PatchPostfix]
        private static void PatchPostfix(ref BulletClass flareCartridge)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var points = LocationScene.GetAll<AirdropPoint>().Any();

            if (gameWorld != null && points && flareCartridge.Template._id == RedFlare)
            {
                gameWorld.gameObject.AddComponent<AirdropsManager>().isFlareDrop = true;
            }
        }
    }
}
