using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT.UI;
using EFT;
using MTGA.Utilities.Core;
using UnityEngine;

namespace MTGA.Patches.Menus
{
    public class RemovePracticeModeWarning : ModulePatch
    {
        [PatchPostfix]
        public static void PatchPostfix(UpdatableToggle ____offlineModeToggle)
        {
            // Do a force of these, just encase it breaks
            ____offlineModeToggle.isOn = true;
            ____offlineModeToggle.gameObject.SetActive(false);
            ____offlineModeToggle.enabled = false;
            ____offlineModeToggle.interactable = false;


            // Hide "no progression save" panel
            var offlineRaidScreenContent = GameObject.Find("Matchmaker Offline Raid Screen").transform.Find("Content").transform;
            var warningPanel = offlineRaidScreenContent.Find("WarningPanelHorLayout");
            warningPanel.gameObject.SetActive(false);
            var spacer = offlineRaidScreenContent.Find("Space (1)");
            spacer.gameObject.SetActive(false);

            // Disable "Enable practice mode for this raid" toggle
            var practiceModeComponent = GameObject.Find("SoloModeCheckmarkBlocker").GetComponent<UiElementBlocker>();
            practiceModeComponent.SetBlock(true, "Raids in SPT are always Offline raids. Don't worry - your progress will be saved!");
        }

        protected override MethodBase GetTargetMethod()
        {
            var type = typeof(EFT.UI.Matchmaker.MatchmakerOfflineRaidScreen);
            var methods = PatchConstants.GetAllMethodsForType(type);

            foreach (var method in methods)
            {
                if (!method.Name.StartsWith("Show")) continue;

                var parameters = method.GetParameters();
                if (parameters.Length == 2
                && parameters[0].Name == "profileInfo"
                && parameters[0].ParameterType == typeof(InfoClass)
                && parameters[1].Name == "raidSettings"
                && parameters[1].ParameterType == typeof(RaidSettings))
                {
                    Logger.LogInfo(method.Name);
                    return method;
                }
            }
            return null;
        }
    }
}
