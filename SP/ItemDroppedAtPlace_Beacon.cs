using SIT.Tarkov.Core;
using System.Linq;
using System.Reflection;

namespace SIT.Tarkov.SP
{
    /// <summary>
    /// Seems like this patch is fixed already. So its useless but i will leave it for future refference
    /// </summary>
    public class ItemDroppedAtPlace_Beacon : ModulePatch
    {
        public ItemDroppedAtPlace_Beacon()  { }

        protected override MethodBase GetTargetMethod() => typeof(EFT.Player)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Single(IsTargetMethod);

        private bool IsTargetMethod(MethodInfo method)
        {
            if (!method.IsVirtual)
            {
                return false;
            }

            var parameters = method.GetParameters();

            if (parameters.Length != 2
            || parameters[0].ParameterType != typeof(EFT.InventoryLogic.Item)
            || parameters[0].Name != "item"
            || parameters[1].ParameterType != typeof(string)
            || parameters[1].Name != "zone")
            {
                return false;
            }

            return true;
        }

        [PatchPrefix]
        public static bool PatchPrefix(EFT.Player __instance, EFT.InventoryLogic.Item item, string zone)
        {
            __instance.Profile.ItemDroppedAtPlace(item.TemplateId, zone);

            return false;
        }
    }
    // Most likely its this method that is edited
    /*
protected void PlantItem(string itemId, string zoneId, bool successful)
{
    if (successful)
    {
	    this.Profile.ItemDroppedAtPlace(itemId, zoneId);
    }
}

// Token: 0x06005529 RID: 21801 RVA: 0x000D55A0 File Offset: 0x000D37A0
internal virtual void vmethod_3(Item item, string zone)
{
    this.PlantItem(item.TemplateId, zone, true);
}
     */
}
