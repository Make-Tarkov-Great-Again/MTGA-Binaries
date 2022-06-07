using EFT;
using FilesChecker;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SIT.A.Tarkov.Core.FileChecker
{
    public class FileCheckerMainApplicationPatch : ModulePatch
    {
        public FileCheckerMainApplicationPatch()
        {

        }

        protected override MethodBase GetTargetMethod() => PatchConstants.GetAllMethodsForType(typeof(MainApplication))
            .Single(x => x.GetParameters().Length >= 2
                && x.GetParameters()[0].Name == "ordinaryFileEnsuranceMode"
                && x.GetParameters()[1].Name == "criticalFileEnsuranceMode");

        [PatchPrefix]
        public static bool PatchPrefix(
            MainApplication __instance
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
