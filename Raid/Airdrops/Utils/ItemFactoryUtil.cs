using Comfort.Common;
using EFT;
using UnityEngine;
using EFT.Interactive;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using System.Collections.Generic;
using Aki.Custom.Airdrops.Models;
using System.Linq;
/***
 * Full Credit for this patch goes to SPT-Aki team. Specifically CWX!
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules. 
*/
namespace Aki.Custom.Airdrops.Utils
{
    public class ItemFactoryUtil : MonoBehaviour
    {
        private ItemFactory itemFactory;
        private static readonly string DropContainer = "6223349b3136504a544d1608";

        public ItemFactoryUtil()
        {
            itemFactory = Singleton<ItemFactory>.Instance;
        }

        public async void BuildContainer(LootableContainer container)
        {
            if (itemFactory.ItemTemplates.TryGetValue(DropContainer, out var template))
            {
                Item item = itemFactory.CreateItem(DropContainer, template._id, null);
                LootItem.CreateLootContainer(container, item, "CRATE", Singleton<GameWorld>.Instance);
            }
            else
            {
                Debug.LogError($"[AKI-AIRDROPS]: unable to find template: {DropContainer}");
            }
        }

        public async void AddLoot(LootableContainer container)
        {
            List<AirdropLootModel> loot = GetLoot();

            Item actualItem;
            List<ResourceKey> resources = new List<ResourceKey>(50);

            foreach (var item in loot)
            {
                if (item.isPreset)
                {
                    actualItem = itemFactory.GetPresetItem(item.tpl);
                    resources.AddRange(actualItem.GetAllItems().Select(x => x.Template).SelectMany(x => x.AllResources));
                }
                else
                {
                    actualItem = itemFactory.CreateItem(item.id, item.tpl, null);
                    actualItem.StackObjectsCount = item.stackCount;

                    resources.AddRange(actualItem.Template.AllResources);
                }

                container.ItemOwner.MainStorage[0].Add(actualItem);
            }

            await Singleton<PoolManager>.Instance.LoadBundlesAndCreatePools(PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Local, resources.ToArray(), JobPriority.General, null, PoolManager.DefaultCancellationToken);
        }

        private List<AirdropLootModel> GetLoot()
        {
            var json = new SIT.Tarkov.Core.Request().GetJson("/client/location/getAirdropLoot");
            var loot = JsonConvert.DeserializeObject<List<AirdropLootModel>>(json);

            return loot;
        }
    }
}