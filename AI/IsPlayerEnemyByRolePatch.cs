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

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return false;
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

        static List<WildSpawnType> ScavRoles = new List<WildSpawnType>() { WildSpawnType.assault, WildSpawnType.assaultGroup, WildSpawnType.marksman };
        static List<WildSpawnType> KnightRoles = new List<WildSpawnType>() { WildSpawnType.bossKnight, WildSpawnType.followerBigPipe, WildSpawnType.followerBirdEye };
        //static List<WildSpawnType> BossRoles = new List<WildSpawnType>() { WildSpawnType.bossKnight, WildSpawnType.followerBigPipe, WildSpawnType.followerBirdEye };
        static List<WildSpawnType> PMCRoles = new List<WildSpawnType>() { WildSpawnType.pmcBot };

        public static bool IsEnemyRole(WildSpawnType myRole, WildSpawnType enemyRole)
        {
            // PMCs kill everyone and everyone kills PMCs
            if (PMCRoles.Contains(myRole) || PMCRoles.Contains(enemyRole))
                return true;

            // Scavs don't kill other scavs
            if (ScavRoles.Contains(myRole) && ScavRoles.Contains(enemyRole))
                return false;

            // Knight guys kill everyone else
            if (KnightRoles.Contains(myRole) && !KnightRoles.Contains(enemyRole))
                return true;

            return false;
        }
    }
}
