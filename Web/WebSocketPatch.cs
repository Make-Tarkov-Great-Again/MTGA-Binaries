using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Tarkov.Core
{
    
    public class WebSocketPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            //_ = GInterface143;

            /*
                ENotificationTransportType TransportType { get; }
                IReadOnlyDictionary<string, string> UriParams { get; }
                IReadOnlyDictionary<string, string> RequestHeaders { get; }
                bool Closed { get; }

                void Close();
                void Open(Action<long, byte[]> onMessage, Action<long, string> onError);
                void SetRequestHeader(string key, string value);
                void SetUri(string uri);
                void SetUriParam(string key, string value);
             */
            //var targetInterface = PatchConstants.EftTypes.Single(x => x == typeof(IConnectionHandler) && x.IsInterface);
            //var typeThatMatches = PatchConstants.EftTypes.Single(x => targetInterface.IsAssignableFrom(x) && x.IsAbstract && !x.IsInterface);
            //return typeThatMatches.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.ReturnType == typeof(Uri));
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
            if (__result != null)
            {
                Logger.LogInfo("WebSocketPatch:PatchPrefix:" + __result.ToString());
            }
            if (PatchConstants.GetBackendUrl().Contains("https"))
            {
                __result = new Uri(PatchConstants.GetBackendUrl().Replace("https", "wss"));
                Logger.LogInfo("[WEBSOCKET] URL contains https, changing to wss");

            } 
            else
            {
                __result = new Uri(PatchConstants.GetBackendUrl().Replace("http", "ws"));
                Logger.LogInfo("[WEBSOCKET] URL contains http, changing to ws");
            }
            Logger.LogInfo("[WEBSOCKET] Patched");
            return false;
        }

        //[PatchPostfix]
        //private static void PatchPostfix(ref Uri __result)
        //{
        //    Logger.LogInfo("WebSocketPatch:PatchPostfix:" + __result.ToString());
        //    //return new Uri(__instance.ToString().Replace("wss:", "ws:"));
        //    __result = new Uri(PatchConstants.GetBackendUrl().Replace("https", "ws"));
        //}
    }

}
