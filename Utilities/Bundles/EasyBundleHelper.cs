﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BindableState = GClass2918<Diz.DependencyManager.ELoadState>;


namespace MTGA.Utilities.Bundles
{
    public class EasyBundleHelper
    {
        private const BindingFlags _flags = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo _pathField;
        private static readonly FieldInfo _keyWithoutExtensionField;
        private static readonly FieldInfo _bundleLockField;
        private static readonly PropertyInfo _dependencyKeysProperty;
        private static readonly PropertyInfo _keyProperty;
        private static readonly PropertyInfo _loadStateProperty;
        private static readonly MethodInfo _loadingCoroutineMethod;
        private readonly object _instance;
        public static readonly Type Type;

        static EasyBundleHelper()
        {
            _ = nameof(IBundleLock.IsLocked);
            _ = nameof(BindableState.Bind);

            Type = PatchConstants.EftTypes.Single(x => x.GetMethod("set_SameNameAsset", _flags) != null);
            PatchConstants.Logger.LogDebug($"EasyBundleHelper::{Type.FullName}");
            _pathField = Type.GetField("string_1", _flags);
            _keyWithoutExtensionField = Type.GetField("string_0", _flags);
            _bundleLockField = Type.GetFields(_flags).FirstOrDefault(x => x.FieldType == typeof(IBundleLock));
            _dependencyKeysProperty = Type.GetProperty("DependencyKeys");
            _keyProperty = Type.GetProperty("Key");
            _loadStateProperty = Type.GetProperty("LoadState");
            _loadingCoroutineMethod = Type.GetMethods(_flags).Single(x => x.GetParameters().Length == 0 && x.ReturnType == typeof(Task));
        }

        public EasyBundleHelper(object easyBundle)
        {
            _instance = easyBundle;
        }

        public IEnumerable<string> DependencyKeys
        {
            get
            {
                return (IEnumerable<string>)_dependencyKeysProperty.GetValue(_instance);
            }
            set
            {
                _dependencyKeysProperty.SetValue(_instance, value);
            }
        }

        public IBundleLock BundleLock
        {
            get
            {
                return (IBundleLock)_bundleLockField.GetValue(_instance);
            }
            set
            {
                _bundleLockField.SetValue(_instance, value);
            }
        }

        public string Path
        {
            get
            {
                return (string)_pathField.GetValue(_instance);
            }
            set
            {
                _pathField.SetValue(_instance, value);
            }
        }

        public string Key
        {
            get
            {
                return (string)_keyProperty.GetValue(_instance);
            }
            set
            {
                _keyProperty.SetValue(_instance, value);
            }
        }

        public BindableState LoadState
        {
            get
            {
                return (BindableState)_loadStateProperty.GetValue(_instance);
            }
            set
            {
                _loadStateProperty.SetValue(_instance, value);
            }
        }

        public string KeyWithoutExtension
        {
            get
            {
                return (string)_keyWithoutExtensionField.GetValue(_instance);
            }
            set
            {
                _keyWithoutExtensionField.SetValue(_instance, value);
            }
        }

        public Task LoadingCoroutine()
        {
            return (Task)_loadingCoroutineMethod.Invoke(_instance, new object[] { });
        }
    }
}
