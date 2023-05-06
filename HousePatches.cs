using System;
using System.Collections.Generic;
using Eremite;
using Eremite.Buildings;
using Eremite.Buildings.UI;
using Eremite.View;
using Eremite.View.HUD;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Stormwalker {

    public static class HousePatches{

        private static GameObject residentSlider;
        private static HouseLimiter houseLimiter;

        public static void PatchPanel(HousePanel panel){
            var go = panel.FindChild("Content/ResidentsPanel");
            residentSlider = CreateResidentSlider(go);
            houseLimiter = residentSlider.AddComponent<HouseLimiter>();
            houseLimiter.SetUp();
            Plugin.Log($"Created house limiter: {houseLimiter}");

            //Minsize for panel so the new slider wont fall outside of it.
            go.GetComponent<SimpleVerticalResizer>().minHeight = 200;
        }

        private static GameObject CreateResidentSlider(GameObject parent){

            var result = Utils.MakeGameObject(parent, "StormwalkerSlots");
            var original = GameObject.Find("/HUD/WorkshopPanel/Content/RecipesPanel/Slots/RecipeSlot/Prio");
            Copy(original, result, "Plus");
            Copy(original, result, "Minus");
            var counter = Copy(original, result, "Counter");
            var tooltip = counter.GetComponent<SimpleTooltipTrigger>();
            tooltip.headerKey = Utils.Text("Allowed Residents", "Stormwalker_residents_tooltip_head").key;
            tooltip.descKey = Utils.Text(
                "Use the buttons to limit the amount of villagers that may live in this house", "Stormwalker_residents_tooltip_desc"
            ).key;
            result.transform.localPosition = new Vector3(-200, -125, 0);
            return result;
        }
        
        public static void Show(House house){
            houseLimiter.Show(house);
        }

        private static Transform Copy(GameObject original, GameObject target, string name){
            var made = GameObject.Instantiate(original.transform.Find(name), target.transform);
            made.name = name;
            return made;
        }
    }

    public static class HouseLimitState {
        private static Dictionary<int, int> slotsPerHouse = new();

        public static int GetAllowedResidents(House house){
            return Math.Min(house.GetHousingPlaces(), slotsPerHouse.GetValueOrDefault(house.Id, 10));
        }
    }

    class HouseLimiter: GameMB {

        public void SetUp() {
            base.EnsureComponent(counter, out counter, "Counter/Text");
            base.EnsureComponent(plusButton, out plusButton, "Plus");
            base.EnsureComponent(minusButton, out minusButton, "Minus");
            this.plusButton.AddCallback(new UnityAction(this.OnPlusClicked));
            this.minusButton.AddCallback(new UnityAction(this.OnMinusClicked));
        }

        public void Show(House house){
            this.house = house;
            RefreshCounter();
        }

        private void RefreshCounter(){
            int residents = HouseLimitState.GetAllowedResidents(house);
            counter.text = residents.ToString();
        }

        private void OnPlusClicked(){

        }

        private void OnMinusClicked(){
            
        }

        private Button plusButton;
        private Button minusButton;
        private TextMeshProUGUI counter;

        private House house;
    }
}