using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MTGA.Core.SP.Raid
{
    public class LoadBotProfileFromServerPatch : ModulePatch
    {
        public LoadBotProfileFromServerPatch() { }

        public static BindingFlags BindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        public static Type GetTargetType() => PatchConstants.EftTypes
            .Last(t => t.GetMethods(BindingFlags).Any(x => IsTargetMethod(x)));

        protected override MethodBase GetTargetMethod() => GetTargetType().GetMethods(BindingFlags).Last(x => IsTargetMethod(x));

        public static bool IsTargetMethod(MethodInfo method)
        {
            var parameters = method.GetParameters();

            return (method.Name == "GetNewProfile" && parameters.Length >= 1 && parameters[0].Name == "data");
        }

        [PatchPrefix]
        public static bool PatchPrefix(object __instance, object data, EFT.Profile __result, List<EFT.Profile> ___list_0)
        {
            return ActualPatch(__instance, data, __result, ___list_0);
        }

        public static bool ActualPatch(object __instance, object data, EFT.Profile __result, List<EFT.Profile> ___list_0)
        {
            if (___list_0.Count > 0)
            {
                Logger.LogInfo("LoadBotProfileFromServerPatch:ActualPatch:List count: " + ___list_0.Count);
                var firstOriginalProfile = ___list_0[0];
                //Logger.LogInfo($"LoadBotProfileFromServerPatch:Original Profile 0:{firstOriginalProfile.AccountId}:{firstOriginalProfile.Side}:{firstOriginalProfile.Info.Voice}");

                var profileObj = PatchConstants
                    .GetAllMethodsForObject(data)
                    .First(x => x.Name == "ChooseProfile")
                    .Invoke(data, new object[] { ___list_0, false });
                //.Invoke(data, new object[] { ___list_0, true });
                if (profileObj == null)
                {
                    Logger.LogInfo("LoadBotProfileFromServerPatch:ActualPatch:Profile not found");
                    return false;
                }
                var profile = profileObj as EFT.Profile;
                if (profile != null)
                {
                    // Fix voices
                    if (profile.Side == EFT.EPlayerSide.Usec)
                    {
                        profile.Info.Voice = "Usec_1";
                    }
                    else if (profile.Side == EFT.EPlayerSide.Bear)
                    {
                        profile.Info.Voice = "Bear_1";
                    }
                    //profile.AccountId = Guid.NewGuid().ToString();
                    //profile.Id = Guid.NewGuid().ToString();

                    Logger.LogInfo($"LoadBotProfileFromServerPatch:Profile:{profile.AccountId}:{profile.Side}:{profile.Info.Voice}");

                    //Dictionary<string, string> args = new Dictionary<string, string>();
                    //args.Add("Side", profile.Side.ToString());
                    //args.Add("Settings", profile.Info.Settings.MTGAToJson());
                    //var resultJson = new Request().PostJson("/client/raid/bots/getNewProfile", args.MTGAToJson());
                    //Logger.LogInfo("LoadBotProfileFromServerPatch.PatchPrefix: " + resultJson);
                    //Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(resultJson);
                    __result = profile;
                }
            }
            else
            {
                __result = null;
                return true;
            }
            //var profilesInCache = PatchConstants.GetFieldOrPropertyFromInstance<List<EFT.Profile>>(__instance, "list_0");
            //Logger.LogInfo("LoadBotProfileFromServerPatch.PatchPrefix: " + profilesInCache.MTGAToJson());

            return false;
        }
    }
}
