using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MTGA.Utilities.Core;

namespace MTGA.Patches.Web
{
    internal class WSS : ModulePatch
    {
        public static string WebSocketAddress { get; private set; }
        public static Uri WebSocketUri { get; private set; }

        /// <summary>
        /// On creation of the WebSocketPatch Instance, Get the web server's determined WebSocket address
        /// </summary>
        public WSS()
        {
            WebSocketAddress = Utilities.Core.Request.Instance.PostJson("/getwebsocket", null);

            if (Uri.TryCreate(WebSocketAddress, UriKind.Absolute, out Uri uri))
            {
                WebSocketUri = uri;
                Logger.LogInfo("[WEBSOCKET] Address is " + WebSocketAddress);
            }
            else
            {
                Logger.LogError("[WEBSOCKET] Failed to set websocket " + WebSocketAddress);
            }
        }
        protected override MethodBase GetTargetMethod()
        {
            var targetInterface = PatchConstants.EftTypes.Single(x => x == typeof(IConnectionHandler) && x.IsInterface);
            var typeThatMatches = PatchConstants.EftTypes.Single(x => targetInterface.IsAssignableFrom(x) && x.IsAbstract && !x.IsInterface);
            var wsMethod = typeThatMatches.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.ReturnType == typeof(Uri));

            if (wsMethod == null)
                Logger.LogError("WebSocket Patch:: Cannot find WebSocket TargetMethod");
            else
            {
                Logger.LogInfo("WebSocket Patch:: WebSocket TargetMethod ::" + typeThatMatches.FullName + "." + wsMethod.Name + "");
            }

            return wsMethod;
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref Uri __result)
        {
            __result = WebSocketUri;
            return false;
        }
    }
}
