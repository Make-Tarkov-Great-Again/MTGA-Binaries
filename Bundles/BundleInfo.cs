namespace SIT.Tarkov.Core
{
    public class BundleInfo
    {
        public string Key { get; }
        public string Path { get; set; }
        public string[] DependencyKeys { get; }

        public BundleInfo(string key, string path, string[] dependencyKeys)
        {
            Key = key;
            Path = path;
            DependencyKeys = dependencyKeys;
        }
    }

    public class StreamingAssetBundle
    {
        public string FileName { get; }
        public uint Crc { get; set; }
        public string[] Dependencies { get; }
    }
}
