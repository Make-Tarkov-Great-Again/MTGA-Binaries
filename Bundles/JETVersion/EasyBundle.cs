using Diz.DependencyManager;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using SIT.Tarkov.Core;
using SIT.Tarkov.Core.Bundles;

namespace BundleLoader.Patches
{
    public class EasyBundle : ModulePatch
    {
        public EasyBundle() { }

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

        static PropertyInfo _loadState;

        public static PropertyInfo LoadState
        {
            get
            {
                if (_loadState == null)
                    _loadState = LoaderType.GetProperty("LoadState");
                return _loadState;
            }
        }

        static FieldInfo _bundleLockField;

        public static FieldInfo BundleLockField
        {
            get
            {
                if (_bundleLockField == null)
                    _bundleLockField = LoaderType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.FieldType == BundleSetup.BundleLockType);
                return _bundleLockField;
            }
        }

        protected override MethodBase GetTargetMethod()
        {
            return LoaderType.GetConstructors().First();
        }

        [PatchPrefix]
        static bool PatchPrefix(object __instance, string key, string rootPath, AssetBundleManifest manifest, object bundleLock, ref string ___string_1, ref string ___string_0, ref Task ___task_0)
        {
            //            var easyBundle = new Utilities.EasyBundleHelper(__instance)
            //            {
            //                Key = key
            //            };
            var dependencyKeys = manifest.GetDirectDependencies(key) ?? new string[0];
            //___string_1 = rootPath + key;
            //___string_0 = Path.GetFileNameWithoutExtension(key);
            //                easyBundle.DependencyKeys = manifest.GetDirectDependencies(key);

            var newInst = Activator.CreateInstance(LoadState.PropertyType, ELoadState.Unloaded, null);
            LoadState.SetValue(__instance, newInst);
            //___task_0 = null;

            //BundleSetup.BundleLockType.
            BundleLockField.SetValue(__instance, bundleLock);

            return false;
            //var path = string.Empty;
            //if (BundleManager.Bundles.TryGetValue(key, out BundleInfo bundle))
            //{
            //    dependencyKeys = (dependencyKeys.Length > 0) ? dependencyKeys.Union(bundle.DependencyKeys).ToArray() : bundle.DependencyKeys;
            //    path = bundle.Path;
            //}

            //Type[] typeArgs = { typeof(ELoadState) };
            //Type constructed = BundleSetup.BindableStateType.MakeGenericType(typeArgs);
            //_ = new EasyBundleHelper(__instance)
            //{
            //    Key = key,
            //    Path = path,
            //    KeyWithoutExtension = Path.GetFileNameWithoutExtension(key),
            //    DependencyKeys = dependencyKeys,
            //    //LoadState = new BindableState<ELoadState>(ELoadState.Unloaded, null),
            //    LoadState = Activator.CreateInstance(constructed, ELoadState.Unloaded, null),
            //    BundleLock = bundleLock
            //};
            //return true;
        }
    }
}
