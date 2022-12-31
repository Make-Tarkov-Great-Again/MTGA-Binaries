using MTGA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace MTGA.Patches.Player
{

	/// <summary>
	/// https://github.com/S3RAPH-1M/SERVPH-Mods/tree/main/VisceralRagdolls
	/// Thanks to SERVPH for sharing this mod. I have only included this here since the original wont work with my remaing/patching of Assembly-CSharp and I love this mod!
	/// </summary>
	public class SERVPHBodyPatch : ModulePatch
	{
		private static String[] TargetBones = { "thigh", "calf", "foot", "spine3", "forearm", "head" };

		protected override MethodBase GetTargetMethod()
		{
			return PatchConstants.GetMethodForType(typeof(EFT.Player), "CreateCorpse");
		}

		[PatchPostfix]
		private static void Postfix(EFT.Player __instance)
		{
			if (__instance.IsYourPlayer)
			{
				return;
			}
			foreach (Transform child in EnumerateHierarchyCore(__instance.Transform.Original).Where(t => TargetBones.Any(u => t.name.ToLower().Contains(u))))
			{
				child.gameObject.layer = 6;
			}
		}

		private static IEnumerable<Transform> EnumerateHierarchyCore(Transform root)
		{
			Queue<Transform> transformQueue = new Queue<Transform>();
			transformQueue.Enqueue(root);

			while (transformQueue.Count > 0)
			{
				Transform parentTransform = transformQueue.Dequeue();

				if (!parentTransform)
				{
					continue;
				}

				for (Int32 i = 0; i < parentTransform.childCount; i++)
				{
					transformQueue.Enqueue(parentTransform.GetChild(i));
				}

				yield return parentTransform;
			}
		}
	}
}