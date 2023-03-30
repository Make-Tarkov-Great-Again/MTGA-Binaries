using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using MTGA.Utilities.Core;

namespace MTGA.Patches.Raid.Fixes
{
    public class MidRaidQuestChangePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Profile).GetMethod("AddToCarriedQuestItems", BindingFlags.Public | BindingFlags.Instance);
        }
        [PatchPostfix]
        public static void PatchPostfix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld != null)
            {
                var player = gameWorld.MainPlayer;

                var questController = Traverse.Create(player).Field<QuestControllerClass>("_questController").Value;
                if (questController != null)
                {
                    foreach (var quest in questController.Quests.ToList())
                    {
                        quest.CheckForStatusChange(true, true);
                    }
                }
            }

        }
    }
}
