using EFT.Interactive;
using Newtonsoft.Json;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.A.Tarkov.Core.SP
{
    internal class LootableContainerInteractPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.GetAllMethodsForType(PatchConstants.EftTypes.Single(x => x.Name == "LootableContainer")).Single(x => x.Name == "Interact");
        }

        [PatchPostfix]
        public static void PatchPostfix(LootableContainer __instance, object interactionResult)
        {
            Logger.LogInfo($"LootableContainerInteractPatch:PatchPostfix");
            Logger.LogInfo($"{__instance.Id}");
            Logger.LogInfo($"{__instance.ItemOwner.ID}");
            Logger.LogInfo($"{__instance.ItemOwner.Name}");
            Logger.LogInfo($"{__instance.ItemOwner.ContainerName}");

            Dictionary<string, string> args = new Dictionary<string, string>();
            //var s = args.SITToJsonAsync().GetAwaiter().GetResult();
            _ = new Request().PostJsonAsync("/client/raid/person/lootingContainer", JsonConvert.SerializeObject(args));
        }
    }
}
