using EFT;
using SIT.Tarkov.Core;
using SIT.Tarkov.Core.PlayerPatches.Health;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.AI
{
    internal class IsPlayerEnemyPatch : ModulePatch
    {
        /*
         * public bool IsPlayerEnemy(GInterface66 requester)
         */
        protected override MethodBase GetTargetMethod()
        {
            return
                PatchConstants.GetMethodForType(
                PatchConstants.EftTypes.Single(x =>
                PatchConstants.GetMethodForType(x, "IsPlayerEnemy") != null), "IsPlayerEnemy");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return false;
        }

        [PatchPostfix]
        public static void PatchPostfix(ref bool __result
            , IAIDetails player
            , WildSpawnType ___wildSpawnType_0
            , WildSpawnType ___wildSpawnType_1
            )
        {
            if (player != null)
            {
                __result = true;

                if (player.AIData.IsAI)
                {
                    if (player.Profile != null && player.Profile.Info != null && player.Profile.Info.Settings != null)
                    {
                        var otherAIPlayerRole = player.AIData.BotOwner.Profile.Info.Settings.Role;

                        bool isEnemyRole =
                            IsPlayerEnemyByRolePatch.IsEnemyRole(___wildSpawnType_0, otherAIPlayerRole)
                            || IsPlayerEnemyByRolePatch.IsEnemyRole(___wildSpawnType_1, otherAIPlayerRole);
                        //var otherPlayerHealthController = HealthControllerHelpers.GetActiveHealthController(player);
                        //var otherPlayerIsAlive = HealthControllerHelpers.IsAlive(otherPlayerHealthController);
                        //__result = otherPlayerIsAlive && isEnemyRole;
                        __result = isEnemyRole;
                    }
                }
            }
        }
    }
}
