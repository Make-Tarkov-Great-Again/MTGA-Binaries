using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FilesChecker;
using MTGA.Core;
using MTGA.Utilities;

namespace MTGA.Patches.FileChecker
{
    public class ConsistencySinglePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.FilesCheckerTypes.Single(x => x.Name == "ConsistencyController")
                .GetMethods().Single(x => x.Name == "EnsureConsistencySingle" && x.ReturnType == typeof(Task<ICheckResult>));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref object __result)
        {
            __result = Task.FromResult<ICheckResult>(new FakeFileCheckerResult());
            return false;
        }
    }
}
