using EFT;
using MTGA.Core;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;

namespace MTGA.SP
{
    class LoadBotDifficultyFromServer : ModulePatch
    {
        public LoadBotDifficultyFromServer()
        {
        }

        protected override MethodBase GetTargetMethod()
        {
            var getBotDifficultyHandler = typeof(EFT.TarkovApplication).Assembly.GetTypes().Where(type => type.Name.StartsWith("GClass") && type.GetMethod("CheckOnExcude", BindingFlags.Public | BindingFlags.Static) != null).First();
            if (getBotDifficultyHandler == null)
                return null;
            return getBotDifficultyHandler.GetMethod("LoadDifficultyStringInternal", BindingFlags.Public | BindingFlags.Static);
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref string __result, BotDifficulty botDifficulty, WildSpawnType role)
        {
            __result = Request(role, botDifficulty);
            return string.IsNullOrWhiteSpace(__result);
        }

        private static Dictionary<string, string> JsonToUrlCache = new Dictionary<string, string>();

        private static string Request(WildSpawnType role, BotDifficulty botDifficulty)
        {
            var url = "/singleplayer/settings/bot/difficulty/" + role.ToString() + "/" + botDifficulty.ToString();
            if(JsonToUrlCache.ContainsKey(url))
                return JsonToUrlCache[url];

            var json = new MTGA.Core.Request(PatchConstants.GetPHPSESSID(), PatchConstants.GetBackendUrl()).GetJson(url);

            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogError("Received bot " + role.ToString() + " " + botDifficulty.ToString() + " difficulty data is NULL, using fallback");
                return null;
            }

            JsonToUrlCache.Add(url, json);
            //Debug.LogError("[JET]: Successfully received bot " + role.ToString() + " " + botDifficulty.ToString() + " difficulty data");
            return json;
        }
    }
}
