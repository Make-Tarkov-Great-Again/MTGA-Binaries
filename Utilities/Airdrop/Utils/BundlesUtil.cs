using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
/***
 * Full Credit for this patch goes to SPT-AKI team. Specifically CWX!
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules. 
*/
namespace MTGA.Utilities.Airdrops.Utils
{
    public static class BundlesUtil
    {
		public static Dictionary<string, AssetBundle> LoadedBundles = new Dictionary<string, AssetBundle>();

		public static async Task<AssetBundle> LoadBundleAsync(string bundleName)
		{
			string bundlePath = bundleName;
			bundleName = Regex.Match(bundleName, @"[^//]*$").Value;

			if (LoadedBundles.TryGetValue(bundleName, out var bundle))
				return bundle;

			var bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);

			while (!bundleRequest.isDone)
				await Task.Yield();

			AssetBundle requestedBundle = bundleRequest.assetBundle;

			if (requestedBundle != null)
			{
				LoadedBundles.Add(bundleName, requestedBundle);
				return requestedBundle;
			}
			else
			{
				Debug.LogError($"Can't load bundle: {bundleName} (does it exist?), unknown error.");
				return null;
			}
		}

		public static async Task<T> LoadAssetAsync<T>(string bundle, string assetName = null) where T : Object
		{
			AssetBundle assetBundle = await LoadBundleAsync(bundle);
			AssetBundleRequest assetBundleRequest;

			if (assetName == null)
			{
				assetBundleRequest = assetBundle.LoadAllAssetsAsync<T>();
			}
			else
			{
				assetBundleRequest = assetBundle.LoadAssetAsync<T>(assetName);
			}

			while (!assetBundleRequest.isDone)
				await Task.Yield();

			var requestedObj = assetBundleRequest.allAssets[0] as T;

			if (requestedObj != null)
			{
				return requestedObj;
			}
			else
			{
				Debug.LogError($"Can't load GameObject from bundle: {bundle}, unknown error.");
                return null;
			}
		}

		public static void UnloadBundle(string bundleName, bool unloadAllLoadedObjects = false)
		{
			if (LoadedBundles.TryGetValue(bundleName, out var ab))
			{
				ab.Unload(unloadAllLoadedObjects);
				LoadedBundles.Remove(bundleName);
			}
			else
			{
				Debug.LogError($"AssetBundle '{bundleName}' already unloaded");
			}
		}
	}
}