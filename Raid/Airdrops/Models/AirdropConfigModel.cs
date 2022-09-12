/***
 * Full Credit for this patch goes to SPT-Aki team. Specifically CWX!
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules. 
*/
namespace Aki.Custom.Airdrops.Models
{
    public class AirdropConfigModel
    {
        public AirdropChancePercent airdropChancePercent { get; set; }
        public int airdropMinStartTimeSeconds { get; set; }
        public int airdropMaxStartTimeSeconds { get; set; }
        public int planeMinFlyHeight { get; set; }
        public int planeMaxFlyHeight { get; set; }
        public float planeVolume { get; set; }
    }

    public class AirdropChancePercent
    {
        public int bigmap { get; set; }
        public int woods { get; set; }
        public int lighthouse { get; set; }
        public int shoreline { get; set; }
        public int interchange { get; set; }
        public int reserve { get; set; }
    }
}