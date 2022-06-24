using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFT;
using System.Reflection;
using UnityEngine;
using Comfort.Common;
using Newtonsoft.Json;
using SIT.Tarkov.Core;
using SIT.A.Tarkov.Core.Spawners.Grenades;

/***
 * Full Credit for this patch goes to SPT-Aki team. Specifically CWX!
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules. 
 * Paulov. Made changes to have better reflection, less hardcoding and use my own Requesting
 * Paulov. Have also added some nifty smoke grenades to show where it is!
 */

namespace SIT.Tarkov.Core.Raid.Aki
{
 
    public class AirdropBoxPatch : ModulePatch
    {
        public static Type AirDropLogicClassType;
        public AirdropBoxPatch()
        {
            AirDropLogicClassType = PatchConstants.EftTypes.Single(x
                => x.GetMethod("ParachuteFadeCoroutine", BindingFlags.Public | BindingFlags.Instance) != null);
            //Logger.LogInfo(GetType().Name + ":" + AirDropLogicClassType.Name);
        }

        public static int height = 0;
        protected override MethodBase GetTargetMethod()
        {
            //return typeof(AirdropLogic2Class).GetMethod("method_17", BindingFlags.NonPublic | BindingFlags.Instance);
            //foreach(var mm in AirDropLogicClassType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            //{
            //    Logger.LogInfo(mm);
            //}


            var method = AirDropLogicClassType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(x=>
                    x.GetParameters().Length >= 4
                    && x.GetParameters()[0].ParameterType == typeof(Vector3)
                    && x.GetParameters()[0].Name == "point"
                    && x.ReturnType == typeof(bool)
                    );

            Logger.LogInfo(GetType().Name + ":" + AirDropLogicClassType.Name + ":" + method.Name);
            return method;

        }

        [PatchPrefix]
        public static bool PatchPreFix(Vector3 point, ref float distance, RaycastHit raycastHit, LayerMask mask)
        {
            //Logger.LogInfo("AirdropBoxPatch:PatchPreFix");

            if (height == 0)
            {
                var url = "/singleplayer/airdrop/config";
                var json = new Request(PatchConstants.GetPHPSESSID(), PatchConstants.GetBackendUrl()).GetJson(url);

                var config = JsonConvert.DeserializeObject<AirdropConfig>(json);
                height = UnityEngine.Random.Range(config.airdropMinOpenHeight, config.airdropMaxOpenHeight);
            }

            distance = height;
            return true;
        }
    }

    public class AirdropPatch : ModulePatch
    {
        private static GameWorld gameWorld = null;
        private static bool points;

        public static int MaxDropCount { get; set; }

        public AirdropPatch(BepInEx.Configuration.ConfigFile config)
        {
            var airdropMaxDropCount = config.Bind("Airdrop", "MaxDropCount", 1);
            if (airdropMaxDropCount != null && airdropMaxDropCount.Value > 0)
            {
                MaxDropCount = airdropMaxDropCount.Value;
            }
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod("OnGameStarted", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        public static void PatchPostFix()
        {
            //Logger.LogInfo("AirdropPatch:PatchPostFix");

            gameWorld = Singleton<GameWorld>.Instance;
            points = LocationScene.GetAll<AirdropPoint>().Any();
            if (gameWorld == null)
            {
                Logger.LogInfo("AirdropPatch:PatchPostFix: gameWorld is null!");
            }
            if (!points)
            {
                Logger.LogInfo("AirdropPatch:PatchPostFix: No Airdrop Points were found!");
            }

            

            if (gameWorld != null && points)
            {
                gameWorld.GetOrAddComponent<AirdropComponent>();
            }
        }
    }

    public class AirdropComponent : MonoBehaviour
    {
        private SynchronizableObject plane;
        private SynchronizableObject[] planes;
        private SynchronizableObject box;
        private SynchronizableObject[] boxes;
        private bool planeEnabled;
        private bool boxEnabled;
        private int amountDropped;
        private int dropChance;
        private List<AirdropPoint> airdropPoints;
        private AirdropPoint randomAirdropPoint;
        private int boxObjId;
        private Vector3 boxPosition;
        private Vector3 planeStartPosition;
        private Vector3 planeStartRotation;
        private int planeObjId;
        private float planePositivePosition;
        private float planeNegativePosition;
        private float dropHeight;
        private float timer;
        private float timeToDrop;
        private bool doNotRun;
        private bool isRunning;
        private GameWorld gameWorld;
        private AirdropConfig config;
        private int airdropIndex = -1;

        public void Start()
        {
            PatchConstants.Logger.LogInfo("AirdropComponent:Start");
            gameWorld = Singleton<GameWorld>.Instance;
            planeEnabled = false;
            boxEnabled = false;
            amountDropped = 0;
            doNotRun = false;
            isRunning = false;
            boxObjId = 10;
            planePositivePosition = 3000f;
            planeNegativePosition = -3000f;

            planes = LocationScene.GetAll<SynchronizableObject>().Where(x => x.GetComponent<AirplaneSynchronizableObject>()).ToArray();
            PatchConstants.Logger.LogInfo($"AirdropComponent:Start:Number of Planes:{planes.Length}");
            boxes = LocationScene.GetAll<SynchronizableObject>().Where(x => x.GetComponent<AirdropSynchronizableObject>()).ToArray();
            PatchConstants.Logger.LogInfo($"AirdropComponent:Start:Number of Boxes:{boxes.Length}");
            airdropPoints = LocationScene.GetAll<AirdropPoint>().OrderBy(_ => Guid.NewGuid()).ToList();
            PatchConstants.Logger.LogInfo($"AirdropComponent:Start:Number of Points:{airdropPoints.Count}");

            config = GetConfigFromServer();
            dropChance = ChanceToSpawn();
            dropHeight = UnityEngine.Random.Range(config.planeMinFlyHeight, config.planeMaxFlyHeight);
            timeToDrop = UnityEngine.Random.Range(config.airdropMinStartTimeSeconds, config.airdropMaxStartTimeSeconds);
            
            GetNextAirdropSetup();
        }

        private bool GetNextAirdropSetup()
        {
            airdropIndex++;
            if (
                airdropIndex < planes.Length && airdropIndex < boxes.Length && airdropIndex < airdropPoints.Count)
            {
                plane = planes[airdropIndex];
                box = boxes[airdropIndex];
                randomAirdropPoint = airdropPoints[airdropIndex];
                planeObjId = airdropIndex + 1 > 4 ? 0 : airdropIndex + 1;
            }

            return false;
        }

        public void FixedUpdate() // https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html
        {
            if (gameWorld == null)
            {
                return;
            }

            timer += 0.02f;

            //if (timer >= timeToDrop && !planeEnabled && amountDropped != 1 && !doNotRun)
            if (timer >= timeToDrop && !planeEnabled && amountDropped < AirdropPatch.MaxDropCount && !doNotRun)
            {
                ScriptStart();
            }

            if (timer >= timeToDrop && planeEnabled && !doNotRun)
            {
                plane.transform.Translate(Vector3.forward, Space.Self);

                switch (planeObjId)
                {
                    case 1:
                        if (plane.transform.position.z >= planePositivePosition && planeEnabled)
                        {
                            DisablePlane();
                        }

                        if (plane.transform.position.z >= randomAirdropPoint.transform.position.z && !boxEnabled)
                        {
                            InitDrop();
                        }
                        break;
                    case 2:
                        if (plane.transform.position.x >= planePositivePosition && planeEnabled)
                        {
                            DisablePlane();
                        }

                        if (plane.transform.position.x >= randomAirdropPoint.transform.position.x && !boxEnabled)
                        {
                            InitDrop();
                        }
                        break;
                    case 3:
                        if (plane.transform.position.z <= planeNegativePosition && planeEnabled)
                        {
                            DisablePlane();
                        }
                        if (plane.transform.position.z <= randomAirdropPoint.transform.position.z && !boxEnabled)
                        {
                            InitDrop();
                        }
                        break;
                    case 4:
                        if (plane.transform.position.x <= planeNegativePosition && planeEnabled)
                        {
                            DisablePlane();
                        }
                        if (plane.transform.position.x <= randomAirdropPoint.transform.position.x && !boxEnabled)
                        {
                            InitDrop();
                        }
                        break;
                }
            }
        }

        private int ChanceToSpawn()
        {
            var location = gameWorld.RegisteredPlayers[0].Location;
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

        private AirdropConfig GetConfigFromServer()
        {
            var url = "/singleplayer/airdrop/config";
            var json = new Request(PatchConstants.GetPHPSESSID(), PatchConstants.GetBackendUrl()).GetJson(url);

            var c = JsonConvert.DeserializeObject<AirdropConfig>(json);
            AirdropPatch.MaxDropCount =  c.maximumAirdopsPerRaid;
            PatchConstants.Logger.LogInfo($"AirdropComponent:MaxDropCount:{AirdropPatch.MaxDropCount}");

            return c;
        }

        public bool ShouldAirdropOccur()
        {
            return UnityEngine.Random.Range(1, 99) <= dropChance 
                && amountDropped <= AirdropPatch.MaxDropCount;
        }

        public void DoNotRun() // currently not doing anything, could be used later for multiple drops
        {
            doNotRun = amountDropped >= AirdropPatch.MaxDropCount;
        }

        public void ScriptStart()
        {
            PatchConstants.Logger.LogInfo("AirdropComponent:ScriptStart");

            if (!ShouldAirdropOccur())
            {
                DoNotRun();
                return;
            }

            if (box != null)
            {
                boxPosition = randomAirdropPoint.transform.position;
                boxPosition.y = dropHeight;
            }

            if (plane != null)
            {
                PlanePositionGen();
            }
        }

        public void InitPlane()
        {
            PatchConstants.Logger.LogInfo("AirdropComponent:InitPlane");

            planeEnabled = true;
            plane.TakeFromPool();
            plane.Init(planeObjId, planeStartPosition, planeStartRotation);
            plane.transform.LookAt(boxPosition);
            plane.ManualUpdate(0);

            var sound = plane.GetComponentInChildren<AudioSource>();
            sound.volume = (float)Math.Min(1.0, Math.Max(0.75, config.planeVolume));
            sound.spatialBlend = 0.9875f;
            sound.spatialize = true;
            sound.dopplerLevel = 0.7f;
            sound.pitch = 0.6f;
            sound.minDistance = 50.0f;
            sound.maxDistance = 900.0f;
            sound.Play();
        }

        public void InitDrop()
        {
            PatchConstants.Logger.LogInfo("AirdropComponent:InitDrop");

            object[] objToPass = new object[1];
            objToPass[0] = SynchronizableObjectType.AirDrop;
            //PatchConstants.GetFieldOrPropertyFromInstance<object>(gameWorld, "SynchronizableObjectLogicProcessor").GetType()
            gameWorld.SynchronizableObjectLogicProcessor.GetType()
                .GetMethod("method_9", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(gameWorld.SynchronizableObjectLogicProcessor, objToPass);

            var airdropLogic = Activator.CreateInstance(AirdropBoxPatch.AirDropLogicClassType);
            boxEnabled = true;
            box.GetType().GetMethod("SetLogic").Invoke(box, new object[] { airdropLogic });
            box.ReturnToPool();
            box.TakeFromPool();
            box.Init(boxObjId, boxPosition, Vector3.zero);
            //var sgp = gameWorld.GetOrAddComponent<MultipleSmokeGrenadeSpawner>();
        }

        public void PlanePositionGen()
        {
            switch (planeObjId)
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

            InitPlane();
        }

        public void DisablePlane()
        {
            PatchConstants.Logger.LogInfo("AirdropComponent:DisablePlane");

            planeEnabled = false;
            amountDropped++;
            plane.ReturnToPool();
            timeToDrop = timer + UnityEngine.Random.Range(config.airdropMinStartTimeSeconds, config.airdropMaxStartTimeSeconds);
            boxEnabled = false;
            GetNextAirdropSetup();
        }
    }

    public class AirdropChancePercent
    {
        public int bigmap { get; set; } = 100;
        public int woods { get; set; } = 100;
        public int lighthouse { get; set; } = 100;
        public int shoreline { get; set; } = 100;
        public int interchange { get; set; } = 100;
        public int reserve { get; set; } = 100;
    }

    public class AirdropConfig
    {
        public AirdropChancePercent airdropChancePercent { get; set; } = new AirdropChancePercent();
        public int airdropMinStartTimeSeconds { get; set; } = 10;
        public int airdropMaxStartTimeSeconds { get; set; } = 30;
        public int airdropMinOpenHeight { get; set; } = 10;
        public int airdropMaxOpenHeight { get; set; } = 20;
        public int planeMinFlyHeight { get; set; } = 50;
        public int planeMaxFlyHeight { get; set; } = 100;
        public float planeVolume { get; set; } = 0.7f;
        public int maximumAirdopsPerRaid { get; set; } = 1;
    }
}
