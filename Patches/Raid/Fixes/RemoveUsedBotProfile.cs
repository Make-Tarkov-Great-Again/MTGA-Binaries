using EFT;
using MTGA.Utilities.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MTGA.Patches.Raid.Fixes
{
    class RemoveUsedBotProfile : ModulePatch
    {
        private static BindingFlags _flags;
        private static Type _targetInterface;
        private static Type _targetType;
        private static FieldInfo _profilesField;

        public RemoveUsedBotProfile()
        {
            _ = nameof(IProfileData.ChooseProfile);

            _flags = BindingFlags.Instance | BindingFlags.NonPublic;
            _targetInterface = PatchConstants.EftTypes.Single(IsTargetInterface);
            _targetType = PatchConstants.EftTypes.Single(IsTargetType);
            _profilesField = _targetType.GetField("list_0", _flags);
        }

        private static bool IsTargetInterface(Type type)
        {
            return type.IsInterface && type.GetProperty("StartProfilesLoaded") != null && type.GetMethod("CreateProfile") != null;
        }

        private bool IsTargetType(Type type)
        {
            return _targetInterface.IsAssignableFrom(type) && _targetInterface.IsAssignableFrom(type.BaseType);
        }

        protected override MethodBase GetTargetMethod()
        {
            return _targetType.GetMethod("GetNewProfile", _flags);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref Profile __result, object __instance, IProfileData data)
        {
            var profiles = (List<Profile>)_profilesField.GetValue(__instance);

            if (profiles.Count > 0)
            {
                // second parameter makes client remove used profiles
                __result = data.ChooseProfile(profiles, true);
            }
            else
            {
                __result = null;
            }

            return false;

        }
    }
}
