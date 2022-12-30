using EFT.Airdrop;
using System.Collections.Generic;

namespace Aki.Custom.Airdrops.Models
{
    public class AirdropParametersModel
    {
        public AirdropConfigModel config;

        public float distanceTraveled;
        public float distanceToTravel;
        public float distanceToDrop;
        public float timer;
        public bool planeSpawned;
        public bool boxSpawned;
        public float boxFallSpeed;

        public int dropHeight;
        public int timeToStart;
        public int? dropChance;

        public List<AirdropPoint> airdropPoints;
        public AirdropPoint randomAirdropPoint;
    }
}
