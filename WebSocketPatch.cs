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
            var targetInterface = PatchConstants.EftTypes
                .Single(
                x => x == typeof(GInterface143)
                && x.IsInterface);
            var typeThatMatches = PatchConstants.EftTypes.Single(x => targetInterface.IsAssignableFrom(x) && x.IsAbstract && !x.IsInterface);
            var wsMethod = typeThatMatches.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .SingleOrDefault(x => x.ReturnType == typeof(Uri));

            if (wsMethod == null)
                UnityEngine.Debug.LogError("WebSocket Patch:: Cannot find WebSocket TargetMethod");
            //else
            //{
            //    UnityEngine.Debug.LogError("WebSocket Patch:: WebSocket TargetMethod ::" + typeThatMatches.FullName + "." + wsMethod.Name + "");
            //}

            return wsMethod;
        }

        [PatchPostfix]
        private static Uri PatchPostfix(Uri __instance)
        {
            return new Uri(__instance.ToString().Replace("wss:", "ws:"));
        }
    }

    public class testIt : GInterface143
    {
        public ENotificationTransportType TransportType => throw new NotImplementedException();

        public IReadOnlyDictionary<string, string> UriParams => throw new NotImplementedException();

        public IReadOnlyDictionary<string, string> RequestHeaders => throw new NotImplementedException();

        public bool Closed => throw new NotImplementedException();

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Open(Action<long, byte[]> onMessage, Action<long, string> onError)
        {
            throw new NotImplementedException();
        }

        public void SetRequestHeader(string key, string value)
        {
            throw new NotImplementedException();
        }

        public void SetUri(string uri)
        {
            throw new NotImplementedException();
        }

        public void SetUriParam(string key, string value)
        {
            throw new NotImplementedException();
        }
    }
}
