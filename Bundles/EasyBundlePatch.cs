using Diz.DependencyManager;
using UnityEngine.Build.Pipeline;
using System.IO;
using System.Linq;
using System.Reflection;
using System;
using SIT.Tarkov.Core.Bundles;
using UnityEngine;

/***
 * Full Credit for this patch goes to SPT-Aki team
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules
 * Paulov. Made changes to have better reflection and less hardcoding
 */

namespace SIT.Tarkov.Core
{
    public class EasyBundlePatch : ModulePatch
    {
        static EasyBundlePatch()
        {
            //_ = nameof(IEasyBundle.SameNameAsset);
            //_ = nameof(IBundleLock.IsLocked);
            //_ = nameof(BindableState<ELoadState>.Bind);
        }

        protected override MethodBase GetTargetMethod()
        {
            return EasyBundleHelper.Type.GetConstructors()[0];
        }

        [PatchPostfix]
        private static void PatchPostfix(object __instance, string key, string rootPath, CompatibilityAssetBundleManifest manifest, object bundleLock)
        {

            var path = rootPath + key;
            var dependencyKeys = manifest.GetDirectDependencies(key) ?? new string[0];
            if (path.Contains("qhb") || path.Contains("l85a2"))
            {
                Logger.LogInfo("EasyBundlePatch:PatchPostfix:Initial Path:" + path);
            }

            if (BundleManager.Bundles.TryGetValue(key, out BundleInfo bundle))
            {
                dependencyKeys = (dependencyKeys.Length > 0) ? dependencyKeys.Union(bundle.DependencyKeys).ToArray() : bundle.DependencyKeys;
                path = bundle.Path;
            }

            if (path.Contains("qhb") || path.Contains("l85a2"))
            {
                Logger.LogInfo("EasyBundlePatch:PatchPostfix:Actual Path:" + path);
            }

            Type[] typeArgs = { typeof(ELoadState) };
            Type constructed = BundleSetup.BindableStateType.MakeGenericType(typeArgs);
            _ = new EasyBundleHelper(__instance)
            {
                Key = key,
                Path = path,
                KeyWithoutExtension = Path.GetFileNameWithoutExtension(key),
                DependencyKeys = dependencyKeys,
                //LoadState = new BindableState<ELoadState>(ELoadState.Unloaded, null),
                LoadState = Activator.CreateInstance(constructed, ELoadState.Unloaded, null),
                BundleLock = bundleLock
            };
        }
    }
}
