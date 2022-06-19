using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.A.Tarkov.Core.AI
{
    internal class BotSystemHelpers
    {
        public static Type BotSystemType { get; set; }
        public static Object BotSystemInstance { get; set; }

        static BotSystemHelpers()
        {
            if (BotSystemType == null)
                BotSystemType = PatchConstants.EftTypes.Single(x => x.IsClass && PatchConstants.GetMethodForType(x, "AddActivePLayer") != null);
        }

        public static void AddActivePlayer(EFT.Player player)
        {
            if (BotSystemInstance == null)
            {
                PatchConstants.Logger.LogInfo("Can't AddActivePlayer when BotSystemInstance is NULL");
                return;
            }

            PatchConstants.GetMethodForType(BotSystemType, "AddActivePLayer").Invoke(BotSystemInstance, new object[] { player });


        }
    }
}
