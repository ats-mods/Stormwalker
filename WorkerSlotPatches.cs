using System.Collections.Generic;
using System.ComponentModel;
using Eremite;
using Eremite.Buildings;
using Eremite.Buildings.UI;
using Eremite.Characters.Villagers;
using Eremite.Services.Analytics;
using Eremite.View.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace Stormwalker {

    public static class WorkerSlotPatches {
        static readonly string MARKER_NAME = "StormwalkerMarker";

        private static Sprite marker = null;
        private static HashSet<int> toUnassign => Plugin.State.workersToUnassign;

        public static void PutMarkerIn(BuildingWorkerSlot slot){
            var statusIcon = slot.transform.Find("StatusIcon")?.gameObject;
            if(statusIcon == null)
                return;
            var go = Utils.MakeGameObject(statusIcon, MARKER_NAME, true);
            go.SetActive(false);
            go.transform.localPosition = new Vector3(-10, 10, 0);
            go.transform.SetScale(0.2f);
            var myImage = go.AddComponent<Image>();
            if (marker == null){
                var markerImage = Utils.StealComponent<Image>("/HUD/SeasonEffectsInitialPanel/Content/FieldEffects/NamedEffectSlot/Marker");
                marker = markerImage.sprite;
            }
            myImage.sprite = marker;
        }

        public static void QueueToggleUnassign(Villager villager){
            if(!toUnassign.Remove(villager.Id)){
                toUnassign.Add(villager.Id);
            }
        }

        public static void TryUnassign(ProductionState production){
            if(toUnassign.Remove(production.worker)){
                var service = GameMB.VillagersService;

                Villager villager = service.GetVillager(production.worker);
                service.ReleaseFromProfession(villager, false);
            }
        }

        public static void Remove(Villager villager){
            toUnassign.Remove(villager.Id);
        }

        public static bool UpdateMarkerStatus(BuildingProductionSlot slot){
            var go = slot.transform.Find($"StatusIcon/{MARKER_NAME}");
            if(go == null)
                return false;
            bool result = toUnassign.Contains(slot.villager.Id);
            go.SetActive(result);
            return result;
        }
    }
}