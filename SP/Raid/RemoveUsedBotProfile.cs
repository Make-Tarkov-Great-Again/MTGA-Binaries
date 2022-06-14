using EFT;
using HarmonyLib;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

////using BotData = GInterface15; // find ChooseProfile and get ginterface off that

namespace SIT.Tarkov.SP.Raid
{
    class RemoveUsedBotProfile : ModulePatch
    {
        private static Type targetInterface;
        private static Type targetType;
        private static AccessTools.FieldRef<object, List<Profile>> profilesField;

        private static Type BotDataInterfaceType;
        private static MethodInfo BotDataChooseProfileMethod;

        public RemoveUsedBotProfile()
        {
            BotDataInterfaceType = PatchConstants.EftTypes.Single(x => x.IsInterface && PatchConstants.GetAllMethodsForType(x).Any(y => y.Name == "ChooseProfile"));
            BotDataChooseProfileMethod = PatchConstants.GetAllMethodsForType(BotDataInterfaceType).Single(x => x.Name == "ChooseProfile");
            // compile-time check
            //_ = nameof(BotData.ChooseProfile);

            targetInterface = PatchConstants.EftTypes.Single(IsTargetInterface);
            targetType = PatchConstants.EftTypes.Single(IsTargetType);
            profilesField = AccessTools.FieldRefAccess<List<Profile>>(targetType, "list_0");
        }

        private static bool IsTargetInterface(Type type)
        {
            if (!type.IsInterface || type.GetProperty("StartProfilesLoaded") == null || type.GetMethod("CreateProfile") == null)
            {
                return false;
            }

            return true;
        }

        private bool IsTargetType(Type type)
        {
            if (!targetInterface.IsAssignableFrom(type) || !targetInterface.IsAssignableFrom(type.BaseType))
            {
                return false;
            }

            return true;
        }

        protected override MethodBase GetTargetMethod()
        {
            return targetType.GetMethod("GetNewProfile", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref Profile __result, object __instance, object data)
        {
            var botDataType = BotDataInterfaceType;
            if (data.GetType() == botDataType)
            {
                var profiles = profilesField(__instance);

                if (profiles.Count > 0)
                {
                    if (BotDataChooseProfileMethod != null)
                        __result = (Profile)BotDataChooseProfileMethod.Invoke(data, new object[] { profiles, true });
                    else
                        __result = null;
                }
                else
                {
                    __result = null;
                }
                return false;
            }
            return true;

        }
    }
}
