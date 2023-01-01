using Comfort.Common;
using EFT;
using EFT.Airdrop;
using MTGA.Utilities.Airdrops;
using MTGA.Utilities.Core;
using System.Linq;
using System.Reflection;

namespace MTGA.Patches.Raid.Airdrops
{
    /// <summary>
    /// This Patch works the same as AirDropPatch but this one gets initiated by Flare ingame
    /// </summary>
    public class AirdropFlarePatch : ModulePatch
    {
        private static readonly string[] _usableFlares = { "624c09cfbc2e27219346d955", "62389ba9a63f32501b1b4451" };

        protected override MethodBase GetTargetMethod() => typeof(FlareCartridge).GetMethod(nameof(FlareCartridge.Init),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);   //typeof(FlareCartridge).GetMethods().FirstOrDefault(IsTargetMethod);

        /*        private static bool IsTargetMethod(MethodInfo mi)
                {
                    var parameters = mi.GetParameters();
                    return (parameters.Length == 4
                            && parameters[0].Name == "flareCartridgeSettings"
                            && parameters[1].Name == "player"
                            && parameters[2].Name == "flareCartridge"
                            && parameters[3].Name == "weapon");
                }*/

        [PatchPostfix]
        private static void PatchPostfix(BulletClass flareCartridge)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var points = LocationScene.GetAll<AirdropPoint>().Any();

            if (gameWorld != null && points && _usableFlares.Any(x => x == flareCartridge.Template._id))
            {
                gameWorld.gameObject.AddComponent<AirdropsManager>().isFlareDrop = true;
            }
        }
    }
}
