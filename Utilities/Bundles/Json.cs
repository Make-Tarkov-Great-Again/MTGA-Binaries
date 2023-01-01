using Newtonsoft.Json;

namespace MTGA.Utilities.Bundles
{
    /// <summary>
    /// Simple Json Serializing and deserializing but shorter xD
    /// </summary>
    public static class Json
    {
        public static string Serialize<T>(T data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
