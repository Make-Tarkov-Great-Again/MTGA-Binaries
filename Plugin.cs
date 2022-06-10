using BepInEx;
using BundleLoader.Patches;
using SIT.A.Tarkov.Core.SP;
using SIT.Tarkov.Core;
using SIT.Tarkov.Core.Bundles;
using SIT.Tarkov.Core.Health;
using SIT.Tarkov.Core.SP;
using SIT.Tarkov.Core.SP.Raid;
using SIT.Tarkov.Core.SP.ScavMode;

namespace SIT.A.Tarkov.Core
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            PatchConstants.GetBackendUrl();

            new ConsistencySinglePatch().Enable();
            new ConsistencyMultiPatch().Enable();
            new BattlEyePatch().Enable();

            //new FileChecker.FileCheckerMainApplicationPatch().Enable();

            new SslCertificatePatch().Enable();
            new UnityWebRequestPatch().Enable();
            new WebSocketPatch().Enable();


            BundleSetup.Init();
            BundleManager.GetBundles();
            //new EasyAssets().Enable();
            //new EasyBundle().Enable();
            new EasyAssetsPatch().Enable();
            new EasyBundlePatch().Enable();

            //new LoadBotTemplatesPatch().Enable();
            new LoadBotProfileFromServerPatch().Enable();
            new UpdateDogtagPatch().Enable();

            new LootableContainerInteractPatch().Enable();

            // --------- On Dead -----------------------
            new OnDeadPatch().Enable();

            // --------- SCAV MODE ---------------------
            //new ScavPrefabLoadPatch().Enable();
            //new ScavProfileLoadPatch().Enable();
            //new ScavExfilPatch().Enable();
            new DisableScavModePatch().Enable();

            new ForceLocalGamePatch().Enable();

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
