using System.Linq;
using System.Reflection;
using EFT;
using UnityEngine;

namespace MTGA.Core.AI
{
    // Credit goes to Props
    // https://hub.sp-tarkov.com/files/file/903-no-bush-esp/#overview
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
                                    float num = 1f;
                                    float num2 = 137f;
                                    LayerMask layerMask = 67110913;
                                    bool flag6 = Physics.SphereCast(bot.Position, num, iaidetails.GetPlayer.Position, out RaycastHit raycastHit, num2, layerMask);
                                    if (flag6)
                                    {
                                        Transform parent = raycastHit.collider.transform.parent;
                                        bool flag7;
                                        if (parent == null)
                                        {
                                            flag7 = false;
                                        }
                                        else
                                        {
                                            GameObject gameObject = parent.gameObject;
                                            bool? flag8;
                                            if (gameObject == null)
                                            {
                                                flag8 = null;
                                            }
                                            else
                                            {
                                                string name = gameObject.name;
                                                flag8 = ((name != null) ? new bool?(name.Contains("filbert")) : null);
                                            }
                                            bool? flag9 = flag8;
                                            bool flag10 = true;
                                            flag7 = (flag9.GetValueOrDefault() == flag10) & (flag9 != null);
                                        }
                                        bool flag11;
                                        if (!flag7)
                                        {
                                            Transform parent2 = raycastHit.collider.transform.parent;
                                            if (parent2 == null)
                                            {
                                                flag11 = false;
                                            }
                                            else
                                            {
                                                GameObject gameObject2 = parent2.gameObject;
                                                bool? flag12;
                                                if (gameObject2 == null)
                                                {
                                                    flag12 = null;
                                                }
                                                else
                                                {
                                                    string name2 = gameObject2.name;
                                                    flag12 = ((name2 != null) ? new bool?(name2.Contains("fibert")) : null);
                                                }
                                                bool? flag9 = flag12;
                                                bool flag10 = true;
                                                flag11 = (flag9.GetValueOrDefault() == flag10) & (flag9 != null);
                                            }
                                        }
                                        else
                                        {
                                            flag11 = true;
                                        }
                                        bool flag13 = flag11;
                                        if (flag13)
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

        // Token: 0x06000004 RID: 4 RVA: 0x00002314 File Offset: 0x00000514
        // Note: this type is marked as 'beforefieldinit'.
        static NoBushESP()
        {
            WildSpawnType[] array = new WildSpawnType[9];
            bossesList = array;
            WildSpawnType[] array2 = new WildSpawnType[10];
            followersList = array2;
            WildSpawnType[] array3 = new WildSpawnType[5];
            pmcList = array3;
            WildSpawnType[] array4 = new WildSpawnType[4];
            scavList = array4;
        }

        public static WildSpawnType[] bossesList;

        public static WildSpawnType[] followersList;

        public static WildSpawnType[] pmcList;

        public static WildSpawnType[] scavList;
    }
}

