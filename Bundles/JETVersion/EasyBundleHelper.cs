//using UnityEngine;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Threading.Tasks;
//using HarmonyLib;
//#region Information how to find "IBundleLock"
///*
// * To get IBundleLock Interface you open dnspy and Assembly-CSharp then you simbly search for everything with "IsLocked" property then you select the only GInterface that pops up with Property you were searching for.
// * that Interface should look like:

//public interface GInterface276
//{
//	bool IsLocked { get; }
//	void Lock();
//	void Unlock();
//}

// */
//#endregion
//using IBundleLock = GInterface275;
//#region Information how to find "BindableState"
///*
// * To get BindableState class you open dnspy and Assembly-CSharp then you simbly search for everything with value "initialValue" in variable then you open the only one GClass visible there
// * that GClass should look like this:

//public sealed class GClass2563<T> : GClass2562<T>
//{
//	public override T Value { set => base.method_0(value); }
//	public GClass2563() : base(null) { }
//	public GClass2563([CanBeNull] T initialValue, IEqualityComparer<T> equalityComparer = null) : base(equalityComparer)
//	{
//		this.gparam_0 = initialValue;
//	}
// }

// */
//#endregion
//using BindableState = GClass2563<Diz.DependencyManager.ELoadState>;

//namespace BundleLoader.Utilities
//{
//    class EasyBundleHelper
//    {
//        private readonly object _instance;
//        private readonly Traverse _trav;

//        // those string names of variables should be unique enough
//        private static readonly string _pathFieldName = "string_1";
//        private static readonly string _keyWithoutExtensionFieldName = "string_0";
//        private static readonly string _loadingJobPropertyName = "task_0";
//        private static readonly string _dependencyKeysPropertyName = "DependencyKeys";
//        private static readonly string _keyPropertyName = "Key";
//        private static readonly string _loadStatePropertyName = "LoadState";
//        private static readonly string _progressPropertyName = "Progress";
//        private static readonly string _bundlePropertyName = "assetBundle_0";
//        private static readonly string _loadingAssetOperationFieldName = "assetBundleRequest_0";
//        private static readonly string _assetsPropertyName = "Assets";
//        private static readonly string _sameNameAssetPropertyName = "SameNameAsset";

//        private static MethodInfo _loadingCoroutineMethod;

//        public IEnumerable<string> DependencyKeys
//        {
//            get
//            {
//                return _trav.Property<IEnumerable<string>>(_dependencyKeysPropertyName).Value;
//            }

//            set
//            {
//                _trav.Property<IEnumerable<string>>(_dependencyKeysPropertyName).Value = value;
//            }
//        }

//        public IBundleLock BundleLock
//        {
//            get
//            {
//                return _trav.Field<IBundleLock>($"{typeof(IBundleLock).Name.ToLower()}_0").Value;
//            }

//            set
//            {
//                _trav.Field<IBundleLock>($"{typeof(IBundleLock).Name.ToLower()}_0").Value = value;
//            }
//        }

//        public Task LoadingJob
//        {
//            get
//            {
//                return _trav.Field<Task>(_loadingJobPropertyName).Value;
//            }

//            set
//            {
//                _trav.Field<Task>(_loadingJobPropertyName).Value = value;
//            }
//        }

//        public string Path
//        {
//            get
//            {
//                return _trav.Field<string>(_pathFieldName).Value;
//            }

//            set
//            {
//                _trav.Field<string>(_pathFieldName).Value = value;
//            }
//        }

//        public string Key
//        {
//            get
//            {
//                return _trav.Property<string>(_keyPropertyName).Value;
//            }

//            set
//            {
//                _trav.Property<string>(_keyPropertyName).Value = value;
//            }
//        }

//        public BindableState LoadState
//        {
//            get
//            {
//                return _trav.Property<BindableState>(_loadStatePropertyName).Value;
//            }

//            set
//            {
//                _trav.Property<BindableState>(_loadStatePropertyName).Value = value;
//            }
//        }

//        public float Progress
//        {
//            get
//            {
//                return _trav.Property<float>(_progressPropertyName).Value;
//            }

//            set
//            {
//                _trav.Property<float>(_progressPropertyName).Value = value;
//            }
//        }


//        public AssetBundle Bundle
//        {
//            get
//            {
//                return _trav.Field<AssetBundle>(_bundlePropertyName).Value;
//            }

//            set
//            {
//                _trav.Field<AssetBundle>(_bundlePropertyName).Value = value;
//            }
//        }

//        public AssetBundleRequest loadingAssetOperation
//        {
//            get
//            {
//                return _trav.Field<AssetBundleRequest>(_loadingAssetOperationFieldName).Value;
//            }

//            set
//            {
//                _trav.Field<AssetBundleRequest>(_loadingAssetOperationFieldName).Value = value;
//            }
//        }


//        public Object[] Assets
//        {
//            get
//            {
//                return _trav.Property<UnityEngine.Object[]>(_assetsPropertyName).Value;
//            }

//            set
//            {
//                _trav.Property<UnityEngine.Object[]>(_assetsPropertyName).Value = value;
//            }
//        }

//        public UnityEngine.Object SameNameAsset
//        {
//            get
//            {
//                return _trav.Property<UnityEngine.Object>(_sameNameAssetPropertyName).Value;
//            }

//            set
//            {
//                _trav.Property<UnityEngine.Object>(_sameNameAssetPropertyName).Value = value;
//            }
//        }

//        public string KeyWithoutExtension
//        {
//            get
//            {
//                return _trav.Field<string>(_keyWithoutExtensionFieldName).Value;
//            }

//            set
//            {
//                _trav.Field<string>(_keyWithoutExtensionFieldName).Value = value;
//            }
//        }

//        public EasyBundleHelper(object easyBundle)
//        {
//            _instance = easyBundle;
//            _trav = Traverse.Create(easyBundle);

//            if (_loadingCoroutineMethod == null)
//            {
//                _loadingCoroutineMethod = easyBundle.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Single(x => x.GetParameters().Length == 0 && x.ReturnType == typeof(Task));
//                //TODO:Search member names by condition
//            }
//        }

//        public Task LoadingCoroutine()
//        {
//            return (Task)_loadingCoroutineMethod.Invoke(_instance, new object[] { });
//        }
//    }
//}
