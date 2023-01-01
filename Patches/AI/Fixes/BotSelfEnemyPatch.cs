﻿using EFT;
using MTGA.Utilities.Core;
using System.Reflection;


namespace MTGA.Patches.AI.Fixes
{
    /***
 * Full Credit for this patch goes to SPT-AKI team. Specifically CWX & SamSwat!
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules. 
*/
    /// <summary>
    /// Goal: patch removes the current bot from its own enemy list - occurs when adding bots type to its enemy array in difficulty settings
    /// </summary>
    internal class BotSelfEnemyPatch : ModulePatch
    {
        private static readonly string methodName = "PreActivate";

        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner).GetMethod(methodName);
        }

        [PatchPrefix]
        private static bool PatchPrefix(BotOwner __instance, BotGroupClass group)
        {
            IAIDetails selfToRemove = null;

            foreach (var enemy in group.Enemies)
            {
                if (enemy.Key.Id == __instance.Id)
                {
                    selfToRemove = enemy.Key;
                    break;
                }
            }

            if (selfToRemove != null)
            {
                group.Enemies.Remove(selfToRemove);
            }

            return true;
        }
    }
}
