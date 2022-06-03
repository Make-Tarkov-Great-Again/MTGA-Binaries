//using Diz.DependencyManager;
//using HarmonyLib;
//using JET.Utility.Patching;
//using SIT.Tarkov.Core;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.Networking;

//namespace BundleLoader.Patches
//{
//    public class BundleLoad : ModulePatch
//    {
//        private static readonly CertificateHandler _certificateHandler = new JET.FakeModels.CertificateHandler();

//        public BundleLoad()
//        {
//            var bundlesFolder = Path.Combine(AppContext.BaseDirectory, Shared.LOCAL_BUNDLES_PATH);

//            if (!Directory.Exists(bundlesFolder))
//                Directory.CreateDirectory(bundlesFolder);

//            var files = Directory.GetFiles(bundlesFolder, "*.bundle", SearchOption.AllDirectories);

//            foreach (var fileName in files)
//            {
//                var fullPath = Path.Combine(bundlesFolder, fileName).Replace('/', '\\');
//                AssetBundle customBundle = null;
//                try
//                {
//                    customBundle = AssetBundle.LoadFromFile(fullPath);
//                    var assets = customBundle.GetAllAssetNames();
//                    var bundlePath = Path.Combine(Application.dataPath, "StreamingAssets/Windows/", customBundle.name);
//                    if (!File.Exists(bundlePath))
//                    {
//                        Shared.CachedBundles.Add(customBundle.name, fullPath);
//                        Debug.Log("Cached modded bundle " + customBundle.name);
//                        var manifestPath = fullPath + ".manifest";
//                        if (File.Exists(manifestPath))
//                        {
//                            Shared.ManifestCache.Add(customBundle.name, manifestPath);
//                            Debug.Log("Cached manifest for " + customBundle.name);
//                        }
//                    }
//                    else
//                    {
//                        foreach (var assetName in assets)
//                        {
//                            if (!Shared.ModdedAssets.ContainsKey(customBundle.name))
//                                Shared.ModdedAssets.Add(customBundle.name, new List<string>());
//                            if (!Shared.ModdedBundlePaths.ContainsKey(customBundle.name))
//                                Shared.ModdedBundlePaths.Add(customBundle.name, fullPath);
//                            if (!Shared.ModdedAssets[customBundle.name].Contains(assetName))
//                                Shared.ModdedAssets[customBundle.name].Add(assetName);
//                        }
//                        Debug.Log("Cached modded assets for " + customBundle.name);
//                    }


//                }
//                catch (Exception e)
//                {
//                    Debug.LogError("Failed to Load modded bundle " + fullPath + ": " + e);
//                }

//                try { customBundle.Unload(true); } catch { }
//            }
//        }

//        protected override MethodBase GetTargetMethod() =>
//            Constants.Instance.TargetAssembly.GetTypes().Single(IsTargetType).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Single(IsTargetMethod);

//        private static bool IsTargetType(Type type) =>
//            type.IsClass && type.GetProperty("SameNameAsset") != null;

//        private static bool IsTargetMethod(MethodInfo method) =>
//            method.GetParameters().Length == 0 && method.ReturnType == typeof(Task);

//        [PatchPrefix]
//        private static bool PatchPrefix(object __instance, bool ___bool_0, ref Task ___task_0, ref string ___string_1, string ___string_0, ref Task __result)
//        {
//            var bundleLock = new Shared.BundleLockWrapper(Shared.BundleLockField.GetValue(__instance));
//            try
//            {
//                if (!File.Exists(___string_1) && Shared.CachedBundles.ContainsKey(___string_1))
//                    ___string_1 = Shared.CachedBundles[___string_1];

//                try
//                {
//                    ___task_0 = LoadTarkovBundle(__instance, bundleLock, ___bool_0, ___string_1, ___string_0);
//                }
//                catch (Exception e)
//                {
//                    Debug.LogError(e);
//                }
//                __result = ___task_0;
//            }
//            catch (Exception e)
//            {
//                Debug.LogError(e);
//            }

//            return false;
//        }

//        private static async Task LoadTarkovBundle(object instance, Shared.BundleLockWrapper bundleLock, bool bool_0, string bundleFilePath, string bundleVirtualPath)
//        {
//            var key = AccessTools.Property(instance.GetType(), "Key").GetValue(instance).ToString();
//            var loadStateInstance = Shared.LoadState.GetValue(instance);

//            while (bundleLock.IsLocked)
//            {
//                if (!bool_0)
//                {
//                    Shared.TaskField.SetValue(instance, null);
//                    return;
//                }

//                await Task.Delay(100);
//            }
//            if (!bool_0)
//            {
//                Shared.TaskField.SetValue(instance, null);
//                return;
//            }

//            bundleLock.Lock();

//            if (Shared.ModdedAssets.ContainsKey(key))
//            {


//                Debug.Log($"Patching bundle {key} with modded assets...");
//                //AssetBundle assetBundle;
//                var request = AssetBundle.LoadFromFileAsync(Shared.ModdedBundlePaths[key]);
//                if (request == null)
//                {
//                    Debug.LogError("Error: could not load bundle: " + key);
//                    Shared.LoadStateProperty.SetValue(loadStateInstance, ELoadState.Failed);
//                    bundleLock.Unlock();
//                    Shared.TaskField.SetValue(instance, null);
//                    return;
//                }

//                while (!request.isDone && request.progress < 1)
//                {
//                    AccessTools.Property(instance.GetType(), "Progress").SetValue(instance, request.progress * 0.5f);
//                    await Task.Delay(100);
//                }

//                var assetBundle = request.assetBundle;

//                if (assetBundle == null)
//                {
//                    Debug.LogError("Error: could not load bundle: " + key +
//                                         " at path " + bundleFilePath);
//                    Shared.LoadStateProperty.SetValue(loadStateInstance, ELoadState.Failed);
//                    bundleLock.Unlock();
//                    AccessTools.Property(instance.GetType(), "Progress").SetValue(instance, 0f);
//                    Shared.TaskField.SetValue(instance, null);
//                    return;
//                }

//                AccessTools.Property(instance.GetType(), "Progress").SetValue(instance, 0.5f);

//                var assetsRequest = assetBundle.LoadAllAssetsAsync();
//                while (!assetsRequest.isDone && assetsRequest.progress < 1)
//                {
//                    await Task.Delay(100);
//                }
//                var assetsList = assetsRequest.allAssets;

//                AccessTools.Property(instance.GetType(), "Progress").SetValue(instance, 0.9f);

//                var sameName = assetsList.Where(x => x.name == bundleVirtualPath);
//                if (sameName.Any())
//                    AccessTools.Property(instance.GetType(), "SameNameAsset").SetValue(instance, sameName.First());


//                AccessTools.Property(instance.GetType(), "Assets").SetValue(instance, assetsList.ToArray());
//                Shared.LoadStateProperty.SetValue(loadStateInstance, ELoadState.Loaded);
//                bundleLock.Unlock();
//                AccessTools.Property(instance.GetType(), "Progress").SetValue(instance, 1f);
//                Shared.TaskField.SetValue(instance, null);
//                Shared.BundleField.SetValue(instance, assetBundle);
//            }
//            else
//            {
//                var request = AssetBundle.LoadFromFileAsync(bundleFilePath);
//                if (request == null)
//                {
//                    Debug.LogError("Error: could not load bundle: " + key);
//                    Shared.LoadStateProperty.SetValue(loadStateInstance, ELoadState.Failed);
//                    bundleLock.Unlock();
//                    Shared.TaskField.SetValue(instance, null);
//                    return;
//                }

//                while (!request.isDone && request.progress < 1)
//                {
//                    AccessTools.Property(instance.GetType(), "Progress").SetValue(instance, request.progress * 0.5f);
//                    await Task.Delay(100);
//                }

//                var assetBundle = request.assetBundle;

//                if (assetBundle == null)
//                {
//                    Debug.LogError("Error: could not load bundle: " + key +
//                                         " at path " + bundleFilePath);
//                    Shared.LoadStateProperty.SetValue(loadStateInstance, ELoadState.Failed);
//                    bundleLock.Unlock();
//                    AccessTools.Property(instance.GetType(), "Progress").SetValue(instance, 0f);
//                    Shared.TaskField.SetValue(instance, null);
//                    return;
//                }

//                AccessTools.Property(instance.GetType(), "Progress").SetValue(instance, 0.5f);
//                var assetBundleRequest = assetBundle.LoadAllAssetsAsync();
//                while (!assetBundleRequest.isDone && assetBundleRequest.progress < 1)
//                {
//                    AccessTools.Property(instance.GetType(), "Progress").SetValue(instance, 0.5f + assetBundleRequest.progress * 0.4f);
//                    await Task.Delay(100);
//                }

//                AccessTools.Property(instance.GetType(), "Assets").SetValue(instance, assetBundleRequest.allAssets);

//                foreach (var asset in assetBundleRequest.allAssets)
//                {
//                    if (asset.name != bundleVirtualPath) continue;
//                    AccessTools.Property(instance.GetType(), "SameNameAsset").SetValue(instance, asset);
//                    Shared.TaskField.SetValue(instance, null);
//                    break;
//                }

//                Shared.LoadStateProperty.SetValue(loadStateInstance, ELoadState.Loaded);
//                bundleLock.Unlock();
//                AccessTools.Property(instance.GetType(), "Progress").SetValue(instance, 1f);
//                Shared.TaskField.SetValue(instance, null);
//                Shared.BundleField.SetValue(instance, assetBundle);
//            }
//        }
//    }
//}
