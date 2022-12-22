using Aki.Custom.Airdrops.Models;
using Aki.Custom.Airdrops.Utils;
using Comfort.Common;
using EFT;
using MTGA.Core;
using UnityEngine;
/***
 * Full Credit for this patch goes to SPT-AKI team. Specifically CWX!
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules. 
*/
namespace Aki.Custom.Airdrops
{
    public class AirdropsManager : MonoBehaviour
    {
        GameWorld gameWorld;

        AirdropPlane airdropPlane;
        AirdropBox airdropBox;
        ItemFactoryUtil factory;

        public bool isFlareDrop = false;
        private AirdropParametersModel airdropParameters;

        public void Start()
        {
            PatchConstants.Logger.LogInfo("AirdropsManager.Start");
            if (gameWorld == null)
            {
                PatchConstants.Logger.LogInfo("AirdropsManager.Start.Get GameWorld");
                gameWorld = Singleton<GameWorld>.Instance;
            }

            if (airdropParameters == null)
            {
                PatchConstants.Logger.LogInfo("AirdropsManager.Start.Get Airdrop Params");
                airdropParameters = AirdropUtil.InitAirdropParams(gameWorld, isFlareDrop);
            }

            if (!AirdropUtil.ShouldAirdropOccur(airdropParameters.dropChance.Value)
                || !AirdropUtil.AirdropHasDropPoint(airdropParameters.airdropPoints))
            {
                PatchConstants.Logger.LogWarning("AirdropsManager.Start.Airdrop - do not run?");

                airdropParameters.doNotRun = true;
                return;

            }

            //airdropPlane = new AirdropPlane();
            airdropPlane = this.GetOrAddComponent<AirdropPlane>();
            DontDestroyOnLoad(airdropPlane);
            airdropBox = new AirdropBox();
            factory = new ItemFactoryUtil();
            PatchConstants.Logger.LogInfo("AirdropsManager.Start.Complete");

        }

        public void FixedUpdate()
        {
            if (airdropParameters == null)
                return;

            if (gameWorld == null)
            {
                PatchConstants.Logger.LogInfo("AirdropsManager.FixedUpdate.No gameWorld");
                return;
            }

            if (airdropParameters.doNotRun || airdropParameters.airdropComplete)
            {
                return;
            }

            airdropParameters.timer += 0.02f;

            if (airdropParameters.timer >= airdropParameters.timeToStart && !airdropParameters.planeSpawned)
            {
                StartPlane();
            }

            if (airdropPlane == null)
            {
                return;
            }

            if (airdropParameters.planeSpawned && airdropPlane.planeEnabled)
            {
                if (airdropParameters.distanceTraveled < airdropParameters.distanceToTravel)
                {
                    airdropPlane.planeObject.transform.Translate(Vector3.forward, Space.Self);
                    airdropPlane.counterMeasures.transform.position = airdropPlane.planeObject.transform.position;
                    airdropParameters.distanceTraveled++;
                }

                if (airdropParameters.distanceTraveled == airdropParameters.distanceToTravel)
                {
                    airdropPlane.planeEnabled = false;
                    airdropPlane.planeObject.SetActive(false);
                }
            }

            if (airdropParameters.distanceTraveled > 40 && airdropParameters.planeSpawned
                && airdropPlane.planeEnabled && airdropParameters.distanceToDrop == 10000f)
            {
                GetDistanceToDrop();
            }

            if (airdropParameters.distanceTraveled >= airdropParameters.distanceToDrop && !airdropParameters.boxSpawned)
            {
                StartBox();
                DeployFlares();
            }

            if (airdropParameters.boxSpawned && airdropBox.boxEnabled && !airdropParameters.boxLanded)
            {
                RaycastBoxDistance();
            }

            if (!airdropParameters.parachuteStarted && airdropParameters.boxSpawned && airdropBox.boxEnabled)
            {
                airdropBox.parachute.SetActive(true);
                airdropParameters.parachuteStarted = true;
                airdropParameters.parachuteStartedTimer = airdropParameters.timer;
            }

            if (airdropParameters.timer > airdropParameters.parachuteStartedTimer + 0.5f && airdropParameters.boxSpawned && !airdropParameters.parachutePaused
                && airdropBox.boxEnabled && !airdropParameters.airdropComplete && !airdropParameters.parachuteComplete)
            {
                airdropBox.paraAnimation.enabled = false;
                airdropParameters.parachutePaused = true;
            }

            if (airdropParameters.timer > airdropParameters.parachuteStartedTimer + 2f && airdropParameters.boxSpawned
                && airdropParameters.parachutePaused && !airdropParameters.containerBuilt)
            {
                BuildLootContainer();
            }

            if (airdropParameters.boxLanded && airdropParameters.parachutePaused && !airdropParameters.airdropComplete && !airdropParameters.parachuteComplete)
            {
                EndParachute();
            }

            if (airdropParameters.boxLanded && !airdropPlane.planeEnabled)
            {
                airdropParameters.airdropComplete = true;
            }
        }

        private void StartPlane()
        {
            airdropParameters.planeSpawned = true;
            airdropPlane.Init(airdropParameters.randomAirdropPoint, airdropParameters.dropHeight, airdropParameters.config.planeVolume);
        }

        private void StartBox()
        {
            airdropBox.Init(airdropParameters.randomAirdropPoint, airdropParameters.dropHeight);
            airdropParameters.boxSpawned = true;
        }
        private void DeployFlares()
        {
            airdropPlane.counterMeasures.SetActive(true);
        }

        private void EndParachute()
        {
            airdropBox.paraAnimation.enabled = true;
            airdropParameters.parachutePaused = false;
            airdropParameters.parachuteComplete = true;
        }

        private void BuildLootContainer()
        {
            factory.BuildContainer(airdropBox.container);
            airdropParameters.containerBuilt = true;
            factory.AddLoot(airdropBox.container);
        }

        private void GetDistanceToDrop()
        {
            airdropParameters.distanceToDrop = Vector3.Distance(
                new Vector3(airdropParameters.randomAirdropPoint.transform.position.x,
                airdropParameters.dropHeight,
                airdropParameters.randomAirdropPoint.transform.position.z),
                airdropPlane.planeObject.transform.position);
        }

        private void RaycastBoxDistance()
        {
            Ray ray = new Ray(airdropBox.boxObject.transform.position, Vector3.down);

            var raycast = Physics.Raycast(ray, out RaycastHit hitinfo, 600, LayerMaskController.TerrainLowPoly);

            if (raycast)
            {
                if (hitinfo.distance > 0.1f)
                {
                    airdropBox.boxObject.transform.Translate(new Vector3(0, -1 * airdropParameters.boxFallSpeedMulti, 0), Space.Self);
                }
                else
                {
                    airdropParameters.boxLanded = true;
                }
            }
            else if (!raycast && airdropParameters.boxSpawned)
            {
                airdropParameters.boxLanded = true;
                EndParachute();
            }
        }
    }
}
