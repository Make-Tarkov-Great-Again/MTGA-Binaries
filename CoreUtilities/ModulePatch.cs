﻿using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;

namespace MTGA.Core
{
    public abstract class ModulePatch
    {
        private readonly Harmony _harmony;
        private readonly List<HarmonyMethod> _prefixList;
        private readonly List<HarmonyMethod> _postfixList;
        private readonly List<HarmonyMethod> _transpilerList;
        private readonly List<HarmonyMethod> _finalizerList;
        private readonly List<HarmonyMethod> _ilmanipulatorList;

        protected static ManualLogSource Logger { get; private set; }
        public string ThisTypeName { get; private set; }

        protected ModulePatch() : this(null)
        {
            ThisTypeName = GetType().FullName;
            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(ModulePatch));
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        protected ModulePatch(string name = null)
        {
            _harmony = new Harmony(name ?? GetType().Name);
            _prefixList = GetPatchMethods(typeof(PatchPrefixAttribute));
            _postfixList = GetPatchMethods(typeof(PatchPostfixAttribute));
            _transpilerList = GetPatchMethods(typeof(PatchTranspilerAttribute));
            _finalizerList = GetPatchMethods(typeof(PatchFinalizerAttribute));
            _ilmanipulatorList = GetPatchMethods(typeof(PatchILManipulatorAttribute));

            if (_prefixList.Count == 0
				&& _postfixList.Count == 0
				&& _transpilerList.Count == 0
				&& _finalizerList.Count == 0
				&& _ilmanipulatorList.Count == 0)
            {
                throw new Exception($"{_harmony.Id}: At least one of the patch methods must be specified");
            }
        }

        /// <summary>
        /// Get original method
        /// </summary>
        /// <returns>Method</returns>
        protected abstract MethodBase GetTargetMethod();

        /// <summary>
        /// Get HarmonyMethod from string
        /// </summary>
        /// <param name="attributeType">Attribute type</param>
        /// <returns>Method</returns>
        private List<HarmonyMethod> GetPatchMethods(Type attributeType)
        {
	        var T = GetType();
	        var methods = new List<HarmonyMethod>();

	        foreach (var method in T.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
	        {
		        if (method.GetCustomAttribute(attributeType) != null)
		        {
			        methods.Add(new HarmonyMethod(method));
                }
	        }

            return methods;
        }

        /// <summary>
        /// Apply patch to target
        /// </summary>
        public void Enable()
        {
            var target = GetTargetMethod();

            if (target == null)
            {
                throw new InvalidOperationException($"{_harmony.Id}: TargetMethod is null");
            }

            try
            {
                foreach (var prefix in _prefixList)
                {
                    _harmony.Patch(target, prefix: prefix);
                }

                foreach (var postfix in _postfixList)
                {
		            _harmony.Patch(target, postfix: postfix);
                }

	            foreach (var transpiler in _transpilerList)
                {
		            _harmony.Patch(target, transpiler: transpiler);
                }

	            foreach (var finalizer in _finalizerList)
                {
		            _harmony.Patch(target, finalizer: finalizer);
                }

	            foreach (var ilmanipulator in _ilmanipulatorList)
                {
		            _harmony.Patch(target, ilmanipulator: ilmanipulator);
                }

                //Logger.LogInfo($"Enabled patch {_harmony.Id}");
            }
            catch (Exception ex)
            {
                throw new Exception($"{_harmony.Id}:", ex);
            }
        }

        /// <summary>
        /// Remove applied patch from target
        /// </summary>
        public void Disable()
        {
            var target = GetTargetMethod();

            if (target == null)
            {
                throw new InvalidOperationException($"{_harmony.Id}: TargetMethod is null");
            }

            try
            {
                _harmony.Unpatch(target, HarmonyPatchType.All, _harmony.Id);
                Logger.LogInfo($"Disabled patch {_harmony.Id}");
            }
            catch (Exception ex)
            {
                throw new Exception($"{_harmony.Id}:", ex);
            }
        }
    }
}
