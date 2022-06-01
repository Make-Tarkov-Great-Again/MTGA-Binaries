using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/***
 * Full Credit for this patch goes to SPT-Aki team
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules
 * Paulov. Made changes to have better reflection and less hardcoding
 */
namespace SIT.Tarkov.Core
{
    public class BundleManager
    {
        public const string CachePath = "user/cache/bundles/";
        public static Dictionary<string, BundleInfo> Bundles { get; private set; }

        static BundleManager()
        {
            Bundles = new Dictionary<string, BundleInfo>();

            if (VFS.Exists(CachePath))
            {
                VFS.DeleteDirectory(CachePath);
            }
        }

        public static void GetBundles()
        {
            var json = new Request().GetJson("/singleplayer/bundles");
            var jArray = JArray.Parse(json);

            foreach (var jObj in jArray)
            {
                var key = jObj["key"].ToString();
                if (Bundles.ContainsKey(key))
                    continue;

                var path = jObj["path"].ToString();
                var dependencyKeys = jObj["dependencyKeys"].ToObject<string[]>();
                var bundle = new BundleInfo(key, path, dependencyKeys);

                if (path.Contains("http"))
                {
                    var filepath = CachePath + Regex.Split(path, "bundle/", RegexOptions.IgnoreCase)[1];
                    var data = new Request().GetData(path, true);
                    if (data.Length == 0)
                    {
                        PatchConstants.Logger.LogInfo("Bundle received is 0 bytes. WTF!");
                        continue;
                    }
                    VFS.WriteFile(filepath, data);
                    bundle.Path = filepath;
                }

                //PatchConstants.Logger.LogInfo($"Adding Custom Bundle : {key} : {path} : dp={dependencyKeys.Length}");
                Bundles.Add(key, bundle);
            }

            VFS.WriteTextFile(CachePath + "bundles.json", Json.Serialize<Dictionary<string, BundleInfo>>(Bundles));
        }
    }
}
