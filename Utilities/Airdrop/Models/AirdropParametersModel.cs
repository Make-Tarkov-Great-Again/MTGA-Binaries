using EFT.Airdrop;
using System.Collections.Generic;

namespace MTGA.Utilities.Airdrops.Models
{
    public class AirdropParametersModel
    {
        public AirdropConfigModel config;

        public bool parachuteStarted;
        public float parachuteStartedTimer;
        public bool parachutePaused;
        public bool containerBuilt;
        public bool airdropComplete;
        public bool parachuteComplete;
        public float distanceTraveled;
        public float distanceToTravel;
        public float distanceToDrop;
        public float timer;
        public bool planeSpawned;
        public bool boxSpawned;
        public bool boxLanded;
        public float boxFallSpeedMulti;

        public int dropHeight;
        public int timeToStart { get; set; }
        public int? dropChance;

        public bool doNotRun;
        public List<AirdropPoint> airdropPoints;
        public AirdropPoint randomAirdropPoint;

        /// <summary>
        /// Increment the timer by a value
        /// Default is 0.02f
        /// </summary>
        /// <param name="amount">value to increment timer by</param>
        public void IncrementTimer(float amount = 0.02f)
        {
            timer += amount;
        }
    }
}
