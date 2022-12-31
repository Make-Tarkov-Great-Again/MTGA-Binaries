using EFT;
using EFT.Airdrop;
using MTGA.Utilities.Airdrops.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/***
 * Full Credit for this patch goes to SPT-AKI team. Specifically CWX!
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules. 
*/
namespace MTGA.Utilities.Airdrops.Utils
{
    public static class AirdropUtil
    {
        public static AirdropConfigModel GetConfigFromServer()
        {
            string json = new MTGA.Core.Request().GetJson("/singleplayer/airdrop/config");
            return JsonConvert.DeserializeObject<AirdropConfigModel>(json);
        }

        public static int ChanceToSpawn(GameWorld gameWorld, AirdropConfigModel config, bool isFlare)
        {
            if (isFlare)
            {
                return 100;
            }

            string location = gameWorld.RegisteredPlayers[0].Location;

            int result = 25;
            switch (location.ToLower())
            {
                case "bigmap":
                    {
                        result = config.airdropChancePercent.bigmap;
                        break;
                    }
                case "interchange":
                    {
                        result = config.airdropChancePercent.interchange;
                        break;
                    }
                case "rezervbase":
                    {
                        result = config.airdropChancePercent.reserve;
                        break;
                    }
                case "shoreline":
                    {
                        result = config.airdropChancePercent.shoreline;
                        break;
                    }
                case "woods":
                    {
                        result = config.airdropChancePercent.woods;
                        break;
                    }
                case "lighthouse":
                    {
                        result = config.airdropChancePercent.lighthouse;
                        break;
                    }
            }

            return result;
        }

        public static bool ShouldAirdropOccur(int dropChance)
        {
            return UnityEngine.Random.Range(1, 99) <= dropChance;
        }

        public static AirdropParametersModel InitAirdropParams(GameWorld gameWorld, bool isFlare)
        {
            AirdropConfigModel serverConfig = GetConfigFromServer();
            List<AirdropPoint> allAirdropPoints = LocationScene.GetAll<AirdropPoint>().ToList();
            Vector3 playerVector = gameWorld.RegisteredPlayers.Find(p => p.IsYourPlayer).Position;
            List<AirdropPoint> flareAirdropPoints = new List<AirdropPoint>();

            if (isFlare && allAirdropPoints.Count > 0)
            {
                foreach (AirdropPoint point in allAirdropPoints)
                {
                    if (Vector3.Distance(playerVector, point.transform.position) <= 100f)
                    {
                        flareAirdropPoints.Add(point);
                    }
                }
            }

            if (flareAirdropPoints.Count == 0 && isFlare)
            {
                Debug.LogError($"[AIRDROPS]: Airdrop called in by flare, Unable to find an airdropPoint within 100m, defaulting to normal drop");
                flareAirdropPoints.Add(allAirdropPoints.OrderBy(_ => Guid.NewGuid()).FirstOrDefault());
            }


            return new AirdropParametersModel()
            {
                config = serverConfig,
                dropChance = ChanceToSpawn(gameWorld, serverConfig, isFlare),
                parachuteStarted = false,
                parachuteStartedTimer = 0,
                parachutePaused = false,
                containerBuilt = false,
                airdropComplete = false,
                parachuteComplete = false,

                distanceTraveled = 0f,
                distanceToTravel = 6000f, // once picked drop point, get distance between plane and drop
                distanceToDrop = 10000f,
                timer = 0,
                planeSpawned = false,
                boxSpawned = false,
                boxLanded = false,
                boxFallSpeedMulti = 0.10f,

                dropHeight = UnityEngine.Random.Range(serverConfig.planeMinFlyHeight, serverConfig.planeMaxFlyHeight),
                timeToStart = UnityEngine.Random.Range(1, 30),

                airdropPoints = allAirdropPoints,
                randomAirdropPoint = isFlare && allAirdropPoints.Count > 0 ? flareAirdropPoints.OrderBy(_ => Guid.NewGuid()).FirstOrDefault() : allAirdropPoints.OrderBy(_ => Guid.NewGuid()).FirstOrDefault()
            };
        }

        public static bool AirdropHasDropPoint(List<AirdropPoint> airdropPoints)
        {
            return airdropPoints.Count > 0;
        }
    }
}
