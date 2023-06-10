using System.IO;
using BepInEx;
using BepInEx.Configuration;
using Eremite;
using Eremite.Buildings.UI;
using Eremite.Controller;
using Eremite.MapObjects;
using Eremite.Services;
using Eremite.View.Cameras;
using Eremite.View.HUD.Construction;
using HarmonyLib;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

namespace Stormwalker
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static readonly float SUPER_SPEED_SCALE = 5f;
        private Harmony harmony;
        public static Plugin Instance;
        public static void Log(object obj) => Instance.Logger.LogInfo(obj);
        public static void Error(object obj) => Instance.Logger.LogError(obj);

        public static PluginState State {get; private set;} = new();

        public static BuildingsPanel buildingPanel = null;

        KeyboardShortcut zoomOverviewKey;
        KeyboardShortcut superSpeed;

        private void Awake()
        {
            Instance = this;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            harmony = Harmony.CreateAndPatchAll(typeof(Patches));

            zoomOverviewKey = new(UnityEngine.KeyCode.Backspace);
            superSpeed = new(UnityEngine.KeyCode.Alpha5);

            this.gameObject.AddComponent<Woodcutters>();
            this.gameObject.AddComponent<BuildingCopier>();
        }

        public static void SetupState(bool noGameState){
            if(noGameState || MB.GameSaveService.IsNewGame()){
                State = new();
            } else {
                State = Load();
            }
            MB.GameSaveService.IsSaving.Where(isStarting=>!isStarting).Subscribe(_=>Save());
        }

        private static void Save(){
            var path = Path.Combine(Serviceable.ProfilesService.GetFolderPath(), PluginInfo.PLUGIN_GUID+".save");
            JsonIO.SaveToFile(State, path);
        }

        private static PluginState Load(){
            var path = Path.Combine(Serviceable.ProfilesService.GetFolderPath(), PluginInfo.PLUGIN_GUID+".save");
            try {
                return JsonIO.GetFromFile<PluginState>(path);
            } catch {
                Plugin.Error("Error while trying to load save state");
                return new();
            }
        }

        Vector2 zoomLimit = new Vector2(-20f, -8f);

        private void Update(){
            if(!GameController.IsGameActive || MB.InputService.IsLocked()) 
                return;

            if(zoomOverviewKey.IsDown()){
                var zoom = -60f;
                var cam = GameController.Instance.CameraController;
                var animator = cam.Camera.GetComponentInChildren<PostProcessesAnimator>();
                if(cam.zoomLimit.x == zoom && cam.zoomLimit.y == zoom){ // Zoom back to normal
                    cam.zoomLimit = zoomLimit;
                    if(animator != null) animator.AdjustDeapthOfField(MB.ClientPrefsService.DeapthOfField.Value);
                } else { // Zoom out
                    zoomLimit = cam.zoomLimit;
                    cam.zoomLimit = new Vector2(zoom, zoom);
                    if(animator != null) animator.AdjustDeapthOfField(isOn: false);
                }
            } else if(superSpeed.IsDown()){
                GameMB.TimeScaleService.Change(SUPER_SPEED_SCALE, true, false);
            }
        }

        private void OnDestroy()
        {
            harmony.UnpatchSelf();
        }
    }
}
