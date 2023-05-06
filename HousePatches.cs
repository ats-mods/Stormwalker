using Eremite.Buildings.UI;
using Eremite.View;
using UnityEngine;

namespace Stormwalker {

    public static class HousePatches{

        public static void Apply(HousePanel panel){
            var go = panel.FindChild("Content/ResidentsPanel");
            var slider = RetrieveSliderPrefab(go);

            //Minsize for panel so the new slider wont fall outside of it.
            go.GetComponent<SimpleVerticalResizer>().minHeight = 200;
            Plugin.Log(slider);
        }

        private static GameObject RetrieveSliderPrefab(GameObject parent){
            var name = "StormwalkerSlots";
            var existingResult = parent.transform.Find(name);
            if(existingResult != null){
                return existingResult.gameObject;
            }
            var result = Utils.MakeGameObject(parent, name);
            var original = GameObject.Find("/HUD/WorkshopPanel/Content/RecipesPanel/Slots/RecipeSlot/Prio");
            Copy(original, result, "Plus");
            Copy(original, result, "Minus");
            Copy(original, result, "Counter");
            result.transform.localPosition = new Vector3(-200, -125, 0);
            return result;
        }

        private static void Copy(GameObject original, GameObject target, string name){
            var made = GameObject.Instantiate(original.transform.Find(name), target.transform);
            made.name = name;
        }
    }
}