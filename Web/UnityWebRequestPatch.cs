using System.Reflection;
using UnityEngine.Networking;

namespace SIT.Tarkov.Core
{
    public class UnityWebRequestPatch : ModulePatch
    {
        private static CertificateHandler _certificateHandler = new FakeCertificateHandler();

        protected override MethodBase GetTargetMethod()
        {
            return typeof(UnityWebRequestTexture).GetMethod(nameof(UnityWebRequestTexture.GetTexture), new[] { typeof(string) });
        }

        [PatchPostfix]
        private static void PatchPostfix(UnityWebRequest __result)
        {
            __result.certificateHandler = _certificateHandler;
            __result.disposeCertificateHandlerOnDispose = false;
            __result.timeout = 1000;
        }

        internal class FakeCertificateHandler : CertificateHandler
        {
            public override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }
    }
}
