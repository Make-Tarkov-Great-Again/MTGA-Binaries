using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.Bundles
{
    public static class BundleSetup
    {
        public static Type IEasyBundleType { get; set; }
        public static Type IBundleLockType { get; set; }
        public static Type BundleLockType { get; set; }
        public static Type DependancyGraphType { get; set; }
        public static Type BindableStateType { get; set; }
        

        public static void Init()
        {
            IEasyBundleType = PatchConstants.EftTypes.Single(x => x.IsInterface
                && 
                 ( PatchConstants.GetFieldFromType(x, "SameNameAsset") != null
                 || PatchConstants.GetPropertyFromType(x, "SameNameAsset") != null )
                
                );

            PatchConstants.Logger.LogInfo("BundleSetup.Init.IEasyBundleType:" + IEasyBundleType.Name);

            IBundleLockType = PatchConstants.EftTypes.Single(x => x.IsInterface
                &&
                 (PatchConstants.GetFieldFromType(x, "IsLocked") != null
                 || PatchConstants.GetPropertyFromType(x, "IsLocked") != null)
                && PatchConstants.GetAllMethodsForType(x).Any(x => x.Name == "Lock")
                );

            PatchConstants.Logger.LogInfo("BundleSetup.Init.IBundleLockType:" + IBundleLockType.Name);

            BundleLockType = PatchConstants.EftTypes.Single(x =>
                 (PatchConstants.GetFieldFromType(x, "IsLocked") != null
                 || PatchConstants.GetPropertyFromType(x, "IsLocked") != null)
                &&
                (PatchConstants.GetFieldFromType(x, "MaxConcurrentOperations") != null
                 || PatchConstants.GetPropertyFromType(x, "MaxConcurrentOperations") != null)
                &&
                PatchConstants.GetAllMethodsForType(x).Any(x => x.Name == "Lock")
                );

            PatchConstants.Logger.LogInfo("BundleSetup.Init.BundleLockType:" + BundleLockType.Name);

            DependancyGraphType = PatchConstants.EftTypes.Single(x =>
                x.IsSealed
               &&
                PatchConstants.GetAllMethodsForType(x).Any(x => x.Name == "Retain")
               &&
               PatchConstants.GetAllMethodsForType(x).Any(x => x.Name == "RetainSeparate")
               &&
               PatchConstants.GetAllMethodsForType(x).Any(x => x.Name == "GetNode")
               );

            PatchConstants.Logger.LogInfo("BundleSetup.Init.DependancyGraphType:" + DependancyGraphType.Name);

            BindableStateType = PatchConstants.EftTypes.Single(x =>
               x.IsSealed
              &&
               x.GetConstructors().Length >= 2
              && 
              x.IsGenericTypeDefinition
              && (PatchConstants.GetFieldFromType(x, "Value") != null
                 || PatchConstants.GetPropertyFromType(x, "Value") != null)
              && (PatchConstants.GetFieldFromType(x, "HasHandlers") != null
                 || PatchConstants.GetPropertyFromType(x, "HasHandlers") != null)
              );

            PatchConstants.Logger.LogInfo("BundleSetup.Init.BindableStateType:" + BindableStateType.Name);
        }
    }
}
