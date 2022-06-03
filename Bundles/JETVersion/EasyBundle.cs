//using Diz.DependencyManager;
//using JET.Utility.Patching;
//using System;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Threading.Tasks;
//using BundleLoader.Utilities;
//using UnityEngine;
//using SIT.Tarkov.Core;

//namespace BundleLoader.Patches
//{
//    public class EasyBundle : ModulePatch
//    {
//        public EasyBundle() { }

//        protected override MethodBase GetTargetMethod()
//        {
//            return Shared.LoaderType.GetConstructors().First();
//        }

//        [PatchPrefix]
//        static bool PatchPrefix(object __instance, string key, string rootPath, AssetBundleManifest manifest, object bundleLock, ref string ___string_1, ref string ___string_0, ref Task ___task_0)
//        {
//            var easyBundle = new Utilities.EasyBundleHelper(__instance)
//            {
//                Key = key
//            };
//            ___string_1 = rootPath + key;
//            ___string_0 = Path.GetFileNameWithoutExtension(key);
//            if (manifest != null)
//                easyBundle.DependencyKeys = manifest.GetDirectDependencies(key);

//            var newInst = Activator.CreateInstance(Shared.LoadState.PropertyType, ELoadState.Unloaded, null);
//            Shared.LoadState.SetValue(__instance, newInst);
//            ___task_0 = null;

//            Shared.BundleLockField.SetValue(__instance, bundleLock);

//            return false;
//        }
//    }
//}
