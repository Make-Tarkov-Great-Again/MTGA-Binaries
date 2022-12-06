using Diz.Jobs;
using Diz.Resources;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Build.Pipeline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MTGA.Core.Bundles;

/***
 * Full Credit for this patch goes to SPT-AKI team
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules
 * Paulov. Made changes to have better reflection and less hardcoding
 */
////using DependencyGraph = DependencyGraph<IEasyBundle>;

namespace MTGA.Core
{
    public class EasyAssetsPatch : ModulePatch
    {
        private static readonly FieldInfo _manifestField;
        private static readonly FieldInfo _bundlesField;
        private static readonly PropertyInfo _systemProperty;

        private static Type easyAssetsType;

        public static Type EasyAssetsType
        {
            get { 
                
                if(easyAssetsType == null)
                {
                    easyAssetsType = PatchConstants.EftTypes.Single(
                        x => x.Name.Contains("EasyAssets")
                        && PatchConstants.GetFieldFromType(x, "Log") != null
                        && PatchConstants.GetFieldFromType(x, "Manifest") != null
                        && PatchConstants.GetPropertyFromType(x, "System") != null
                    );
                }

                return easyAssetsType; 
            }
            set { easyAssetsType = value; }
        }



        static EasyAssetsPatch()
        {
            //var type = typeof(EasyAssets);

            //_manifestField = EasyAssetsType.GetField(nameof(EasyAssets.Manifest));
            _manifestField = EasyAssetsType.GetField("Manifest");
            _bundlesField = EasyAssetsType.GetField($"{EasyBundleHelper.Type.Name.ToLowerInvariant()}_0", PatchConstants.PrivateFlags);
            _systemProperty = EasyAssetsType.GetProperty("System");

            Logger.LogInfo("EasyAssetsPatch");
            Logger.LogInfo(_manifestField.Name);
            Logger.LogInfo(_bundlesField.Name);
            Logger.LogInfo(_systemProperty.Name);
        }

        public EasyAssetsPatch()
{
            //_ = nameof(IEasyBundle.SameNameAsset);
            //_ = nameof(IBundleLock.IsLocked);
            //_ = nameof(BundleLock.MaxConcurrentOperations);
            //_ = nameof(DependencyGraph.GetDefaultNode);
        }

        protected override MethodBase GetTargetMethod()
        {
            //return typeof(EasyAssets).GetMethods(PatchConstants.PrivateFlags).Single(IsTargetMethod);
            return PatchConstants.EftTypes.Single(
                x => x.Name.Contains("EasyAssets")
                && PatchConstants.GetFieldFromType(x, "Log") != null
                && PatchConstants.GetFieldFromType(x, "Manifest") != null
                && PatchConstants.GetPropertyFromType(x, "System") != null
                ).GetMethods(PatchConstants.PrivateFlags).Single(IsTargetMethod);
        }

        /*
         * // Token: 0x0600D5C8 RID: 54728 RVA: 0x00424FF4 File Offset: 0x004231F4
		private async Task method_0([CanBeNull] GInterface284 bundleLock
        , string defaultKey
        , string rootPath
        , string platformName
        , [CanBeNull] Func<string, bool> shouldExclude
        , [CanBeNull] Func<string, Task> bundleCheck)
         */

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return (parameters.Length >= 6
                && parameters[0].Name == "bundleLock"
                && parameters[1].Name == "defaultKey"
                && parameters[2].Name == "rootPath"
                && parameters[3].Name == "platformName"
                && parameters[4].Name == "shouldExclude"
                && parameters[5].Name == "bundleCheck");
        }

        [PatchPrefix]
        //private static bool PatchPrefix(ref Task __result, EasyAssets __instance, [CanBeNull] object bundleLock, string defaultKey, string rootPath,
        //    string platformName, [CanBeNull] Func<string, bool> shouldExclude, [CanBeNull] Func<string, Task> bundleCheck)
        private static bool PatchPrefix(ref Task __result, object __instance, [CanBeNull] object bundleLock, string defaultKey, string rootPath,
            string platformName, [CanBeNull] Func<string, bool> shouldExclude, [CanBeNull] Func<string, Task> bundleCheck)
        {
            try
            {
                __result = Init(__instance, bundleLock, defaultKey, rootPath, platformName, shouldExclude, bundleCheck);
            }
            catch (Exception)
            {
                Logger.LogInfo("Error: Unable to setup EasyAssetsPatch");
                return true;
            }
            return false;
        }

        public static string GetPairKey(KeyValuePair<string, BundleItem> x)
        {
            return x.Key;
        }

        public static BundleDetails GetPairValue(KeyValuePair<string, BundleItem> x)
        {
            return new BundleDetails
            {
                FileName = x.Value.FileName,
                Crc = x.Value.Crc,
                Dependencies = x.Value.Dependencies
            };
            //string filename = PatchConstants.GetAllPropertiesForObject(x.Key).Single(x => x.Name == "FileName").GetValue(x.Value).ToString();
            //uint crc = uint.Parse(PatchConstants.GetAllPropertiesForObject(x.Key).Single(x => x.Name == "Crc").GetValue(x.Value).ToString());
            //string[] dependancies = Json.Deserialize<string[]>(PatchConstants.GetAllPropertiesForObject(x.Key).Single(x => x.Name == "Dependencies").GetValue(x.Value).MTGAToJson());

            //return new BundleDetails
            //{
            //    //FileName = x.Value.FileName,
            //    //Crc = x.Value.Crc,
            //    //Dependencies = x.Value.Dependencies
            //    FileName = filename,
            //    Crc = crc,
            //    Dependencies = dependancies
            //};
        }

        private static async Task<CompatibilityAssetBundleManifest> GetManifestJson(string filepath)
        {
            if (!filepath.EndsWith(".json"))
                filepath += ".json";

            if (!File.Exists(filepath))
            {
                Logger.LogInfo("EasyAssetsPatch::GetManifestJson::Unable to get " + filepath);
                return null;
            }

            var text = string.Empty;
           

            using (var reader = File.OpenText($"{filepath}"))
            {
                text = await reader.ReadToEndAsync();
            }

            Dictionary<string, BundleDetails> data
                = JsonConvert.DeserializeObject<Dictionary<string, BundleItem>>(text).ToDictionary(GetPairKey, GetPairValue);
            var manifest = ScriptableObject.CreateInstance<CompatibilityAssetBundleManifest>();
            manifest.SetResults(data);

            return manifest;
        }

        private static async Task<CompatibilityAssetBundleManifest> GetManifestBundle(string filepath)
        {
            if (!File.Exists(filepath))
            {
                Logger.LogInfo("EasyAssetsPatch::GetManifestJson::Unable to get " + filepath);
                return null;
            }

            var manifestLoading = AssetBundle.LoadFromFileAsync(filepath);
            await manifestLoading.Await();

            var assetBundle = manifestLoading.assetBundle;
            var assetLoading = assetBundle.LoadAllAssetsAsync();
            await assetLoading.Await();

            return (CompatibilityAssetBundleManifest)assetLoading.allAssets[0];
        }

        //private static async Task Init(EasyAssets instance, [CanBeNull] object bundleLock, string defaultKey, string rootPath,
                                      //string platformName, [CanBeNull] Func<string, bool> shouldExclude, Func<string, Task> bundleCheck)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance">EasyAssets instance</param>
        /// <param name="bundleLock"></param>
        /// <param name="defaultKey"></param>
        /// <param name="rootPath"></param>
        /// <param name="platformName"></param>
        /// <param name="shouldExclude"></param>
        /// <param name="bundleCheck"></param>
        /// <returns></returns>
        private static async Task Init(object instance, object bundleLock, string defaultKey, string rootPath,
                                      string platformName, Func<string, bool> shouldExclude, Func<string, Task> bundleCheck)
        {
            Logger.LogInfo("EasyAssetsPatch.Init.Started");

            // platform manifest
            var path = $"{rootPath.Replace("file:///", string.Empty).Replace("file://", string.Empty)}/{platformName}/";
            var filepath = path + platformName;
            var manifest = (File.Exists(filepath)) ? await GetManifestBundle(filepath) : await GetManifestJson(filepath);
            
            // load bundles
            Logger.LogInfo($"EasyAssetsPatch.Init.1.path={path}");
            Logger.LogInfo($"EasyAssetsPatch.Init.1.filepath={filepath}");

            var bundleNames = manifest.GetAllAssetBundles().Union(BundleManager.Bundles.Keys).ToArray();

            Logger.LogInfo("EasyAssetsPatch.Init.2");
            var bundles = (object[])Array.CreateInstance(EasyBundleHelper.Type, bundleNames.Length);

            if (bundleLock == null)
            {
                Logger.LogInfo("EasyAssetsPatch.Init.3");
                bundleLock = Activator.CreateInstance(BundleSetup.BundleLockType, int.MaxValue);// new BundleLock(int.MaxValue);
            }

            Logger.LogInfo($"EasyAssetsPatch.Init.4.Loading {bundleNames.Length} bundles");
            for (uint iBundleName = 0; iBundleName < bundleNames.Length; iBundleName++)
            {
                //Logger.LogInfo("EasyAssetsPatch.Init." + bundleNames[i]);
                //bundles[i] = (IEasyBundle)Activator.CreateInstance(EasyBundleHelper.Type, new object[] { bundleNames[i], path, manifest, bundleLock, bundleCheck });
                //Logger.LogInfo("EasyAssetsPatch.Init.4");
                bundles[iBundleName] = Activator.CreateInstance(EasyBundleHelper.Type, new object[] { bundleNames[iBundleName], path, manifest, bundleLock, bundleCheck });
                //await JobScheduler.Yield(EJobPriority.Immediate);
                JobScheduler.Yield().GetAwaiter();

            }


            _manifestField.SetValue(instance, manifest);
            _bundlesField.SetValue(instance, bundles);
            //_systemProperty.SetValue(instance, new DependencyGraph(bundles, defaultKey, shouldExclude));
            Logger.LogInfo("EasyAssetsPatch.Init.5");
            Type generic = BundleSetup.DependancyGraphType;
            Type[] typeArgs = { BundleSetup.IEasyBundleType };
            Type constructed = generic.MakeGenericType(typeArgs);
            //_systemProperty.SetValue(instance, Activator.CreateInstance(BundleSetup.DependancyGraphType, bundles, defaultKey, shouldExclude));
            _systemProperty.SetValue(instance, Activator.CreateInstance(constructed, bundles, defaultKey, shouldExclude));
            Logger.LogInfo("EasyAssetsPatch.Init.Complete");

        }

        private static void DisplayTypeInfo(Type t)
        {
            Logger.LogInfo(string.Format("\r\n{0}", t));

            Logger.LogInfo(string.Format("\tIs this a generic type definition? {0}",
                t.IsGenericTypeDefinition));

            Logger.LogInfo(string.Format("\tIs it a generic type? {0}",
                t.IsGenericType));

            Type[] typeArguments = t.GetGenericArguments();
            Logger.LogInfo(string.Format("\tList type arguments ({0}):", typeArguments.Length));
            foreach (Type tParam in typeArguments)
            {
                Logger.LogInfo(string.Format("\t\t{0}", tParam));
            }
        }
    }
}
