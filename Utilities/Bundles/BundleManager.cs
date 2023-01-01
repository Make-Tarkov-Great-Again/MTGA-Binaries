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
                    try
                    {
                        var data = MTGA_Request.Instance.GetData(path, true);
                        if (data != null && data.Length == 0)
                        {
                            PatchConstants.Logger.LogInfo("Bundle received is 0 bytes. WTF!");
                            continue;
                        }
                        VFS.WriteFile(filepath, data);
                        //PatchConstants.Logger.LogInfo($"Adding Custom Bundle : {filepath}");
                        bundle.Path = filepath;
                    }
                    catch
                    {

                    }
                }

                //PatchConstants.Logger.LogInfo($"Adding Custom Bundle : {key} : {path} : dp={dependencyKeys.Length}");
                Bundles.Add(key, bundle);
            }

            VFS.WriteTextFile(CachePath + "bundles.json", Json.Serialize(Bundles));
        }
    }
}
