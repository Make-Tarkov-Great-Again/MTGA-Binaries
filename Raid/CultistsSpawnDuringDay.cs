using System.Reflection;

// Credit goes to Lua
// https://hub.sp-tarkov.com/files/file/795-lua-s-cultists-spawn-patcher/
namespace MTGA.Core.Raid
{
    internal class CultistsSpawnDuringDay : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ZoneLeaveControllerClass).GetMethod("IsDayByHour");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}
