using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SIT.Tarkov.Core.SP.ScavMode
{
    public class ScavProfileLoadPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainApplication)
                .GetNestedTypes(PatchConstants.PrivateFlags)
                .Single(x =>
                    x.GetField("entryPoint") != null
                    && x.GetField("timeAndWeather") != null
                    && x.GetField("timeHasComeScreenController") != null
                    && x.GetField("location") != null
                    && x.Name.Contains("Struct"))
                .GetMethods(PatchConstants.PrivateFlags)
                .FirstOrDefault(x => x.Name == "MoveNext");
        }

        [PatchTranspiler]
        private static IEnumerable<CodeInstruction> PatchTranspile(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            // Search for code where backend.Session.getProfile() is called.
            var searchCode = new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(PatchConstants.SessionInterfaceType, "get_Profile"));
            var searchIndex = -1;

            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == searchCode.opcode && codes[i].operand == searchCode.operand)
                {
                    searchIndex = i;
                    break;
                }
            }

            // Patch failed.
            if (searchIndex == -1)
            {
                Logger.LogError(string.Format("Patch {0} failed: Could not find reference code.", MethodBase.GetCurrentMethod()));
                return instructions;
            }

            // Move back by 4. This is the start of this method call.
            // Note that we don't actually want to replace the code at searchIndex (which is a Ldloc0) since there is a branch
            // instruction prior to this instruction that leads to it and we can reuse a Ldloc0 instruction here.
            searchIndex -= 4;

            var brFalseLabel = generator.DefineLabel();
            var brLabel = generator.DefineLabel();
            var newCodes = CodeGenerator.GenerateInstructions(new List<Code>()
            {
                new Code(OpCodes.Ldloc_1),
                new Code(OpCodes.Ldfld, typeof(ClientApplication), "_backEnd"),
                new Code(OpCodes.Callvirt, PatchConstants.BackendInterfaceType, "get_Session"),
                new Code(OpCodes.Ldloc_1),
                new Code(OpCodes.Ldfld, typeof(MainApplication), "esideType_0"),
                new Code(OpCodes.Ldc_I4_0),
                new Code(OpCodes.Ceq),
                new Code(OpCodes.Brfalse, brFalseLabel),
                new Code(OpCodes.Callvirt, PatchConstants.SessionInterfaceType, "get_Profile"),
                new Code(OpCodes.Br, brLabel),
                new CodeWithLabel(OpCodes.Callvirt, brFalseLabel, PatchConstants.SessionInterfaceType, "get_ProfileOfPet"),
                new CodeWithLabel(OpCodes.Stfld, brLabel, typeof(MainApplication).GetNestedTypes(BindingFlags.NonPublic).Single(IsTargetNestedType), "profile")
            });

            codes.RemoveRange(searchIndex + 1, 5);
            codes.InsertRange(searchIndex + 1, newCodes);
            return codes.AsEnumerable();
        }

        private static bool IsTargetNestedType(System.Type nestedType)
        {
            return nestedType.GetMethods(PatchConstants.PrivateFlags)
                .Count(x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(IResult)) > 0 && nestedType.GetField("savageProfile") != null;
        }
    }
}
