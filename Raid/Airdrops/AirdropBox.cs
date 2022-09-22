using Aki.Custom.Airdrops.Utils;
using EFT.Airdrop;
using EFT.Interactive;
using EFT.SynchronizableObjects;
using UnityEngine;

/***
 * Full Credit for this patch goes to SPT-AKI team. Specifically CWX!
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules. 
*/
namespace Aki.Custom.Airdrops
{
    public class AirdropBox : MonoBehaviour
    {
        private readonly string cratePath = "EscapeFromTarkov_Data/StreamingAssets/Windows/assets/content/location_objects/lootable/prefab/scontainer_crate.bundle";
        public GameObject boxObject;
        public AirdropSynchronizableObject boxLogic;
        public LootableContainer container;
        public GameObject parachute;
        public Animator paraAnimation;

        public bool boxEnabled = false;

        public async void Init(AirdropPoint airdropPoint, int dropHeight)
        {
            boxObject = Instantiate(await BundlesUtil.LoadAssetAsync<GameObject>(cratePath), new Vector3(airdropPoint.transform.position.x, dropHeight, airdropPoint.transform.position.z), airdropPoint.transform.rotation);

            boxLogic = boxObject.GetComponent<AirdropSynchronizableObject>();
            container = boxLogic.GetComponentInChildren<LootableContainer>().gameObject.GetComponentInChildren<LootableContainer>();

            boxLogic.SetLogic(new AirdropLogicClass());
            boxLogic.Init(1, new Vector3(airdropPoint.transform.position.x, dropHeight, airdropPoint.transform.position.z), Vector3.zero);

            parachute = boxObject.transform.Find("parachute").gameObject;
            paraAnimation = parachute.GetComponent<Animator>();

            boxEnabled = true;
        }

        private void OnDestroy()
        {
            BundlesUtil.UnloadBundle("scontainer_crate.bundle", true);
        }
    }
}
