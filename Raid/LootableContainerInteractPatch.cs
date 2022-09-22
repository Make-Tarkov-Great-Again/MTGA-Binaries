using EFT.Interactive;
using Newtonsoft.Json;
using MTGA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MTGA.Core.SP
{
    internal class LootableContainerInteractPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.GetAllMethodsForType(PatchConstants.EftTypes.First(x => x.Name == "LootableContainer")).First(x => x.Name == "Interact");
        }

        [PatchPostfix]
        public static void PatchPostfix(LootableContainer __instance, object interactionResult)
        {
            //Logger.LogInfo($"LootableContainerInteractPatch:PatchPostfix");
            //Logger.LogInfo($"{__instance.Id}");
            //Logger.LogInfo($"{__instance.ItemOwner.ID}");
            //Logger.LogInfo($"{__instance.ItemOwner.Name}");
            //Logger.LogInfo($"{__instance.ItemOwner.ContainerName}");

            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("Id", __instance.Id);
            args.Add("ItemOwner.Id", __instance.ItemOwner.ID);
            args.Add("ItemOwner.Name", __instance.ItemOwner.Name);
            args.Add("ItemOwner.ContainerName", __instance.ItemOwner.ContainerName);
            //var s = args.MTGAToJsonAsync().GetAwaiter().GetResult();
            _ = new MTGA.Core.Request().PostJsonAsync("/client/raid/person/lootingContainer", JsonConvert.SerializeObject(args));
        }
    }
}
