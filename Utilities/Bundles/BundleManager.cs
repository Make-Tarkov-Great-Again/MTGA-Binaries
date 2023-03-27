using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

using MTGA_Request = MTGA.Utilities.Core.Request;
/***
 * Full Credit for this patch goes to SPT-AKI team
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules
 * Paulov. Made changes to have better reflection and less hardcoding
 */
namespace MTGA.Utilities.Bundles
{
    public static class BundleManager
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
            var json = MTGA_Request.Instance.GetJson("/getBundleList");
            //PatchConstants.Logger.LogInfo($"{json}");

            var jArray = JArray.Parse(json);
            //PatchConstants.Logger.LogInfo($"{jArray}");


            foreach (var jObj in jArray)
            {
               // PatchConstants.Logger.LogInfo("in foreach");

                var key = jObj["key"].ToString();
                //PatchConstants.Logger.LogInfo("key");

                if (Bundles.ContainsKey(key))
                {
                    //PatchConstants.Logger.LogInfo("Bundles.ContainsKey(key)");
                    continue;
                }

                var path = jObj["path"].ToString();
                //PatchConstants.Logger.LogInfo("path");

                var dependencyKeys = jObj["dependencyKeys"].ToObject<string[]>();
               // PatchConstants.Logger.LogInfo("dependencyKeys");

                var bundle = new BundleInfo(key, path, dependencyKeys);
                //PatchConstants.Logger.LogInfo("bundle");


                if (path.Contains("http"))
                {
                    //PatchConstants.Logger.LogInfo("path.Contains(\"http\")");

                    var filepath = CachePath + Regex.Split(path, "bundle/", RegexOptions.IgnoreCase)[1];

                    var data = MTGA_Request.Instance.GetData(path, true);
                    if (data != null && data.Length == 0)
                    {
                        PatchConstants.Logger.LogInfo("Bundle received is 0 bytes. WTF!");
                        continue;
                    }
                    VFS.WriteFile(filepath, data);
                    PatchConstants.Logger.LogInfo($"Adding Custom Bundle : {filepath}");
                    bundle.Path = filepath;
                }

                PatchConstants.Logger.LogInfo($"Adding Custom Bundle : {key} : {path} : dp={dependencyKeys.Length}");
                Bundles.Add(key, bundle);
            }

            //PatchConstants.Logger.LogInfo($"BUNDLES: {Bundles}");
            var serialized = Json.Serialize<Dictionary<string, BundleInfo>>(Bundles);
            //PatchConstants.Logger.LogInfo($"serialized: {serialized}");

            VFS.WriteTextFile(CachePath + "bundles.json", serialized);
        }
    }
}
