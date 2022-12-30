using Comfort.Common;
using EFT;
using EFT.Airdrop;
using MTGA.Core;
using System.Linq;
using System.Reflection;

/***
 * Full Credit for this patch goes to SPT-AKI team. Specifically CWX & SamSwat!
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules. 
*/

namespace Aki.Custom.Airdrops.Patches
{
    public class AirdropFlarePatch : ModulePatch
    {
        private static readonly string[] _usableFlares = { "624c09cfbc2e27219346d955", "62389ba9a63f32501b1b4451" };

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
            var flare = flareCartridge;
            var gameWorld = Singleton<GameWorld>.Instance;
            var points = LocationScene.GetAll<AirdropPoint>().Any();

            if (gameWorld != null && points && _usableFlares.Any(x => x == flare.Template._id))
            {
                gameWorld.gameObject.AddComponent<AirdropsManager>().isFlareDrop = true;
            }
        }
    }
}
