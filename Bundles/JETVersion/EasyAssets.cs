//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Cache;
//using System.Threading.Tasks;
//using System.IO;
//using System.Reflection;
//using Diz.Jobs;
//using Diz.Resources;
//using HarmonyLib;
//using JetBrains.Annotations;
//using UnityEngine;
//using JET.Utility.Patching;
//using Newtonsoft.Json;
//using UnityEngine.Build.Pipeline;
//using JET.Utility;
//using SIT.Tarkov.Core;

//namespace BundleLoader.Patches
//{
//    /// <summary>
//    /// This Patch will ask the server for bundles it requires for game to run properly, will also try to save bundles on your hard drive
//    /// </summary>
//    public class EasyAssets : ModulePatch
//    {
//        private const string BundleUrl = "/singleplayer/bundles";
//        private static readonly WebClient _client = new WebClient { CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore) };

//        public EasyAssets() { }

//        protected override MethodBase GetTargetMethod()
//        {
//            var targetType = Constants.Instance.TargetAssembly.GetTypes().First(IsTargetType);
//            return targetType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.Name == "method_0");
//        }

//        private static bool IsTargetType(Type type)
//        {
//            return type.IsClass && type.Name.EndsWith("EasyAssets");
//        }

//       [PatchPrefix]
//        private static bool PatchPrefix(Diz.Resources.EasyAssets __instance, object bundleLock, string defaultKey, string rootPath, string platformName, Func<string, bool> shouldExclude, Func<string, Task> bundleCheck)
//        {
//            CacheServerBundles();

//            var text = rootPath.Replace("file:///", "").Replace("file://", "") + "/" + platformName + "/";
//            var results =
//            JsonConvert.DeserializeObject<Dictionary<string, Shared.BundleDetailStruct>>(File.ReadAllText(text + platformName + ".json"))
//                .ToDictionary(k => k.Key, v => new BundleDetails
//                {
//                    FileName = v.Value.FileName,
//                    Crc = v.Value.Crc,
//                    Dependencies = v.Value.Dependencies
//                });
//            foreach (var (key, value) in Shared.CachedBundles)
//            {
//                uint i = 100;
//                var detail = new BundleDetails()
//                {
//                    FileName = value,
//                    Crc = i++,
//                    Dependencies = Shared.ManifestCache.ContainsKey(key)
//                        ? File.ReadAllLines(Shared.ManifestCache[key])
//                        : new string[] { }
//                };

//                results.Add(key, detail);
//            }
//            __instance.Manifest = ScriptableObject.CreateInstance<CompatibilityAssetBundleManifest>();
//            __instance.Manifest.SetResults(results);

//            var allAssetBundles = __instance.Manifest.GetAllAssetBundles();
//            var bundles = new object[allAssetBundles.Length];
//            bundleLock = Shared.BundleLockConstructor.Invoke(new object[] {int.MaxValue}); // there was ??= but it was throwing errors so it got removed
//            for (var i = 0; i < allAssetBundles.Length; i++)
//            {
//                bundles[i] = Activator.CreateInstance(Shared.LoaderType, allAssetBundles[i], string.Empty, __instance.Manifest, bundleLock, bundleCheck);
//                JobScheduler.Yield().GetAwaiter();
//            }

//            AccessTools.Property(__instance.GetType(), "System").SetValue(__instance, Activator.CreateInstance(Shared.NodeType, bundles, defaultKey, shouldExclude));
//            return false;
//        }

//        private static string GetLocalBundlePath(Bundle bundle)
//        {
//            // lets wrap everything in trycatch cause Unity
//            try
//            {
//                Debug.LogError(bundle.key);
//                Debug.LogError(bundle.path);
//                var local = false;
//                var backend = new Uri(ClientAccesor.BackendUrl);

//                // Check if host is local
//                if (IPAddress.TryParse(backend.Host, out var ip))
//                    if (ip.MapToIPv4().ToString().StartsWith("127"))
//                        local = true;


//                if (local && File.Exists(bundle.path))
//                    return bundle.path;

//                // Check local bundles folder
//                var possibleLocalPath = Path.Combine(Shared.LOCAL_BUNDLES_PATH, bundle.key);
//                if (File.Exists(possibleLocalPath))
//                    return possibleLocalPath;

//                // Check local cache
//                var cachePath = Path.Combine(Shared.CACHE_BUNDLES_PATH, backend.Host, bundle.key);
//                if (File.Exists(cachePath))
//                    return cachePath;

//                // Download bundle and put it in the cache folder
//                var url = ClientAccesor.BackendUrl + "/files/bundle/" + bundle.key;
//                var dirPath = Path.GetDirectoryName(cachePath);
//                if (!Directory.Exists(dirPath))
//                    Directory.CreateDirectory(dirPath);

//                Debug.Log("Downloading bundle from " + url);

//                _client.DownloadFile(url, cachePath);

//                return cachePath;
//            }
//            catch (Exception e)
//            {
//                Debug.Log(e.Data);
//                return null;
//            }
//        }

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
//        internal class Bundle
//        {
//            public string key { get; set; }
//            public string path { get; set; }
//            public string[] dependencyKeys { get; set; }
//        }
//    }
//}
