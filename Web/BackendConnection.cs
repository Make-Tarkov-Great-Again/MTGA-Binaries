using Newtonsoft.Json;
using System;

namespace MTGA.Core.Web
{
    public class BackendConnection
    {
        public string BackendUrl { get; }
        public string Version { get; }

        public string PHPSESSID { get; private set; }

        public BackendConnection(string backendUrl, string version)
        {
            BackendUrl = backendUrl;
            Version = version;
        }

        static BackendConnection Instance;

        private static BackendConnection CreateBackendConnectionFromEnvVars()
        {
            if (Instance != null)
                return Instance;

            string[] args = Environment.GetCommandLineArgs();

            // Get backend url
            foreach (string arg in args)
            {
                PatchConstants.Logger.LogInfo(arg);

                if (arg.Contains("BackendUrl"))
                {
                    string json = arg.Replace("-config=", string.Empty);
                    Instance = JsonConvert.DeserializeObject<BackendConnection>(json);
                }
            }

            // get token / phpsessid
            foreach (string arg in args)
            {
                if (arg.Contains("-token="))
                {
                    Instance.PHPSESSID = arg.Replace("-token=", string.Empty);
                }
            }

            return Instance;
        }

        public static BackendConnection GetBackendConnection()
        {
            if (Instance != null)
                return Instance;

            return CreateBackendConnectionFromEnvVars();
        }
    }
}
