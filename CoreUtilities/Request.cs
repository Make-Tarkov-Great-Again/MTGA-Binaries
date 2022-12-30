using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using ComponentAce.Compression.Libs.zlib;
using System.Threading.Tasks;

namespace MTGA.Core
{
    public class Request : IDisposable
    {
        public static string Session;
        public static string RemoteEndPoint;
        public bool isUnity;

        public Request()
        {
            if(string.IsNullOrEmpty(Session))
                Session = PatchConstants.GetPHPSESSID();
            if(string.IsNullOrEmpty(RemoteEndPoint))
                RemoteEndPoint = PatchConstants.GetBackendUrl();
        }

        public Request(string session, string remoteEndPoint, bool isUnity = true)
        {
            Session = session;
            RemoteEndPoint = remoteEndPoint;
        }
        /// <summary>
        /// Send request to the server and get Stream of data back
        /// </summary>
        /// <param name="url">String url endpoint example: /start</param>
        /// <param name="method">POST or GET</param>
        /// <param name="data">string json data</param>
        /// <param name="compress">Should use compression gzip?</param>
        /// <returns>Stream or null</returns>
        private Stream Send(string url, string method = "GET", string data = null, bool compress = true)
        {

            // disable SSL encryption
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var fullUri = url;
            if (!Uri.IsWellFormedUriString(fullUri, UriKind.Absolute))
                fullUri = RemoteEndPoint + fullUri;

            //PatchConstants.Logger.LogInfo(fullUri);

            WebRequest request = WebRequest.Create(new Uri(fullUri));
            //var request = WebRequest.CreateHttp(fullUri);

            if (!string.IsNullOrEmpty(Session))
            {
                request.Headers.Add("Cookie", $"PHPSESSID={Session}");
                request.Headers.Add("SessionId", Session);
            }

            request.Headers.Add("Accept-Encoding", "deflate");

            request.Method = method;

            if (method != "GET" && !string.IsNullOrEmpty(data))
            {
                // set request body
                byte[] bytes = (compress) ? SimpleZlib.CompressToBytes(data, zlibConst.Z_BEST_COMPRESSION) : Encoding.UTF8.GetBytes(data);

                request.ContentType = "application/json";
                request.ContentLength = bytes.Length;

                if (compress)
                {
                    request.Headers.Add("content-encoding", "deflate");
                }

                using Stream stream = request.GetRequestStream();
                stream.Write(bytes, 0, bytes.Length);
            }

            // get response stream
            try
            {
                WebResponse response = request.GetResponse();
                return response.GetResponseStream();
            }
            catch (Exception e)
            {
                if (isUnity)
                    Debug.LogError(e);
            }

            return null;
        }

        public byte[] GetData(string url, bool hasHost = false)
        {
            var ms = new MemoryStream();
            var dataStream = Send(url, "GET");
            if (dataStream != null)
            {
                dataStream.CopyTo(ms);

                return ms.ToArray();
            }
            return null;
        }

        public void PutJson(string url, string data, bool compress = true)
        {
            using Stream stream = Send(url, "PUT", data, compress);
        }

        public string GetJson(string url, bool compress = true)
        {
            using Stream stream = Send(url, "GET", null, compress);
            using MemoryStream ms = new();
            if (stream == null)
                return "";
            stream.CopyTo(ms);
            return SimpleZlib.Decompress(ms.ToArray(), null);
        }

        public string PostJson(string url, string data, bool compress = true)
        {
            using Stream stream = Send(url, "POST", data, compress);
            using MemoryStream ms = new();
            if (stream == null)
                return "";
            stream.CopyTo(ms);
            return SimpleZlib.Decompress(ms.ToArray(), null);
        }

        public async Task<string> PostJsonAsync(string url, string data, bool compress = true)
        {
            return await Task.Run(() => { return PostJson(url, data, compress); });
        }

        public Texture2D GetImage(string url, bool compress = true)
        {
            using Stream stream = Send(url, "GET", null, compress);
            using MemoryStream ms = new ();
            if (stream == null)
                return null;
            Texture2D texture = new(8, 8);

            stream.CopyTo(ms);
            texture.LoadImage(ms.ToArray());
            return texture;
        }

        public void Dispose()
        {
        }
    }
}
