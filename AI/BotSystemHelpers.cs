using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.AI
{
    public class BotSystemHelpers
    {
        public static Type BotControllerType { get; set; }
        public static Type BotPresetType { get; set; }
        public static Type BotScatteringType { get; set; }
        public static Type BossSpawnRunnerType { get; set; }

        public static Object BotControllerInstance { get; set; }
        public static MethodInfo SetSettingsMethod { get; set; }
        public static MethodInfo StopMethod { get; set; }
        public static MethodInfo AddActivePlayerMethod { get; set; }

        public static BepInEx.Logging.ManualLogSource Logger { get; set; }

        static BotSystemHelpers()
        {
            Setup();
        }

        public static void Setup()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource("SIT.Tarkov.Core.BotSystemHelpers");
            //Logger = PatchConstants.Logger;

            if (BotControllerType == null)
                //BotControllerType = PatchConstants.EftTypes.Single(x => PatchConstants.GetMethodForType(x, "AddActivePLayer") != null);
                BotControllerType = PatchConstants.EftTypes.Single(x => 
                    x.GetMethod("SetSettings", BindingFlags.Public | BindingFlags.Instance) != null
                    && x.GetMethod("AddActivePLayer", BindingFlags.Public | BindingFlags.Instance) != null
                );

            Logger.LogInfo($"{BotControllerType.Name}");

            if (BotPresetType == null)
                BotPresetType = PatchConstants.EftTypes.Single(x => x.IsClass
                    && PatchConstants.GetFieldFromType(x, "BotDifficulty") != null
                    && PatchConstants.GetFieldFromType(x, "Role") != null
                    && PatchConstants.GetFieldFromType(x, "UseThis") != null
                    && PatchConstants.GetFieldFromType(x, "VisibleAngle") != null
                    );

            Logger.LogInfo($"{BotPresetType.Name}");

            if (BotScatteringType == null)
                BotScatteringType = PatchConstants.EftTypes.Single(x => x.IsClass
                    && PatchConstants.GetFieldFromType(x, "PriorityScatter1meter") != null
                    && PatchConstants.GetFieldFromType(x, "PriorityScatter10meter") != null
                    && PatchConstants.GetFieldFromType(x, "PriorityScatter100meter") != null
                    && PatchConstants.GetMethodForType(x, "Check") != null
                    );

            Logger.LogInfo($"{BotScatteringType.Name}");

            if(BossSpawnRunnerType == null)
                BossSpawnRunnerType = PatchConstants.EftTypes.Single(x => x.IsClass
                    && PatchConstants.GetPropertyFromType(x, "HaveSectants") != null
                    && PatchConstants.GetPropertyFromType(x, "BossSpawnWaves") != null
                    && x.GetMethod("Run", BindingFlags.Public | BindingFlags.Instance) != null
                    );

            Logger.LogInfo($"{BossSpawnRunnerType.Name}");

            if (SetSettingsMethod == null)
                SetSettingsMethod = PatchConstants.GetMethodForType(BotControllerType, "SetSettings");

            Logger.LogInfo($"{SetSettingsMethod.Name}");

            if (StopMethod == null)
                StopMethod = PatchConstants.GetMethodForType(BotControllerType, "Stop");

            Logger.LogInfo($"{StopMethod.Name}");

            if (AddActivePlayerMethod == null)
                AddActivePlayerMethod = PatchConstants.GetMethodForType(BotControllerType, "AddActivePLayer");

            Logger.LogInfo($"{AddActivePlayerMethod.Name}");
        }

        public static void AddActivePlayer(EFT.Player player)
        {
            if (BotControllerInstance == null)
            {
                PatchConstants.Logger.LogInfo("Can't AddActivePlayer when BotSystemInstance is NULL");
                return;
            }

            AddActivePlayerMethod?.Invoke(BotControllerInstance, new object[] { player });
        }

        public static void SetSettingsNoBots()
        {
            if (BotControllerInstance == null)
            {
                PatchConstants.Logger.LogInfo("Can't SetSettings when BotSystemInstance is NULL");
                return;
            }

            var botPresets = Array.CreateInstance(BotPresetType, 0);
            var botScattering = Array.CreateInstance(BotScatteringType, 0);

            SetSettingsMethod?.Invoke(BotControllerInstance
                , new object[] { 0, botPresets, botScattering });
        }

        public static void Stop()
        {
            if (BotControllerInstance == null)
            {
                PatchConstants.Logger.LogInfo("Can't Stop when BotSystemInstance is NULL");
                return;
            }

            StopMethod?.Invoke(BotControllerInstance
                , new object[] {  });
        }
    }
}
