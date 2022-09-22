using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MTGA.Core.SP.Raid
{
    public class FilterProfilesPatch : ModulePatch
    {
        private static Type _targetInterface;
        private static Type _targetType;
        private static FieldInfo _wildSpawnTypeField;
        private static FieldInfo _botDifficultyField;

        static FilterProfilesPatch()
        {
            _targetInterface = PatchConstants.EftTypes.Single(IsTargetInterface);
            _targetType = PatchConstants.EftTypes.Single(IsTargetType);
            _wildSpawnTypeField = _targetType.GetField("wildSpawnType_0", PatchConstants.PrivateFlags);
            _botDifficultyField = _targetType.GetField("botDifficulty_0", PatchConstants.PrivateFlags);
        }

        private static bool IsTargetInterface(Type type)
        {
            return type.IsInterface && type.GetMethod("ChooseProfile", new[] { typeof(List<EFT.Profile>), typeof(bool) }) != null;
        }

        private static bool IsTargetType(Type type)
        {
            if (!_targetInterface.IsAssignableFrom(type) || type.GetMethod("method_1", PatchConstants.PrivateFlags) == null)
            {
                return false;
            }

            var fields = type.GetFields(PatchConstants.PrivateFlags);
            return fields.Any(f => f.FieldType != typeof(WildSpawnType)) && fields.Any(f => f.FieldType == typeof(BotDifficulty));
        }

        protected override MethodBase GetTargetMethod()
        {
            return _targetType.GetMethod("method_1", PatchConstants.PrivateFlags);
        }

        [PatchPrefix]
        private static bool PatchPrefix(
            ref bool __result
            , object __instance
            , Profile x)
        {
            var botType = (WildSpawnType)_wildSpawnTypeField.GetValue(__instance);
            var botDifficulty = (BotDifficulty)_botDifficultyField.GetValue(__instance);

            //Logger.LogInfo("FilterProfilesPatch");
            //Logger.LogInfo("Wildspawn BotType: " + botType);
            //Logger.LogInfo("Profile BotType: " + x.Info.Settings.Role);
            //Logger.LogInfo("Wildspawn BotDifficulty: " + botDifficulty);
            //Logger.LogInfo("Profile BotDifficulty: " + x.Info.Settings.BotDifficulty);
            //Logger.LogInfo("Profile Side: " + x.Side);

            __result = x.Info.Settings.Role == botType;// && profile.Info.Settings.BotDifficulty == botDifficulty;
            return false;
        }
    }
}
