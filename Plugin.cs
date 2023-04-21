using BepInEx;
using BepInEx.Configuration;
using Eremite;
using Eremite.Controller;
using Eremite.MapObjects;
using Eremite.View.Cameras;
using HarmonyLib;
using UnityEngine;

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


        KeyboardShortcut zoomOverviewKey;
        KeyboardShortcut placeGathererHut;
        KeyboardShortcut superSpeed;

        private void Awake()
        {
            Instance = this;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            harmony = Harmony.CreateAndPatchAll(typeof(Patches));

            zoomOverviewKey = new(UnityEngine.KeyCode.Backspace);
            placeGathererHut = new(UnityEngine.KeyCode.LeftShift);
            superSpeed = new(UnityEngine.KeyCode.Alpha5);
        }

        Vector2 zoomLimit = new Vector2(-20f, -8f);

        private void Update(){
            if(!GameController.IsGameActive) return;

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
            } else if(placeGathererHut.IsDown()){
                var deposit = GameMB.GameInputService.MouseoverObject.Value as ResourceDeposit;
                if(deposit != null && GameMB.DepositsService.HutsMatrix.ContainsKey(deposit.Model)){
                    foreach (var building in GameMB.DepositsService.HutsMatrix[deposit.Model])
                    {
                        if(GameMB.GameContentService.IsUnlocked(building) && GameMB.ConstructionService.CanConstruct(building)){
                            GameMB.GameBlackboardService.BuildingConstructionRequested.OnNext(building);
                            break;
                        }
                    }
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
