using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT;
using MTGA.Utilities.Core;
using UnityEngine;

namespace MTGA.Patches.AI.Mods
{
    /// <summary>
    /// This patch adds additional layers to check in bot vision so they cannot see you through trees/deep foliage etc.
    /// Initially made by: Props (https://hub.sp-tarkov.com/files/file/903-no-bush-esp/#overview)
    /// </summary>
    internal class NoBushESP : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(BotGroupClass).GetMethod("CalcGoalForBot");


        [PatchPostfix]
        public static void PatchPostfix(BotOwner bot)
        {
            object value = bot.Memory.GetType().GetProperty("GoalEnemy").GetValue(bot.Memory);
            bool flag = value != null;
            if (flag)
            {
                IAIDetails iaidetails = (IAIDetails)value.GetType().GetProperty("Person").GetValue(value);
                bool isYourPlayer = iaidetails.GetPlayer.IsYourPlayer;
                if (isYourPlayer)
                {
                    bool flag2 = scavList.Contains(bot.Profile.Info.Settings.Role) && Plugin.ScavsStillSee;
                    if (!flag2)
                    {
                        bool flag3 = bossesList.Contains(bot.Profile.Info.Settings.Role) && Plugin.BossesStillSee;
                        if (!flag3)
                        {
                            bool flag4 = followersList.Contains(bot.Profile.Info.Settings.Role) && Plugin.FollowersStillSee;
                            if (!flag4)
                            {
                                bool flag5 = pmcList.Contains(bot.Profile.Info.Settings.Role) && Plugin.PMCsStillSee;
                                if (!flag5)
                                {
                                    LayerMask layerMask = 67110913;
                                    float num = Vector3.Distance(bot.Position, iaidetails.GetPlayer.Position);
                                    bool flag6 = Physics.SphereCast(bot.Position, Plugin.TestRayRadius, iaidetails.GetPlayer.Position, out RaycastHit raycastHit, num, layerMask);
                                    if (flag6)
                                    {
                                        List<string> list = exclusionList;
                                        Transform parent = raycastHit.collider.transform.parent;
                                        string text;
                                        if (parent == null)
                                        {
                                            text = null;
                                        }
                                        else
                                        {
                                            GameObject gameObject = parent.gameObject;
                                            text = gameObject != null ? gameObject.name.ToLower() : null;
                                        }
                                        bool flag7 = list.Contains(text);
                                        if (flag7)
                                        {
                                            bool value2 = Plugin.BlockingTypeGoalEnemy;
                                            if (value2)
                                            {
                                                bot.Memory.GetType().GetProperty("GoalEnemy").SetValue(bot.Memory, null);
                                            }
                                            else
                                            {
                                                value.GetType().GetProperty("IsVisible").SetValue(value, false);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Token: 0x06000004 RID: 4 RVA: 0x00002314 File Offset: 0x00000514
        // Note: this type is marked as 'beforefieldinit'.

        public static WildSpawnType[] bossesList = { WildSpawnType.bossBully, WildSpawnType.bossKilla, WildSpawnType.bossKojaniy, WildSpawnType.bossGluhar, WildSpawnType.bossSanitar, WildSpawnType.bossTagilla,
                WildSpawnType.bossKnight, WildSpawnType.sectantWarrior, WildSpawnType.sectantPriest };

        public static WildSpawnType[] followersList = { WildSpawnType.followerBully, WildSpawnType.followerKojaniy, WildSpawnType.followerGluharAssault, WildSpawnType.followerGluharSecurity, WildSpawnType.followerGluharScout,
                WildSpawnType.followerGluharSnipe, WildSpawnType.followerSanitar, WildSpawnType.followerTagilla, WildSpawnType.followerBigPipe, WildSpawnType.followerBirdEye };

        public static WildSpawnType[] pmcList = { WildSpawnType.pmcBot, WildSpawnType.exUsec, WildSpawnType.gifter};

        public static WildSpawnType[] scavList = { WildSpawnType.assault, WildSpawnType.marksman, WildSpawnType.cursedAssault, WildSpawnType.assaultGroup };

        public static List<string> exclusionList = new List<string> { "filbert", "fibert", "tree", "pine", "plant" };
    }
}

