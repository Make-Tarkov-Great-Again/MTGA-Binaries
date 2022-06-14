using Newtonsoft.Json;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.SP.Raid
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
                EFT.Profile profile = PatchConstants.GetAllMethodsForObject(data).First(x => x.Name == "ChooseProfile").Invoke(data, new object[] { ___list_0, true }) as EFT.Profile;
                if (profile != null)
                {
                    EFT.Profile profile2 = profile;
                    if (profile2.Side == EFT.EPlayerSide.Usec)
                    {
                        profile2.Info.Voice = "Usec_1";
                    }
                    else if (profile2.Side == EFT.EPlayerSide.Bear)
                    {
                        profile2.Info.Voice = "Bear_1";
                    }
                    profile2.AccountId = Guid.NewGuid().ToString();
                    profile2.Id = Guid.NewGuid().ToString();

                    Dictionary<string, string> args = new Dictionary<string, string>();
                    args.Add("Side", profile.Side.ToString());
                    args.Add("Settings", profile.Info.Settings.SITToJson());
                    var resultJson = new Request().PostJson("/client/raid/bots/getNewProfile", args.SITToJson());
                    Logger.LogInfo("LoadBotProfileFromServerPatch.PatchPrefix: " + resultJson);
                    Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(resultJson);
                    __result = profile2;
                }
            }
            __result = null;
            //var profilesInCache = PatchConstants.GetFieldOrPropertyFromInstance<List<EFT.Profile>>(__instance, "list_0");
            //Logger.LogInfo("LoadBotProfileFromServerPatch.PatchPrefix: " + profilesInCache.SITToJson());

            return false;
        }
    }
}
