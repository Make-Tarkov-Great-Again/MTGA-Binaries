﻿using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FilesChecker;
using MTGA.Utilities.Core;
using MTGA.Utilities.FileChecker;

namespace MTGA.Patches.FileChecker
{
    public class ConsistencyMultiPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.FilesCheckerTypes.Single(x => x.Name == "ConsistencyController")
                .GetMethods().Single(x => x.Name == "EnsureConsistency" && x.ReturnType == typeof(Task<ICheckResult>));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref object __result)
        {
            __result = Task.FromResult<ICheckResult>(new FakeFileCheckerResult());
            return false;
        }
    }
}
