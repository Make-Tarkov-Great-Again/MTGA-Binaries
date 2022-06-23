using SIT.Tarkov.Core;
using System.Reflection;

namespace SIT.Tarkov.Core.Menus
{
    /// <summary>
    /// This patch disables the Ready button when you select the location (game by default enables that button in that case)
    /// </summary>
    class DisableReadyButtonOnSelectLocation : ModulePatch
    {
        public DisableReadyButtonOnSelectLocation() { }

            [PatchPrefix]
        public static void PatchPostfix(ref EFT.UI.DefaultUIButton ____readyButton)
        {
            ____readyButton.gameObject.SetActive(false);
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(EFT.UI.Matchmaker.MatchMakerSelectionLocationScreen).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}
