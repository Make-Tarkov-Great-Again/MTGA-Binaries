using System.Linq;
using System.Reflection;
using UnityEngine.Networking;

namespace SIT.Tarkov.Core
{
	public class SslCertificatePatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return PatchConstants.EftTypes.Single(x => x.BaseType == typeof(CertificateHandler))
				.GetMethod("ValidateCertificate", PatchConstants.PrivateFlags);
		}

		[PatchPrefix]
		private static bool PatchPrefix(ref bool __result)
		{
			__result = true;
			return false;
		}
	}
}
