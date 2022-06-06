using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT.InventoryLogic;
using SIT.Tarkov.Core;

namespace SIT.A.Tarkov.Core.Spawners
{
    public class ItemFactory
	{
		private static object instance;

		private static MethodBase methodCreateItem;

		private static Dictionary<string, ItemTemplate> dict = new Dictionary<string, ItemTemplate>();

		public static void Init()
		{
			Type type = PatchConstants.EftTypes.Single((Type x) => x.GetMethod("LogErrors") != null);
			Type type2 = typeof(Singleton<>).MakeGenericType(type);
			ItemFactory.instance = type2.GetProperty("Instance").GetValue(type2);
			ItemFactory.methodCreateItem = type.GetMethod("CreateItem");
			ItemFactory.dict = (Dictionary<string, ItemTemplate>)type.GetField("ItemTemplates").GetValue(ItemFactory.instance);
		}

		public static Item CreateItem(string id, string tpid)
		{
			if (null == ItemFactory.methodCreateItem)
			{
				ItemFactory.Init();
			}
			return (Item)ItemFactory.methodCreateItem.Invoke(ItemFactory.instance, new object[3] { id, tpid, null });
		}

		public static ItemTemplate GetItemTemplateById(string id)
		{
			if (null == ItemFactory.methodCreateItem)
			{
				ItemFactory.Init();
			}
			return ItemFactory.dict[id];
		}
	}

}
