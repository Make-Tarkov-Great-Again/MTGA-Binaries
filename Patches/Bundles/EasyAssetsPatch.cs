using Diz.Jobs;
using Diz.Resources;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Build.Pipeline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MTGA.Utilities.Bundles;
using MTGA.Utilities.Core;
using DependencyGraph = DependencyGraph<IEasyBundle>;


namespace MTGA.Patches.Bundles
{
    public class EasyAssetsPatch : ModulePatch
    {
        private static readonly FieldInfo _manifestField;
        private static readonly FieldInfo _bundlesField;
        private static readonly PropertyInfo _systemProperty;

        static EasyAssetsPatch()
        {
            var type = typeof(EasyAssets);

            _manifestField = type.GetField(nameof(EasyAssets.Manifest));
            _bundlesField = PatchConstants.GetFieldFromTypeByFieldType(typeof(EasyAssets), typeof(AssetBundle[]));//  type.GetField($"{EasyBundleHelper.Type.Name.ToLowerInvariant()}_0", PatchConstants.PrivateFlags);
            Logger.LogDebug(_bundlesField.Name);
            _systemProperty = type.GetProperty("System");
        }

        public EasyAssetsPatch()
        {
            _ = nameof(IEasyBundle.SameNameAsset);
            _ = nameof(IBundleLock.IsLocked);
            _ = nameof(BundleLock.MaxConcurrentOperations);
            _ = nameof(DependencyGraph.GetDefaultNode);
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(EasyAssets).GetMethods(PatchConstants.PrivateFlags).Single(IsTargetMethod);
        }

        private static bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return (parameters.Length == 6
                    && parameters[0].Name == "bundleLock"
                    && parameters[1].Name == "defaultKey"
                    && parameters[4].Name == "shouldExclude");
        }

        //[PatchPrefix]
        //private static bool PatchPrefix(ref Task __result, EasyAssets __instance, [CanBeNull] IBundleLock bundleLock, string defaultKey, string rootPath,
        //    string platformName, [CanBeNull] Func<string, bool> shouldExclude, [CanBeNull] Func<string, Task> bundleCheck)
        //{
        //    //await Init(__instance, bundleLock, defaultKey, rootPath, platformName, shouldExclude, bundleCheck);
        //    //__result = Init(__instance, bundleLock, defaultKey, rootPath, platformName, shouldExclude, bundleCheck);
        //    //return false;
        //    return true;
        //}

        [PatchPrefix]
        public static bool PatchPrefix(
            ref Task __result
            , EasyAssets __instance
            , [CanBeNull] IBundleLock bundleLock
            , string defaultKey
            , string rootPath
            , string platformName
            , [CanBeNull] Func<string, bool> shouldExclude
            , [CanBeNull] Func<string, Task> bundleCheck

            )
        {
            __result = Init(__instance, bundleLock, defaultKey, rootPath, platformName, shouldExclude, bundleCheck);
            return false;
        }

        [PatchPostfix]
        public static void PatchPostfix()
        {

        }
        //[PatchPostfix]
        //public static async Task PatchPostfix(Task __result, EasyAssets __instance, [CanBeNull] IBundleLock bundleLock, string defaultKey, string rootPath,
        //    string platformName, [CanBeNull] Func<string, bool> shouldExclude, [CanBeNull] Func<string, Task> bundleCheck)
        //{
        //    //await Init(__instance, bundleLock, defaultKey, rootPath, platformName, shouldExclude, bundleCheck);
        //    //__result = Init(__instance, bundleLock, defaultKey, rootPath, platformName, shouldExclude, bundleCheck);
        //    //return true;
        //}

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
        }

        private static async Task<CompatibilityAssetBundleManifest> GetManifestBundle(string filepath)
        {
            var manifestLoading = AssetBundle.LoadFromFileAsync(filepath);
            await manifestLoading.Await();

            var assetBundle = manifestLoading.assetBundle;
            var assetLoading = assetBundle.LoadAllAssetsAsync();
            await assetLoading.Await();

            return (CompatibilityAssetBundleManifest)assetLoading.allAssets[0];
        }

        private static async Task<CompatibilityAssetBundleManifest> GetManifestJson(string filepath)
        {
            var text = string.Empty;

            using (var reader = File.OpenText($"{filepath}.json"))
            {
                text = await reader.ReadToEndAsync();
            }

            var data = JsonConvert.DeserializeObject<Dictionary<string, BundleItem>>(text).ToDictionary(GetPairKey, GetPairValue);
            var manifest = ScriptableObject.CreateInstance<CompatibilityAssetBundleManifest>();
            manifest.SetResults(data);

            return manifest;
        }

        private static async Task Init(EasyAssets instance, [CanBeNull] IBundleLock bundleLock, string defaultKey, string rootPath,
                                      string platformName, [CanBeNull] Func<string, bool> shouldExclude, Func<string, Task> bundleCheck)
        {
            // platform manifest
            var path = $"{rootPath.Replace("file:///", string.Empty).Replace("file://", string.Empty)}/{platformName}/";
            var filepath = path + platformName;
            var manifest = (File.Exists(filepath)) ? await GetManifestBundle(filepath) : await GetManifestJson(filepath);

            // load bundles
            var bundleNames = manifest.GetAllAssetBundles().Union(BundleManager.Bundles.Keys).ToArray();
            var bundles = (IEasyBundle[])Array.CreateInstance(EasyBundleHelper.Type, bundleNames.Length);

            if (bundleLock == null)
            {
                bundleLock = new BundleLock(int.MaxValue);
            }

            for (var i = 0; i < bundleNames.Length; i++)
            {
                bundles[i] = (IEasyBundle)Activator.CreateInstance(EasyBundleHelper.Type, new object[] { bundleNames[i], path, manifest, bundleLock, bundleCheck });
                await JobScheduler.Yield(EJobPriority.Immediate);
            }

            _manifestField.SetValue(instance, manifest);
            _bundlesField.SetValue(instance, bundles);
            _systemProperty.SetValue(instance, new DependencyGraph(bundles, defaultKey, shouldExclude));
        }
    }
}
