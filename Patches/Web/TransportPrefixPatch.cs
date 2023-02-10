using MTGA.Utilities.Core;
using System;
using System.Reflection;

namespace MTGA.Patches.Web
{
    // This should work to https to http -slejm
    public class TransportPrefixCtorPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(TransportPrefix).GetConstructor(new Type[] { } );
        }

        [PatchPostfix]
        private static bool PatchPostfix()
        {
            TransportPrefix.TransportPrefixes.Clear();
            TransportPrefix.TransportPrefixes.Add(ETransportProtocolType.HTTPS, "http://");
            TransportPrefix.TransportPrefixes.Add(ETransportProtocolType.WSS, "ws://");

            Logger.LogInfo("TransportPrefix Ctor Is Done!");
            return false;
        }
    }

    public class TransportPrefixPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(TransportPrefix).GetMethod("CreateFromLegacyParams", BindingFlags.Static | BindingFlags.Public);
        }

        [PatchPostfix]
        private static bool PatchPostfix(Request legacyParams)
        {
            legacyParams.Url = legacyParams.Url.Replace("http://","https://"); //   We always replace it to nothing if its runned successfully
            Logger.LogInfo("TransportPrefix Is Done!");
            return true;
        }
    }
}
