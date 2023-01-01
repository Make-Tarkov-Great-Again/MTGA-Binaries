using EFT;
using MTGA.Utilities.Core;
using System.Linq;
using System.Reflection;

namespace MTGA.Patches.AI.Fixes
{
    public class IsEnemyPatch : ModulePatch
    {
        /*
         * public bool IsEnemy(GInterface66 requester)
         */
        protected override MethodBase GetTargetMethod()
        {
            return
                PatchConstants.GetMethodForType(
                PatchConstants.EftTypes.Single(x =>
                PatchConstants.GetMethodForType(x, "IsEnemy") != null), "IsEnemy");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return false;
        }


        [PatchPostfix]
        public static void PatchPostfix(ref bool __result, BotGroupClass __instance, IAIDetails requester)
        {
            var isEnemy = false; // default not an enemy

            // Check existing enemies list
            if (__instance.Enemies.Any(x => x.Value.Player.Id == requester.Id))
            {
                isEnemy = true;
            }
            else
            {
                if (__instance.Side == EPlayerSide.Usec)
                {
                    if (requester.Side == EPlayerSide.Bear || requester.Side == EPlayerSide.Savage ||
                        ShouldAttackUsec(requester))
                    {
                        isEnemy = true;
                        __instance.AddEnemy(requester);
                    }
                }
                else if (__instance.Side == EPlayerSide.Bear)
                {
                    if (requester.Side == EPlayerSide.Usec || requester.Side == EPlayerSide.Savage ||
                        ShouldAttackBear(requester))
                    {
                        isEnemy = true;
                        __instance.AddEnemy(requester);
                    }
                }
                else if (__instance.Side == EPlayerSide.Savage)
                {
                    if (requester.Side != EPlayerSide.Savage)
                    {
                        // everyone else is an enemy to savage (scavs)
                        isEnemy = true;
                        __instance.AddEnemy(requester);
                    }
                }
            }

            __result = isEnemy;
            __result = true;
        }

        /// <summary>
        /// Return True when usec default behavior is attack + bot is usec
        /// </summary>
        /// <param name="requester"></param>
        /// <returns>bool</returns>
        private static bool ShouldAttackUsec(IAIDetails requester)
        {
            var requesterMind = requester?.AIData?.BotOwner?.Settings?.FileSettings?.Mind;

            if (requesterMind == null)
            {
                return false;
            }

            return requester.IsAI && requesterMind.DEFAULT_USEC_BEHAVIOUR == EWarnBehaviour.Attack && requester.Side == EPlayerSide.Usec;
        }

        /// <summary>
        /// Return True when bear default behavior is attack + bot is bear
        /// </summary>
        /// <param name="requester"></param>
        /// <returns></returns>
        private static bool ShouldAttackBear(IAIDetails requester)
        {
            var requesterMind = requester.AIData?.BotOwner?.Settings?.FileSettings?.Mind;

            if (requesterMind == null)
            {
                return false;
            }

            return requester.IsAI && requesterMind.DEFAULT_BEAR_BEHAVIOUR == EWarnBehaviour.Attack && requester.Side == EPlayerSide.Bear;
        }
    }
}
