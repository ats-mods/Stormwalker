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
using Eremite.View.Popups.Recipes;
using Eremite.Buildings.UI.Trade;
using UnityEngine.Events;
using Eremite.Characters.Villagers;
using System;
using UniRx;
using Eremite.Controller.Generator;

namespace Stormwalker
{
    internal static class Patches{
        [HarmonyPatch(typeof(MainController), nameof(MainController.OnServicesReady))]
        [HarmonyPostfix]
        private static void HookMainControllerSetup()
        {   
            MinePatches.Apply();
        }

        [HarmonyPatch(typeof(GameLoader), nameof(GameLoader.LoadState))]
        [HarmonyPostfix]
        private static void Load(GameLoader __instance)
        {
            Plugin.SetupState(__instance.state ==  null);
        }

        [HarmonyPatch(typeof(Mine), nameof(Mine.SetUp))]
        [HarmonyPostfix]
        private static void SetUpMine(Mine __instance) => MinePatches.AttachPrefab(__instance);

        [HarmonyPatch(typeof(HousePanel), nameof(HousePanel.Awake))]
        [HarmonyPostfix]
        private static void SetUpHousePanel(HousePanel __instance) => HousePatches.PatchPanel(__instance);

        [HarmonyPatch(typeof(HousePanel), nameof(HousePanel.Show))]
        [HarmonyPostfix]
        private static void House__Show(House house) => HousePatches.Show(house);

        [HarmonyPatch(typeof(House), nameof(House.GetPlacesLeft))]
        [HarmonyPrefix]
        private static bool House__GetPlacesLeft(House __instance, ref int __result){
            __result = HousePatches.houseLimiter.state.GetAllowedResidents(__instance) - __instance.state.residents.Count;
            return false; // This skips the original method
        }

        [HarmonyPatch(typeof(BuildingWorkerSlot), nameof(BuildingWorkerSlot.Unassign))]
        [HarmonyPrefix]
        private static bool BuildingWorkerSlot__Unassign(BuildingWorkerSlot __instance){
            if (MB.InputService.IsTriggering(MB.InputConfig.InputModifierControl)) {
                WorkerSlotPatches.QueueToggleUnassign(__instance);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(ProductionBuilding), nameof(ProductionBuilding.DispatchProductionFinished))]
        [HarmonyPostfix]
        private static void UnassignSinceDone(ProductionState production) => WorkerSlotPatches.TryUnassign(production);

        [HarmonyPatch(typeof(BuildingProductionSlot), nameof(BuildingProductionSlot.SetStatus))]
        [HarmonyPrefix]
        private static void OverrideProduction(BuildingProductionSlot __instance, ref Func<string> descSource){
            if(WorkerSlotPatches.UpdateMarkerStatus(__instance)){
                string original = descSource.Invoke();
                descSource = (() => original + "; worker set to leave once production finishes");
            }
        }

        [HarmonyPatch(typeof(VillagersService), nameof(VillagersService.RemoveFromProfession))]
        [HarmonyPostfix]
        private static void SyncStateVillagerGone(Villager villager) => WorkerSlotPatches.Remove(villager);

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

        [HarmonyPatch(typeof(IngredientsMenu), nameof(IngredientsMenu.OnIngredientClicked))]
        [HarmonyPrefix]
        private static bool OnIngredientClicked(IngredientState state){
            if (MB.InputService.IsTriggering(MB.InputConfig.InputModifierControl))
			{
                var goodModel = Serviceable.Settings.GetGood(state.good);
                GameMB.GameBlackboardService.RecipesPopupRequested.OnNext(new RecipesPopupRequest(goodModel, true));
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(IngredientSlot), nameof(IngredientSlot.OnClick))]
        [HarmonyPrefix]
        private static bool IngredientSlot__OnClick(IngredientSlot __instance){
            if (MB.InputService.IsTriggering(MB.InputConfig.InputModifierControl))
			{
                GameMB.GameBlackboardService.RecipesPopupRequested.OnNext(new RecipesPopupRequest(__instance.GetGood(), true));
                return false;
            }
            return true;
        }


        [HarmonyPatch(typeof(TimeScalePanel), nameof(TimeScalePanel.SetUp))]
        [HarmonyPostfix]
        private static void TimeScalePanel__SetUp(TimeScalePanel __instance){
            var transform = __instance.gameObject.transform;
            var newGo = GameObject.Instantiate(transform.Find("Slot (5)"), transform);
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

        [HarmonyPatch(typeof(TradingGoodSlot), nameof(TradingGoodSlot.Start))]
        [HarmonyPostfix]
        private static void GoodSlotAddMiddleClick(TradingGoodSlot __instance){
            __instance.multiButton.AddMiddleListener(
                new UnityAction( ()=> TradePatches.MatchOffer(__instance)  )
            );
        }

        [HarmonyPatch(typeof(TraderPanel), nameof(TraderPanel.Show))]
        [HarmonyPostfix]
        private static void TraderPanel__Show(TraderPanel __instance){
            var go = __instance.FindChild("Content");
            var text = Utils.PatchInGameObject<TextMeshProUGUI>(
                go, 
                "StormwalkerCityScore", 
                t => { t.fontSize = 24; t.fontSizeMax = 24; t.transform.localPosition = new Vector3(460, 328, 0); }
            );
            var cityScore = (Serviceable.TradeService as TradeService).GetScore();
            text.text = $"City Score: {cityScore}";
        }

        // [HarmonyPatch(typeof(WorkersOverlaysManager), "Show")]
        // [HarmonyPostfix]
        // private static void InjectOverlay(WorkersOverlaysManager __instance, ProductionBuilding productionBuilding, int index){
        //     var overlay = __instance.overlays[index];
        //     OverlayPatches.EnhanceOverlay(overlay);

        // }
    }
}