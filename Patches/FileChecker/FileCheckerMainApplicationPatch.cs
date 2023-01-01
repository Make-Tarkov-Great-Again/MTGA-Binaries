using EFT;
using FilesChecker;
using MTGA.Utilities.Core;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MTGA.FileChecker
{
    public class FileCheckerTarkovApplicationPatch : ModulePatch
    {
        public FileCheckerTarkovApplicationPatch()
        {

        }

        protected override MethodBase GetTargetMethod() => PatchConstants.GetAllMethodsForType(typeof(TarkovApplication))
            .Single(x => x.GetParameters().Length >= 2
                && x.GetParameters()[0].Name == "ordinaryFileEnsuranceMode"
                && x.GetParameters()[1].Name == "criticalFileEnsuranceMode");

        [PatchPrefix]
        public static bool PatchPrefix(
            TarkovApplication __instance
            , ConsistencyEnsuranceMode ordinaryFileEnsuranceMode
            , ConsistencyEnsuranceMode criticalFileEnsuranceMode
            , object timeHasComeScreenController
            , CancellationToken token
            , Task __result
            )
        {
            __result.GetAwaiter().GetResult();
            return true;
        }
    }
}
