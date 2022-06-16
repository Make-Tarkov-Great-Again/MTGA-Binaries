using Newtonsoft.Json;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.AI
{
    public class CreateFriendlyAIPatch : ModulePatch
    {
        public static bool? ShouldFriendlyAI = null;
        public static int? NumberOfFriendlies = 0;
        public static int? MaxNumberOfFriendlies = 4;

        public static EFT.LocalPlayer MyPlayer;
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.GetMethodForType(typeof(EFT.LocalPlayer), "Init");
        }

        [PatchPostfix]
        public static
            async
            void
            PatchPostfix(EFT.LocalPlayer __instance)
        {
            //Logger.LogInfo("CreateFriendlyAIPatch.PatchPostfix");

            //if (!ShouldFriendlyAI.HasValue)
            //{
            //    var result = new Request().PostJson("/client/raid/createFriendlyAI", JsonConvert.SerializeObject(new Dictionary<string, object>()));
            //    Logger.LogInfo("CreateFriendlyAIPatch.PatchPostfix.Result=" + result);
            //    if(bool.TryParse(result, out bool resultB))
            //    {
            //        ShouldFriendlyAI = resultB;
            //    }
            //}
            //if (ShouldFriendlyAI == false)
            //{
            //    Logger.LogInfo("CreateFriendlyAIPatch.PatchPostfix.Friendly AI is turned OFF");
            //    return;
            //}
            //if (ShouldFriendlyAI.Value == true)
            //{
            //    //var aiData = PatchConstants.GetFieldOrPropertyFromInstance<object>(__instance, "AIData");
            //    //var botsGroup = PatchConstants.GetFieldOrPropertyFromInstance<object>(__instance, "BotsGroup");

            //    if (__instance.Profile.AccountId == PatchConstants.GetPHPSESSID())
            //        MyPlayer = __instance;
            //    else if (__instance.IsAI
            //        //&& aiData != null
            //        //&& botsGroup != null
            //        )
            //    {
            //        Logger.LogInfo("CreateFriendlyAIPatch.PatchPostfix.Spawning AI");
            //        if (NumberOfFriendlies < MaxNumberOfFriendlies)
            //        {
            //            if (__instance.Side == MyPlayer.Side)
            //            {
            //                Logger.LogInfo($"CreateFriendlyAIPatch.PatchPostfix.Creating Friendly AI #{NumberOfFriendlies}");

            //                //PatchConstants.GetMethodForType(botsGroup.GetType(), "RemoveInfo").Invoke(botsGroup, new object[] { MyPlayer });
            //                //PatchConstants.GetMethodForType(botsGroup.GetType(), "AddNeutral").Invoke(botsGroup, new object[] { MyPlayer });
            //                //__instance.BotsGroup.RemoveInfo(MyPlayer);
            //                //__instance.BotsGroup.AddNeutral(MyPlayer);
            //                //__instance.Teleport(MyPlayer.Position, true);
            //                NumberOfFriendlies++;
            //            }
            //        }
            //    }
            //}
        }
    }
}
