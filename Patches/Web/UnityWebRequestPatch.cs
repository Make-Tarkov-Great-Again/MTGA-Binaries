using MTGA.Utilities.Core;
using System.Reflection;
using UnityEngine.Networking;

namespace MTGA
{ 
    // Commenting here, because this dont need if want to switch to http (Need to check)
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
