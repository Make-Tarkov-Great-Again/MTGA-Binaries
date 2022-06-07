using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Diz.Jobs;
using Diz.Resources;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Build.Pipeline;
using SIT.Tarkov.Core;
using SIT.Tarkov.Core.Bundles;

namespace BundleLoader.Patches
{
    /// <summary>
    /// This Patch will ask the server for bundles it requires for game to run properly, will also try to save bundles on your hard drive
    /// </summary>
    public class EasyAssets : ModulePatch
    {
        private const string BundleUrl = "/singleplayer/bundles";
        private static readonly WebClient _client = new WebClient { CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore) };

        //private static FieldInfo _manifestField;
        private static FieldInfo _bundlesField;
        private static PropertyInfo _systemProperty;

        public EasyAssets() {

            var type = typeof(EasyAssets);
            //_ = nameof(IEasyBundle.SameNameAsset);
            //_ = nameof(IBundleLock.IsLocked);
            //_ = nameof(BundleLock.MaxConcurrentOperations);
            //_ = nameof(DependencyGraph.GetDefaultNode);

            //_manifestField = type.GetField(nameof(EasyAssets.Manifest));
            _bundlesField = type.GetField($"{EasyBundleHelper.Type.Name.ToLowerInvariant()}_0", PatchConstants.PrivateFlags);
            _systemProperty = type.GetProperty("System");
        }

        protected override MethodBase GetTargetMethod()
        {
            var targetType = Constants.Instance.TargetAssembly.GetTypes().First(IsTargetType);
            return targetType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.Name == "method_0");
        }

        private static bool IsTargetType(Type type)
        {
            return type.IsClass && type.Name.EndsWith("EasyAssets");
        }

        [PatchPrefix]
        private static bool PatchPrefix(Diz.Resources.EasyAssets __instance, object bundleLock, string defaultKey, string rootPath, string platformName, Func<string, bool> shouldExclude, Func<string, Task> bundleCheck)
        {
            Logger.LogInfo("EasyAssets.Init.1");

            //            CacheServerBundles();
            var path = $"{rootPath.Replace("file:///", string.Empty).Replace("file://", string.Empty)}/{platformName}/";

            var text = rootPath.Replace("file:///", "").Replace("file://", "") + "/" + platformName + "/";
            //Logger.LogInfo(text + platformName + ".json");
            var json = File.ReadAllText(text + platformName + ".json");
            //var results =
            ////JsonConvert.DeserializeObject<Dictionary<string, Shared.BundleDetailStruct>>(File.ReadAllText(text + platformName + ".json"))
            ///
            var results = JsonConvert.DeserializeObject<Dictionary<string, StreamingAssetBundle>>(json)
                .ToDictionary(k => k.Key, v => new BundleDetails
                {
                    FileName = v.Value.FileName,
                    Crc = v.Value.Crc,
                    Dependencies = v.Value.Dependencies != null && v.Value.Dependencies.Length > 0 ? v.Value.Dependencies : new string[0]
                });
            //JsonConvert.DeserializeObject<Dictionary<string, BundleDetails>>(json)
            //    .ToDictionary(k => k.Key, v => new BundleDetails
            //    {
            //        FileName = v.Value.FileName,
            //        Crc = v.Value.Crc,
            //        Dependencies = v.Value.Dependencies
            //    });
            ////foreach (var (key, value) in Shared.CachedBundles)
            Logger.LogInfo("EasyAssets.Init.2");
            foreach (var (key, value) in BundleManager.Bundles)
            {
                uint i = 100;
                var detail = new BundleDetails()
                {
                    FileName = value.Path,
                    Crc = i++,
                    Dependencies = value.DependencyKeys
                };

                results.Add(key, detail);
            }
            __instance.Manifest = ScriptableObject.CreateInstance<CompatibilityAssetBundleManifest>();
            __instance.Manifest.SetResults(results);

            Logger.LogInfo("EasyAssets.Init.3");
            var allAssetBundles = __instance.Manifest.GetAllAssetBundles();
            var bundles = (object[])Array.CreateInstance(LoaderType, allAssetBundles.Length);
            //var bundles = new object[allAssetBundles.Length];
            if (bundleLock == null)
            {
                bundleLock = Activator.CreateInstance(BundleSetup.BundleLockType, int.MaxValue);// new BundleLock(int.MaxValue);
                Logger.LogInfo("EasyAssets.Init.3.Created Bundle Lock");
            }

            Logger.LogInfo($"EasyAssetsPatch.Init.4.Loading {allAssetBundles.Length} bundles");
            for (uint iBundleName = 0; iBundleName < allAssetBundles.Length; iBundleName++)
            {
                //Logger.LogInfo("EasyAssetsPatch.Init." + bundleNames[i]);
                //bundles[i] = (IEasyBundle)Activator.CreateInstance(EasyBundleHelper.Type, new object[] { bundleNames[i], path, manifest, bundleLock, bundleCheck });
                //Logger.LogInfo("EasyAssetsPatch.Init.4");
                //bundles[iBundleName] = Activator.CreateInstance(EasyBundleHelper.Type, new object[] { allAssetBundles[iBundleName], string.Empty, __instance.Manifest, bundleLock, bundleCheck });
                bundles[iBundleName] = Activator.CreateInstance(LoaderType, allAssetBundles[iBundleName], string.Empty, __instance.Manifest, bundleLock, bundleCheck);
                //await JobScheduler.Yield();
                JobScheduler.Yield().GetAwaiter();

            }
            
            //Logger.LogInfo($"EasyAssetsPatch.Init.4.{bundles[0].GetType().Name}");
            //Logger.LogInfo($"EasyAssetsPatch.Init.4.Loading Complete");

            //            bundleLock = Shared.BundleLockConstructor.Invoke(new object[] {int.MaxValue}); // there was ??= but it was throwing errors so it got removed
            //            for (var i = 0; i < allAssetBundles.Length; i++)
            //            {
            //                bundles[i] = Activator.CreateInstance(Shared.LoaderType, allAssetBundles[i], string.Empty, __instance.Manifest, bundleLock, bundleCheck);
            //                JobScheduler.Yield().GetAwaiter();
            //            }

            //            AccessTools.Property(__instance.GetType(), "System").SetValue(__instance, Activator.CreateInstance(Shared.NodeType, bundles, defaultKey, shouldExclude));
            //Logger.LogInfo("EasyAssetsPatch.Init.5");
            //Type generic = BundleSetup.DependancyGraphType;
            ////Logger.LogInfo($"EasyAssetsPatch.Init.5.{generic.Name}");
            //Type[] typeArgs = { LoaderType }; // BundleSetup.IEasyBundleType };
            //Type constructed = generic.MakeGenericType(typeArgs);
            //Logger.LogInfo($"EasyAssetsPatch.Init.5.{constructed.Name}");
            //Logger.LogInfo($"EasyAssetsPatch.Init.5.{NodeType}");
            //_systemProperty.SetValue(__instance, Activator.CreateInstance(BundleSetup.DependancyGraphType, bundles, defaultKey, shouldExclude));
            //var myInstance = Activator.CreateInstance(NodeType, bundles, defaultKey, shouldExclude);
            var myInstance = Activator.CreateInstance(NodeType, bundles, null, null);
            //_systemProperty.SetValue(__instance, Activator.CreateInstance(constructed, bundles, defaultKey, shouldExclude));
            //_systemProperty.SetValue(__instance, Activator.CreateInstance(typeof(GClass2586<GInterface287>), bundles, defaultKey, shouldExclude));
            //Logger.LogInfo("EasyAssetsPatch.Init.Complete");

            return false;
            //return true;
        }


        static Type _loaderType;

        public static Type LoaderType
        {
            get
            {
                if (_loaderType == null)
                    _loaderType = Constants.Instance.TargetAssembly.GetTypes().Single(x => x.IsClass && x.GetProperty("SameNameAsset") != null);
                Debug.LogError("LoaderType: " + _loaderType.FullName);
                return _loaderType;
            }
        }

        static Type _nodeType;

        public static Type NodeType
        {
            get
            {
                if (_nodeType == null)
                {
                    var nodeInterfaceType = PatchConstants.EftTypes
                        .First(x => x.IsInterface && x.GetProperty("SameNameAsset") != null);

                    _nodeType = PatchConstants.EftTypes
                        .Single(x =>
                            x.IsClass && x.GetMethod("GetNode") != null && string.IsNullOrWhiteSpace(x.Namespace))
                        .MakeGenericType(nodeInterfaceType);
                }

                return _nodeType;
            }
        }
        /*
        private static string GetLocalBundlePath(Bundle bundle)
        {
            // lets wrap everything in trycatch cause Unity
            try
            {
                Debug.LogError(bundle.key);
                Debug.LogError(bundle.path);
                var local = false;
                var backend = new Uri(PatchConstants.GetBackendUrl());

                // Check if host is local
                if (IPAddress.TryParse(backend.Host, out var ip))
                    if (ip.MapToIPv4().ToString().StartsWith("127"))
                        local = true;


                if (local && File.Exists(bundle.path))
                    return bundle.path;

                // Check local bundles folder
                var possibleLocalPath = Path.Combine(Shared.LOCAL_BUNDLES_PATH, bundle.key);
                if (File.Exists(possibleLocalPath))
                    return possibleLocalPath;

                // Check local cache
                var cachePath = Path.Combine(Shared.CACHE_BUNDLES_PATH, backend.Host, bundle.key);
                if (File.Exists(cachePath))
                    return cachePath;

                // Download bundle and put it in the cache folder
                var url = PatchConstants.GetBackendUrl() + "/files/bundle/" + bundle.key;
                var dirPath = Path.GetDirectoryName(cachePath);
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                Debug.Log("Downloading bundle from " + url);

                _client.DownloadFile(url, cachePath);

                return cachePath;
            }
            catch (Exception e)
            {
                Debug.Log(e.Data);
                return null;
            }
        }
        */
        //        private static void CacheServerBundles()
        //        {
        //            try
        //            {
        //                var text = new Request(null, ClientAccesor.BackendUrl).GetJson(BundleUrl);
        //                var serverBundles = JsonConvert.DeserializeObject<Bundle[]>(text);
        //                foreach (var bundle in serverBundles)
        //                {
        //                    var localPath = GetLocalBundlePath(bundle);
        //                    AssetBundle customBundle = null;


        //                    if (bundle.dependencyKeys.Length > 0)
        //                        File.WriteAllLines(localPath + ".manifest", bundle.dependencyKeys);

        //                    try
        //                    {
        //                        customBundle = AssetBundle.LoadFromFile(localPath);
        //                        var bundlePath = Path.Combine(AppContext.BaseDirectory,
        //                            "EscapeFromTarkov_Data/StreamingAssets/Windows/", customBundle.name);
        //                        if (!File.Exists(bundlePath))
        //                        {
        //                            Shared.CachedBundles.Add(customBundle.name, localPath);
        //                            Debug.Log("Cached modded bundle " + customBundle.name);
        //                            var manifestPath = localPath + ".manifest";
        //                            if (File.Exists(manifestPath))
        //                            {
        //                                Shared.ManifestCache.Add(customBundle.name, manifestPath);
        //                                Debug.Log("Cached manifest for " + customBundle.name);
        //                            }
        //                        }

        //                        else
        //                        {
        //                            var assets = customBundle.GetAllAssetNames();
        //                            foreach (var assetName in assets)
        //                            {
        //                                if (!Shared.ModdedAssets.ContainsKey(customBundle.name))
        //                                    Shared.ModdedAssets.Add(customBundle.name, new List<string>());
        //                                if (!Shared.ModdedBundlePaths.ContainsKey(customBundle.name))
        //                                    Shared.ModdedBundlePaths.Add(customBundle.name, localPath);
        //                                if (!Shared.ModdedAssets[customBundle.name].Contains(assetName))
        //                                    Shared.ModdedAssets[customBundle.name].Add(assetName);
        //                            }

        //                            Debug.Log("Cached modded assets for " + customBundle.name);
        //                        }
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        Debug.LogError("Failed to Load modded bundle " + localPath + ": " + e);
        //                    }

        //                    try
        //                    {
        //                        customBundle.Unload(true);
        //                    }
        //                    catch
        //                    {
        //                    }
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                Debug.LogError(e.ToString());
        //            }
        //        }
        internal class Bundle
        {
            public string key { get; set; }
            public string path { get; set; }
            public string[] dependencyKeys { get; set; }
        }
    }
}
