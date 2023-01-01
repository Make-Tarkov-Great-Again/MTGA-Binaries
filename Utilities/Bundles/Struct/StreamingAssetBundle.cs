namespace MTGA.Utilities.Bundles.Struct
{
    public class StreamingAssetBundle
    {
        public string FileName { get; }
        public uint Crc { get; set; }
        public string[] Dependencies { get; }
    }
}
