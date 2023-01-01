using EFT.Interactive;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MTGA.Utilities.Core;
// this naming has conflitc with windows System.Request thats why its used like that ~Mao
using MTGA_Request = MTGA.Utilities.Core.Request;

namespace MTGA.Patches.Raid.Menus
{
    internal class LootableContainerInteractPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => PatchConstants
            .GetAllMethodsForType(PatchConstants.EftTypes.First(x => x.Name == "LootableContainer")).First(x => x.Name == "Interact");

        [PatchPostfix]
        public static void PatchPostfix(LootableContainer __instance, object interactionResult)
        {
            //Logger.LogInfo($"LootableContainerInteractPatch:PatchPostfix");
            //Logger.LogInfo($"{__instance.Id}");
            //Logger.LogInfo($"{__instance.ItemOwner.ID}");
            //Logger.LogInfo($"{__instance.ItemOwner.Name}");
            //Logger.LogInfo($"{__instance.ItemOwner.ContainerName}");

            Dictionary<string, string> args = new()
            {
                { "Id", __instance.Id },
                { "ItemOwner.Id", __instance.ItemOwner.ID },
                { "ItemOwner.Name", __instance.ItemOwner.Name },
                { "ItemOwner.ContainerName", __instance.ItemOwner.ContainerName }
            };
            //var s = args.MTGAToJsonAsync().GetAwaiter().GetResult();
            _ = new MTGA_Request().PostJsonAsync("/client/raid/person/lootingContainer", JsonConvert.SerializeObject(args));
        }
    }
}
