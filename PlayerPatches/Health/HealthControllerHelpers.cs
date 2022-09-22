using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MTGA.Core.PlayerPatches.Health
{
    public class HealthControllerHelpers
    {
        public static Type GetDamageInfoType()
        {
            return PatchConstants.EftTypes.Single(
                x =>
                PatchConstants.GetAllMethodsForType(x).Any(y => y.Name == "GetOverDamage")
                );
        }

        public static DamageInfo ReadyMadeDamageInstance;

        public static DamageInfo CreateDamageInfoTypeFromDict(Dictionary<string, object> dict)
        {
            ReadyMadeDamageInstance = new DamageInfo();

            PatchConstants.GetFieldFromType(ReadyMadeDamageInstance.GetType(), "Damage").SetValue(ReadyMadeDamageInstance, float.Parse(dict["damage"].ToString()));
            PatchConstants.GetFieldFromType(ReadyMadeDamageInstance.GetType(), "DamageType").SetValue(ReadyMadeDamageInstance, Enum.Parse(typeof(EDamageType), dict["damageType"].ToString()));

            PatchConstants.GetFieldFromType(ReadyMadeDamageInstance.GetType(), "ArmorDamage").SetValue(ReadyMadeDamageInstance, float.Parse(dict["armorDamage"].ToString()));
            PatchConstants.GetFieldFromType(ReadyMadeDamageInstance.GetType(), "DidArmorDamage").SetValue(ReadyMadeDamageInstance, float.Parse(dict["didArmorDamage"].ToString()));
            PatchConstants.GetFieldFromType(ReadyMadeDamageInstance.GetType(), "DidBodyDamage").SetValue(ReadyMadeDamageInstance, float.Parse(dict["didBodyDamage"].ToString()));

            return ReadyMadeDamageInstance;
        }

        public static MethodInfo GetHealthControllerChangeHealthMethod(object healthController)
        {
            return healthController.GetType()
                .GetMethod("ChangeHealth", BindingFlags.Public | BindingFlags.Instance);
        }

        public static void ChangeHealth(object healthController, EBodyPart bodyPart, float value, object damageInfo)
        {
            GetHealthControllerChangeHealthMethod(healthController).Invoke(healthController, new object[] { bodyPart, value, damageInfo });
        }

        public static object GetActiveHealthController(object player)
        {
            object activeHealthController = PatchConstants.GetFieldOrPropertyFromInstance<object>(player, "ActiveHealthController", false);
            return activeHealthController;
        }

        public static bool IsAlive(object healthController)
        {
            bool isAlive = PatchConstants.GetFieldOrPropertyFromInstance<bool>(healthController, "IsAlive", false);
            return isAlive;
        }

        /// <summary>
        /// Gets the Body Part Health Value struct for provided health controller
        /// </summary>
        /// <param name="healthController"></param>
        /// <param name="bodyPart"></param>
        /// <returns></returns>
        public static EFT.HealthSystem.ValueStruct GetBodyPartHealth(object healthController, EBodyPart bodyPart)
        {
            var getbodyparthealthmethod = healthController.GetType().GetMethod("GetBodyPartHealth"
                , System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.FlattenHierarchy
                );
            if (getbodyparthealthmethod == null)
            {
                PatchConstants.Logger.LogInfo("HealthListener:GetBodyPartHealth not found!");
                return new EFT.HealthSystem.ValueStruct();
            }

            //PatchConstants.Logger.LogInfo("GetBodyPartHealth found!");

            return (EFT.HealthSystem.ValueStruct)getbodyparthealthmethod.Invoke(healthController, new object[] { bodyPart, false });
        }
    }
}
