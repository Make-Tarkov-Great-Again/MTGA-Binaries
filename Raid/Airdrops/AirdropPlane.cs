using System.Collections;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.Airdrop;
using EFT.SynchronizableObjects;
using UnityEngine;

/***
 * Full Credit for this patch goes to SPT-AKI team. Specifically CWX & SamSwat!
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules. 
*/
namespace Aki.Custom.Airdrops
{
    public class AirdropPlane : MonoBehaviour
    {
        private const string PLANE_PATH = "assets/content/location_objects/lootable/prefab/il76md-90.prefab";
        private const float PLANE_POSITIVE_POS = 3000f;
        private const float PLANE_NEGATIVE_POS = -3000f;

        private AirplaneSynchronizableObject airplaneSync;
        private float distanceToDrop;
        private float flaresCooldown;
        private bool flaresDeployed;
        private bool headingChanged;

        public static async Task<AirdropPlane> Init(Vector3 airdropPoint, int dropHeight, float planeVolume)
        {
            var instance = (await LoadPlane()).AddComponent<AirdropPlane>();

            instance.airplaneSync = instance.GetComponent<AirplaneSynchronizableObject>();
            instance.airplaneSync.SetLogic(new AirplaneLogicClass());

            instance.SetPosition(dropHeight, airdropPoint);
            instance.SetAudio(planeVolume);
            instance.gameObject.SetActive(false);
            return instance;
        }

        private static async Task<GameObject> LoadPlane()
        {
            var easyAssets = Singleton<PoolManager>.Instance.EasyAssets;
            await easyAssets.Retain(PLANE_PATH, null, null).LoadingJob;
            var plane = Instantiate(easyAssets.GetAsset<GameObject>(PLANE_PATH));
            return plane;
        }

        private void SetAudio(float planeVolume)
        {
            var airplaneAudio = gameObject.AddComponent<AudioSource>();
            airplaneAudio.clip = airplaneSync.soundClip.Clip;

            airplaneAudio.dopplerLevel = 1f;
            airplaneAudio.outputAudioMixerGroup = Singleton<BetterAudio>.Instance.VeryStandartMixerGroup;
            airplaneAudio.loop = true;
            airplaneAudio.maxDistance = 2000;
            airplaneAudio.minDistance = 1;
            airplaneAudio.pitch = 0.5f;
            airplaneAudio.priority = 128;
            airplaneAudio.reverbZoneMix = 1;
            airplaneAudio.rolloffMode = AudioRolloffMode.Custom;
            airplaneAudio.spatialBlend = 1;
            airplaneAudio.spread = 60;
            airplaneAudio.volume = planeVolume;

            airplaneAudio.Play();
        }

        private void SetPosition(int dropHeight, Vector3 airdropPoint)
        {
            var startNumber = Random.Range(1, 4);
            var planeStartPosition = Vector3.zero;
            var planeStartRotation = Vector3.zero;

            switch (startNumber)
            {
                case 1:
                    planeStartPosition = new Vector3(0, dropHeight, PLANE_NEGATIVE_POS);
                    planeStartRotation = Vector3.zero;
                    break;
                case 2:
                    planeStartPosition = new Vector3(PLANE_NEGATIVE_POS, dropHeight, 0);
                    planeStartRotation = new Vector3(0, 90, 0);
                    break;
                case 3:
                    planeStartPosition = new Vector3(0, dropHeight, PLANE_POSITIVE_POS);
                    planeStartRotation = new Vector3(0, 180, 0);
                    break;
                case 4:
                    planeStartPosition = new Vector3(PLANE_POSITIVE_POS, dropHeight, 0);
                    planeStartRotation = new Vector3(0, 270, 0);
                    break;
            }

            transform.SetPositionAndRotation(planeStartPosition, Quaternion.Euler(planeStartRotation));
            transform.LookAt(new Vector3(airdropPoint.x, dropHeight, airdropPoint.z));
        }

        public void ManualUpdate(float distance)
        {
            transform.Translate(Vector3.forward);
            distanceToDrop = distance;
            UpdateFlaresLogic();

            if (distance - 200f > 0f || headingChanged) return;

            StartCoroutine(ChangeHeading());
            headingChanged = true;
        }

        private void UpdateFlaresLogic()
        {
            if (flaresDeployed) return;

            if (distanceToDrop > 0f && flaresCooldown <= Time.unscaledTime)
            {
                flaresCooldown = Time.unscaledTime + 4f;
                StartCoroutine(DeployFlares(Random.Range(0.2f, 0.4f)));
            }

            if (distanceToDrop > 0f) return;

            flaresDeployed = true;
            StartCoroutine(DeployFlares(5f));
        }

        private IEnumerator DeployFlares(float emissionTime)
        {
            var projectile = Instantiate(airplaneSync.infraredCountermeasureParticles, transform);
            projectile.transform.localPosition = new Vector3(0f, -5f, 0f);
            var flares = projectile.GetComponentsInChildren<ParticleSystem>();
            var endTime = Time.unscaledTime + emissionTime;
            Singleton<GameWorld>.Instance.SynchronizableObjectLogicProcessor.AirdropManager.AddProjectile(projectile,
                endTime + flares[0].main.duration + flares[0].main.startLifetime.Evaluate(1f));

            while (endTime > Time.unscaledTime)
                yield return null;

            projectile.transform.parent = null;
            foreach (var particleSystem in flares)
                particleSystem.Stop();
        }

        private IEnumerator ChangeHeading()
        {
            var startingRotation = transform.eulerAngles;
            var middleRotation = startingRotation + new Vector3(0f, 40f, -200f);
            var endRotation = middleRotation + new Vector3(0f, 40f, 200f);

            for (float i = 0; i < 1; i += Time.deltaTime / 25f)
            {
                var finalRotation = Vector3.Lerp(middleRotation, endRotation, EasingSmoothSquared(i));
                transform.eulerAngles = Vector3.Lerp(startingRotation, finalRotation, EasingSmoothSquared(i));
                yield return null;
            }
        }

        private float EasingSmoothSquared(float x)
        {
            return x < 0.5 ? x * x * 2 : (1 - (1 - x) * (1 - x) * 2);
        }
    }
}
