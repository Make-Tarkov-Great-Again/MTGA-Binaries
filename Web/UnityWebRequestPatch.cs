using System.Reflection;
using UnityEngine.Networking;

namespace MTGA.Core
{
    public class UnityWebRequestPatch : ModulePatch
    {
        //private static CertificateHandler _certificateHandler = new FakeCertificateHandler();

        protected override MethodBase GetTargetMethod()
        {
            return typeof(UnityWebRequestTexture).GetMethod(nameof(UnityWebRequestTexture.GetTexture), new[] { typeof(string) });
        }

        [PatchPostfix]
        private static void PatchPostfix(UnityWebRequest __result)
        {
            __result.certificateHandler = new FakeCertificateHandler(); //_certificateHandler;
            __result.disposeCertificateHandlerOnDispose = true; //false
            __result.timeout = 15000; //1000;
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
