using Cysharp.Threading.Tasks.Triggers;
using Eremite;
using Eremite.Buildings;
using Eremite.Services;
using UnityEngine;

namespace Stormwalker {

    public static class MinePatches{

        private static readonly string GO_NAME = "SWArea";
        private static GameObject prefab; 

        public static void Apply(){
            var S = Serviceable.Settings;
            var farmModel = (FarmModel) S.GetBuilding("SmallFarm");
            MakeAreaGameObject(farmModel);
        }

        public static void AttachPrefab(Mine mine){
            var cloned = Object.Instantiate(prefab, mine.transform);
            cloned.name = GO_NAME;
        }

        private static void MakeAreaGameObject(FarmModel farmModel){
            var farmGo = farmModel.prefab.gameObject;
            var originalArea = farmGo.transform.Find("Area");
            prefab = Utils.MakeGameObject(null, GO_NAME);
            // Object.DontDestroyOnLoad(areaGo);
            // Utils.CopyComponent(originalArea.GetComponent<Transform>(), areaGo);
            Utils.CopyComponent(originalArea.GetComponent<SpriteRenderer>(), prefab);
            var oriTransform = originalArea.GetComponent<Transform>();
            prefab.transform.position = oriTransform.position;
            prefab.transform.localPosition = oriTransform.localPosition;
            prefab.transform.rotation = oriTransform.rotation;
        }

        public static void Show(Mine mine){
            var go = mine.FindChild(GO_NAME);
            var renderer = go.GetComponent<SpriteRenderer>();
            var areaRect = new Rect(mine.state.field - Vector2Int.one, new Vector2(5, 5));
            renderer.transform.position = new Vector3(areaRect.x, renderer.transform.position.y, areaRect.y);
			renderer.size = areaRect.size;
            renderer.enabled = true;
        }

        public static void Hide(Mine mine){
            var renderer = mine.FindChild(GO_NAME).GetComponent<SpriteRenderer>();
            renderer.enabled = false;
        }
    }
}