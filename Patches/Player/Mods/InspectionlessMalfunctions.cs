using System.Reflection;
using EFT.InventoryLogic;
using MTGA.Utilities.Core;

// Credit to Fontaine
// https://hub.sp-tarkov.com/files/file/661-fontaine-s-inspectionless-malfs/#overview

namespace MTGA.Patches.Player.Mods
{
    internal class InspectionlessMalfunctions : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(Weapon.MalfunctionState).GetMethod("IsKnownMalfType", BindingFlags.Instance | BindingFlags.Public);

        [PatchPostfix]
        public static void PatchPostfix(ref bool __result)
        {
            __result = true;
        }
    }
}
