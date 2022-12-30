using Aki.Custom.Airdrops.Models;
using Aki.Custom.Airdrops.Utils;
using Comfort.Common;
using EFT;
using UnityEngine;
/***
 * Full Credit for this patch goes to SPT-AKI team. Specifically CWX & SamSwat!
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules. 
*/
namespace Aki.Custom.Airdrops
{
    public class AirdropsManager : MonoBehaviour
    {
        private GameWorld gameWorld;

        private AirdropPlane airdropPlane;
        private AirdropBox airdropBox;
        private ItemFactoryUtil factory;

        public bool isFlareDrop;
        private AirdropParametersModel airdropParameters;

        public async void Start()
        {
            if (gameWorld == null)
            {
                gameWorld = Singleton<GameWorld>.Instance;
            }

            airdropParameters ??= AirdropUtil.InitAirdropParams(gameWorld, isFlareDrop);

            if (!AirdropUtil.ShouldAirdropOccur(airdropParameters.dropChance.Value)
                || !AirdropUtil.AirdropHasDropPoint(airdropParameters.airdropPoints))
            {
                Destroy(this);
                return;
            }

            try
            {
                airdropPlane = await AirdropPlane.Init(airdropParameters.randomAirdropPoint.transform.position, airdropParameters.dropHeight,
                    airdropParameters.config.planeVolume);
                airdropBox = await AirdropBox.Init(airdropParameters.boxFallSpeed);
                factory = new ItemFactoryUtil();
            }
            catch
            {
                Debug.LogError($"Unable to create plane or crate, airdrop won't occur");
                Destroy(this);
                throw;
            }

            SetDistanceToDrop();
        }

        public void FixedUpdate()
        {
            if (gameWorld == null) return;

            airdropParameters.timer += 0.02f;

            if (airdropParameters.timer >= airdropParameters.timeToStart && !airdropParameters.planeSpawned)
            {
                StartPlane();
            }

            if (airdropParameters.distanceTraveled >= airdropParameters.distanceToDrop && !airdropParameters.boxSpawned)
            {
                StartBox();
                BuildLootContainer();
            }

            if (!airdropParameters.planeSpawned) return;

            if (airdropParameters.distanceTraveled < airdropParameters.distanceToTravel)
            {
                airdropParameters.distanceTraveled++;
                var distanceToDrop = airdropParameters.distanceToDrop - airdropParameters.distanceTraveled;
                airdropPlane.ManualUpdate(distanceToDrop);
            }
            else
            {
                Destroy(airdropPlane.gameObject);
                Destroy(this);
            }
        }

        private void StartPlane()
        {
            airdropPlane.gameObject.SetActive(true);
            airdropParameters.planeSpawned = true;
        }

        private void StartBox()
        {
            airdropParameters.boxSpawned = true;
            var pointPos = airdropParameters.randomAirdropPoint.transform.position;
            var dropPos = new Vector3(pointPos.x, airdropParameters.dropHeight, pointPos.z);
            airdropBox.gameObject.SetActive(true);
            airdropBox.StartCoroutine(airdropBox.DropCrate(dropPos));
        }

        private void BuildLootContainer()
        {
            factory.BuildContainer(airdropBox.container);
            factory.AddLoot(airdropBox.container);
        }

        private void SetDistanceToDrop()
        {
            var position = airdropParameters.randomAirdropPoint.transform.position;

            airdropParameters.distanceToDrop = Vector3.Distance(
                new Vector3(position.x, airdropParameters.dropHeight, position.z),
                airdropPlane.transform.position);
        }
    }
}
