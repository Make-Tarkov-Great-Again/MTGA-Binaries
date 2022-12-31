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
 * Full Credit for this patch goes to SPT-AKI team. Specifically CWX & SamSwat!
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules. 
*/
namespace Aki.Custom.Airdrops.Utils
{
    public class ItemFactoryUtil
    {
        private readonly ItemFactory itemFactory;
        private static readonly string DropContainer = "6223349b3136504a544d1608";

        public ItemFactoryUtil()
        {
            itemFactory = Singleton<ItemFactory>.Instance;
        }

        public void BuildContainer(LootableContainer container)
        {
            if (itemFactory.ItemTemplates.TryGetValue(DropContainer, out var template))
            {
                Item item = itemFactory.CreateItem(DropContainer, template._id, null);
                LootItem.CreateLootContainer(container, item, "CRATE", Singleton<GameWorld>.Instance);
            }
            else
            {
                Debug.LogError($"[AIRDROPS]: Unable to find template: {DropContainer}");
            }
        }

        public async void AddLoot(LootableContainer container)
        {
            List<AirdropLootModel> loot = GetLoot();

            Item actualItem;

            foreach (var item in loot)
            {
                ResourceKey[] resources;
                if (item.IsPreset)
                {
                    actualItem = itemFactory.GetPresetItem(item.Tpl);
                    resources = actualItem.GetAllItems().Select(x => x.Template).SelectMany(x => x.AllResources).ToArray();
                }
                else
                {
                    actualItem = itemFactory.CreateItem(item.ID, item.Tpl, null);
                    actualItem.StackObjectsCount = item.StackCount;

                    resources = actualItem.Template.AllResources.ToArray();
                }

                container.ItemOwner.MainStorage[0].Add(actualItem);
                await Singleton<PoolManager>.Instance.LoadBundlesAndCreatePools(PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Local, resources, JobPriority.Immediate, null, PoolManager.DefaultCancellationToken);
            }
        }

        private List<AirdropLootModel> GetLoot()
        {
            var json = new MTGA.Core.Request().GetJson("/client/location/getAirdropLoot");
            var loot = JsonConvert.DeserializeObject<List<AirdropLootModel>>(json);

            return loot;
        }
    }
}
