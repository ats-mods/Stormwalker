using BepInEx;
using HarmonyLib;
using Eremite;
using Eremite.Controller;
using Eremite.Services;
using UnityEngine;
using Eremite.Buildings;
using Eremite.Buildings.UI;
using Eremite.View.HUD;
using TMPro;
using System.Linq;
using Eremite.View.HUD.Construction;

namespace Stormwalker
{
    internal static class Patches{
        [HarmonyPatch(typeof(MainController), nameof(MainController.OnServicesReady))]
        [HarmonyPostfix]
        private static void HookMainControllerSetup()
        {   
            MinePatches.Apply();
            var settings = Serviceable.Settings;
            settings.GetBuilding("Trading Post").movable = true;
            settings.buildingsCache.cache = null;
        }

        [HarmonyPatch(typeof(Mine), nameof(Mine.SetUp))]
        [HarmonyPostfix]
        private static void SetUpMine(Mine __instance){
            MinePatches.AttachPrefab(__instance);
        }

        [HarmonyPatch(typeof(OreService), nameof(OreService.GetOreUnder), typeof(Vector2Int), typeof(Vector2Int))]
        [HarmonyPrefix]
        private static void ExtendMineArea(ref Vector2Int isoPos, ref Vector2Int size)
        { 
            isoPos -= Vector2Int.one;
            size += 2*Vector2Int.one;
        }

        [HarmonyPatch(typeof(Mine), nameof(Mine.OnPlacingStarted))]
        [HarmonyPrefix]
        private static void ShowMineArea(Mine __instance) => MinePatches.Show(__instance);

        [HarmonyPatch(typeof(MinePanel), nameof(MinePanel.Show))]
        [HarmonyPostfix]
        private static void ShowMineAreaPanel(Mine mine) => MinePatches.Show(mine);

        [HarmonyPatch(typeof(Mine), nameof(Mine.OnPlaced))]
        [HarmonyPrefix]    
        private static void HideMineArea(Mine __instance) => MinePatches.Hide(__instance);

        [HarmonyPatch(typeof(ProductionBuildingPanel), nameof(ProductionBuildingPanel.Hide))]
        [HarmonyPrefix]
        private static void HideMineArea(ProductionBuildingPanel __instance){
            if(__instance is MinePanel mp) MinePatches.Hide(mp.current);
        }

        [HarmonyPatch(typeof(BaseRecipeLimitPanel), nameof(BaseRecipeLimitPanel.OnPlusClicked))]
        [HarmonyPrefix]
        private static bool OnPlusClicked(BaseRecipeLimitPanel __instance){
            if(MB.InputService.IsTriggering(MB.InputConfig.InputModifierMore)){
                __instance.OnValueChanged(__instance.Limit + 10, true);
                return false;
            } else if (MB.InputService.IsTriggering(MB.InputConfig.InputModifierControl)){
                __instance.OnValueChanged(__instance.Limit + 5, true); 
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(BaseRecipeLimitPanel), nameof(BaseRecipeLimitPanel.OnMinusClicked))]
        [HarmonyPrefix]
        private static bool OnMinusClicked(BaseRecipeLimitPanel __instance){
            if(MB.InputService.IsTriggering(MB.InputConfig.InputModifierMore)){
                __instance.OnValueChanged(__instance.Limit - 10, true);
                return false;
            } else if (MB.InputService.IsTriggering(MB.InputConfig.InputModifierControl)){
                __instance.OnValueChanged(__instance.Limit - 5, true); 
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(TimeScalePanel), nameof(TimeScalePanel.SetUp))]
        [HarmonyPostfix] 
        private static void TimeScalePanel__SetUp(TimeScalePanel __instance){
            var transform = __instance.gameObject.transform;
            var newGo = Object.Instantiate(transform.Find("Slot (5)"), transform);
            newGo.name = "Slot (6)";
            newGo.localPosition = new Vector3(90, 90, 0);
            newGo.GetComponent<TimeScaleSlot>().timeScale = 5f;
            newGo.Find("Desc").GetComponent<TextMeshProUGUI>().text = $"x{Plugin.SUPER_SPEED_SCALE:0.}";
        }

        [HarmonyPatch(typeof(TimeScaleService), nameof(TimeScaleService.SpeedUp))]
        [HarmonyPrefix]
        private static bool TimeScaleService__SpeedUp(TimeScaleService __instance){
            float speedNow = __instance.Scale.Value;
            if(speedNow == Plugin.SUPER_SPEED_SCALE){
                Serviceable.SoundsManager.PlayFailedSound();
                return false;
            }
            if(speedNow == TimeScaleService.Speeds.Last()){
                __instance.Change(5f, true, false);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(TimeScaleService), nameof(TimeScaleService.SlowDown))]
        [HarmonyPrefix]
        private static bool TimeScaleService__SlowDown(TimeScaleService __instance){
            float speedNow = __instance.Scale.Value;
            if(speedNow == Plugin.SUPER_SPEED_SCALE){
                 __instance.Change(TimeScaleService.Speeds.Last(), true, false);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(BuildingsPanel), nameof(BuildingsPanel.SetUp))]
        [HarmonyPostfix]
        private static void BuildingPanel__SetUp(BuildingsPanel __instance){
            Plugin.buildingPanel = __instance;
        }
    }
}