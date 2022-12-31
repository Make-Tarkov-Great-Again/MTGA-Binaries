using MTGA.Core;
using System;
using System.Linq;
using System.Reflection;

using MTGARequest = MTGA.Core.Request;

namespace MTGA.Patches.Web
{
    /// <summary>
    /// The Web Socket patch affects the Notifications pushed from Web Server to the Client via Sockets
    /// The client's class that handles all of this contains OnNotificationReceived, Activate
    /// If you look at "method_7" (at the time of writing), you can see how it parses the notification via ENotificationType to the Client to process
    /// </summary>
    public class WebSocketPatch : ModulePatch
    {
        public static string WebSocketAddress { get; private set; }
        public static Uri WebSocketUri { get; private set; }

        /// <summary>
        /// On creation of the WebSocketPatch Instance, Get the web server's determined WebSocket address
        /// </summary>
        public WebSocketPatch()
        {
            WebSocketAddress = new MTGARequest().PostJson("/client/WebSocketAddress", null);

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
            var targetInterface = PatchConstants.TypesDictionary["EftTypes"]// PatchConstants.EftTypes
                .Single(
                x => 
                //x == typeof(GInterface143)
                x.GetMethod("SetUri", BindingFlags.Public | BindingFlags.Instance) != null
                && x.GetProperty("TransportType", BindingFlags.Public | BindingFlags.Instance) != null
                && x.IsInterface);
            var typeThatMatches = PatchConstants.EftTypes.Single(x => targetInterface.IsAssignableFrom(x) && x.IsAbstract && !x.IsInterface);
            var wsMethod = typeThatMatches.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .SingleOrDefault(x => x.ReturnType == typeof(Uri));

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
