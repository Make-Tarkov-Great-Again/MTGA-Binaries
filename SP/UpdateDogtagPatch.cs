using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

//using Equipment = GClass2069; // GetSlot
//using StDamage = GStruct252; // HittedBallisticCollider

namespace SIT.Tarkov.Core.SP
{
    class UpdateDogtagPatch : ModulePatch
    {
        //private static Func<Player, Equipment> getEquipmentProperty;

        public UpdateDogtagPatch() 
        {
            // compile-time checks
            //_ = nameof(Equipment.GetSlot);
            //_ = nameof(StDamage.Weapon);

            //getEquipmentProperty = typeof(Player)
            //    .GetProperty("Equipment", BindingFlags.NonPublic | BindingFlags.Instance)
            //    .GetGetMethod(true)
            //    .CreateDelegate(typeof(Func<Player, Equipment>)) as Func<Player, Equipment>;
        }

        protected override MethodBase GetTargetMethod() => typeof(Player)
            .GetMethod("OnBeenKilledByAggressor", BindingFlags.NonPublic | BindingFlags.Instance);

        [PatchPostfix]
        public static void PatchPostfix(Player __instance, Player aggressor, object damageInfo)
        {
            if (__instance.Profile.Info.Side == EPlayerSide.Savage)
            {
                return;
            }

            //var equipment = getEquipmentProperty(__instance);
            var equipment = PatchConstants.GetAllPropertiesForObject(__instance).Single(x => x.Name == "Equipment").GetValue(__instance);
            var dogtagSlot = PatchConstants.GetAllMethodsForType(equipment.GetType()).Single(x => x.Name == "GetSlot").Invoke(equipment, new object[] { EquipmentSlot.Dogtag });
            var dogtagItem = PatchConstants.GetFieldOrPropertyFromInstance<object>(dogtagSlot, "ContainedItem", false) as Item;
            //var dogtagSlot = equipment.GetSlot(EquipmentSlot.Dogtag);
            //var dogtagItem = dogtagSlot.ContainedItem as Item;

            if (dogtagItem == null)
            {
                Debug.LogError("[DogtagPatch] error > DogTag slot item is null somehow.");
                return;
            }

            MethodInfo method = PatchConstants.GetAllMethodsForType(dogtagItem.GetType()).Single(x => x.Name == "GetItemComponent");
            MethodInfo generic = method.MakeGenericMethod(typeof(DogtagComponent));
            var itemComponent = generic.Invoke(dogtagItem, null);

            //var itemComponent = dogtagItem.GetItemComponent<DogtagComponent>();

            if (itemComponent == null)
            {
                Logger.LogInfo("[DogtagPatch] error > DogTagComponent on dog tag slot is null");
                return;
            }

            var dogTagComponent = itemComponent as DogtagComponent;
            if (dogTagComponent == null)
            {
                Logger.LogInfo("[DogtagPatch] error > itemComponent is not DogTagComponent");
                return;
            }

            var victimProfileInfo = __instance.Profile.Info;

            dogTagComponent.AccountId = __instance.Profile.AccountId;
            dogTagComponent.ProfileId = __instance.Profile.Id;
            dogTagComponent.Nickname = victimProfileInfo.Nickname;
            dogTagComponent.Side = victimProfileInfo.Side;
            dogTagComponent.KillerName = aggressor.Profile.Info.Nickname;
            dogTagComponent.Time = DateTime.Now;
            dogTagComponent.Status = "Killed by ";
            dogTagComponent.KillerAccountId = aggressor.Profile.AccountId;
            dogTagComponent.KillerProfileId = aggressor.Profile.Id;
            ////itemComponent.WeaponName = damageInfo.Weapon.Name;

            if (__instance.Profile.Info.Experience > 0)
            {
                dogTagComponent.Level = victimProfileInfo.Level;
            }
        }
    }
}
