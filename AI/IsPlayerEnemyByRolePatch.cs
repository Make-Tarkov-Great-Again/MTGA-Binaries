using EFT;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.AI
{
    internal class IsPlayerEnemyByRolePatch : ModulePatch
    {
        /*
         * public bool IsPlayerEnemyByRole(WildSpawnType role)
         */
        protected override MethodBase GetTargetMethod()
        {
            return
                PatchConstants.GetMethodForType(
                PatchConstants.EftTypes.Single(x =>
                PatchConstants.GetMethodForType(x, "IsPlayerEnemyByRole") != null), "IsPlayerEnemyByRole");
        }


        [PatchPostfix]
        public static void PatchPostfix(ref bool __result
            , WildSpawnType role
            , WildSpawnType ___wildSpawnType_0
            , WildSpawnType ___wildSpawnType_1
            )
        {
            //__result = ___wildSpawnType_0 != role 
            //    || ___wildSpawnType_1 != role 
            //    || role == WildSpawnType.pmcBot 
            //    || ___wildSpawnType_0 == WildSpawnType.pmcBot
            //    || ___wildSpawnType_1 == WildSpawnType.pmcBot;

            //__result = IsEnemyRole(role, ___wildSpawnType_0) || IsEnemyRole(role, ___wildSpawnType_1);
            __result = IsEnemyRole(role, ___wildSpawnType_0);// || IsEnemyRole(role, ___wildSpawnType_1);
        }

        public static bool IsEnemyRole(WildSpawnType myRole, WildSpawnType enemyRole)
        {
            return myRole == WildSpawnType.pmcBot
                || enemyRole == WildSpawnType.pmcBot
                || (enemyRole != myRole && (enemyRole != WildSpawnType.marksman && myRole != WildSpawnType.assault));
        }
    }
}
