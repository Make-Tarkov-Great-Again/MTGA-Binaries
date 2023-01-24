using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EFT;
using FilesChecker;
using MTGA.Utilities.Core;
using MTGA.Utilities.FileChecker;

namespace MTGA.Patches.FileChecker
{
    public class ConsistencySinglePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(TarkovApplication).BaseType.GetMethod("DefaultBundleCheck", PatchConstants.PrivateFlags);
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref object __result)
        {
            __result = Task.CompletedTask;
            return false;
        }
    }
}
