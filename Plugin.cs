using BepInEx;
using SIT.A.Tarkov.Core.SP;
using SIT.Tarkov.Core;
using SIT.Tarkov.Core.Bundles;
using SIT.Tarkov.Core.SP;

namespace SIT.A.Tarkov.Core
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            new ConsistencySinglePatch().Enable();
            new ConsistencyMultiPatch().Enable();
            new BattlEyePatch().Enable();
            new SslCertificatePatch().Enable();
            new UnityWebRequestPatch().Enable();
            new WebSocketPatch().Enable();

            PatchConstants.GetBackendUrl();

            BundleSetup.Init();
            BundleManager.GetBundles();
            new EasyAssetsPatch().Enable();
            new EasyBundlePatch().Enable();

            //new LoadBotTemplatesPatch().Enable();
            new UpdateDogtagPatch().Enable();

            new LootableContainerInteractPatch().Enable();

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
