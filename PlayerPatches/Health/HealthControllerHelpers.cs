using EFT;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.PlayerPatches.Health
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

        public static object ReadyMadeDamageInstance;

        public static object CreateDamageInfoTypeFromDict(Dictionary<string, object> dict)
        {
            if(ReadyMadeDamageInstance == null)
                ReadyMadeDamageInstance = Activator.CreateInstance(GetDamageInfoType());

            PatchConstants.GetFieldFromType(ReadyMadeDamageInstance.GetType(), "Damage").SetValue(ReadyMadeDamageInstance, float.Parse(dict["damage"].ToString()));
            PatchConstants.GetFieldFromType(ReadyMadeDamageInstance.GetType(), "DamageType").SetValue(ReadyMadeDamageInstance, Enum.Parse(typeof(EDamageType), dict["damageType"].ToString()));

            //PatchConstants.ConvertDictionaryToObject(ReadyMadeDamageInstance, dict);
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
