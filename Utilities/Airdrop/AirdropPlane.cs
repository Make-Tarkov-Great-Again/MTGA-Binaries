using MTGA.Utilities.Airdrops.Utils;
using EFT.Airdrop;
using EFT.SynchronizableObjects;
using UnityEngine;
/***
 * Full Credit for this patch goes to SPT-AKI team. Specifically CWX!
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules. 
*/
namespace MTGA.Utilities.Airdrops
{
    public class AirdropPlane : MonoBehaviour
    {
        private readonly string planePath = "EscapeFromTarkov_Data/StreamingAssets/Windows/assets/content/location_objects/lootable/prefab/il76md-90.prefab";

        public GameObject planeObject;

        private float planePositivePosition = 3000f;
        private float planeNegativePosition = -3000f;
        private Vector3 planeStartPosition;
        private Vector3 planeStartRotation;
        public bool planeEnabled = false;
        public GameObject counterMeasures;

        public async void Init(AirdropPoint airdropPoint, int dropHeight, float planeVolume)
        {
            SetPosition(dropHeight);

            planeObject = Instantiate(await BundlesUtil.LoadAssetAsync<GameObject>(planePath), planeStartPosition, Quaternion.Euler(planeStartRotation));

            var airplaneSync = planeObject.GetComponent<AirplaneSynchronizableObject>();
            airplaneSync.SetLogic(new AirplaneLogicClass());

            counterMeasures = airplaneSync.infraredCountermeasureParticles;

            SetAudio(planeVolume, airplaneSync);
            SetLookTowardsDrop(airdropPoint, dropHeight);
            planeEnabled = true;
        }

        private void SetLookTowardsDrop(AirdropPoint airdropPoint, int dropHeight)
        {
            planeObject.transform.LookAt(new Vector3(airdropPoint.transform.position.x, dropHeight, airdropPoint.transform.position.z));
        }

        private void SetAudio(float planeVolume, AirplaneSynchronizableObject airplaneSync)
        {

            planeObject.AddComponent<AudioSource>();
            var airplaneAudio = planeObject.GetComponent<AudioSource>();
            airplaneAudio.clip = airplaneSync.soundClip.Clip;


            airplaneAudio.dopplerLevel = 1f;
            airplaneAudio.loop = true;
            airplaneAudio.maxDistance = 950;
            airplaneAudio.minDistance = 1;
            airplaneAudio.pitch = 0.6f;
            airplaneAudio.priority = 128;
            airplaneAudio.reverbZoneMix = 1;
            airplaneAudio.rolloffMode = AudioRolloffMode.Custom;
            airplaneAudio.spatialBlend = 1;
            airplaneAudio.spread = 60;
            airplaneAudio.volume = planeVolume;

            airplaneAudio.Play();
        }

        private void SetPosition(int dropHeight)
        {
            var startNumber = Random.Range(1, 4);
            switch (startNumber)
            {
                case 1:
                    planeStartPosition = new Vector3(0, dropHeight, planeNegativePosition);
                    planeStartRotation = new Vector3(0, 0, 0);
                    break;
                case 2:
                    planeStartPosition = new Vector3(planeNegativePosition, dropHeight, 0);
                    planeStartRotation = new Vector3(0, 90, 0);
                    break;
                case 3:
                    planeStartPosition = new Vector3(0, dropHeight, planePositivePosition);
                    planeStartRotation = new Vector3(0, 180, 0);
                    break;
                case 4:
                    planeStartPosition = new Vector3(planePositivePosition, dropHeight, 0);
                    planeStartRotation = new Vector3(0, 270, 0);
                    break;
            }
        }

        private void OnDestroy()
        {
            BundlesUtil.UnloadBundle("il76md-90.prefab", true);
        }
    }
}
