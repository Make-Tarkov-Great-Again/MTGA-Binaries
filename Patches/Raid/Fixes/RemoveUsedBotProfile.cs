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

        public RemoveUsedBotProfile()
        {
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotPresetFactory).GetMethod("GetNewProfile", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref Profile __result, BotPresetFactory __instance, CreationData data, List<EFT.Profile> ___list_0)
        {
            if (___list_0.Count > 0)
            {
                Profile profile = data.ChooseProfile(___list_0, true);
                if (profile != null)
                {
                    Profile profile2 = profile.ToJson().ParseJsonTo<Profile>();
                    profile2.Inventory.Equipment = profile2.Inventory.Equipment.CloneItem(null);
                    profile2.AccountId = Guid.NewGuid().ToString();
                    profile2.Id = Guid.NewGuid().ToString();
                    __result = profile2;
                    return false;
                }
                __result = profile;
                return false;
            }
            return true;
        }
    }
}
