using EFT;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.Misc
{
    internal class MongoIDPatch : ModulePatch
    {
        public static bool MongoIDExists = PatchConstants.EftTypes.Any(x => x.FullName == "EFT.MongoID");

        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.EftTypes.Single(x => x.FullName == "EFT.MongoID").GetConstructor(new Type[] { typeof(string) });
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref string id)
        {
            if(id.StartsWith("I-") || id.Length > 24)
            {
                Logger.LogInfo("MongoIDPatch: Id to Parse: " + id);
                id = new MongoID(true);
                Logger.LogInfo("MongoIDPatch: NewId: " + id);
            }
            return true;
        }
    }
}
