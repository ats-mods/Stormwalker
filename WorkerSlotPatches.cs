using System.Collections.Generic;
using System.ComponentModel;
using Eremite;
using Eremite.Buildings;
using Eremite.Buildings.UI;
using Eremite.Characters.Villagers;
using UnityEngine;

namespace Stormwalker {

    public static class WorkerSlotPatches {

        private static Sprite status = null;
        private static HashSet<int> toUnassign = new();

        public static void QueueUnassign(Villager villager){
            if(status == null){
                status = Utils.LoadSprite("production.png");
            }
            Plugin.Log($"Queueing {villager.Id} from unassign");
            toUnassign.Add(villager.Id);
        }

        public static void TryUnassign(ProductionState production){
            Plugin.Log($"Try to remove {production.worker} from list");
            if(toUnassign.Remove(production.worker)){
                Plugin.Log($"Removing villager {production.worker} from list");
                var service = GameMB.VillagersService;

                Villager villager = service.GetVillager(production.worker);
                service.ReleaseFromProfession(villager, false);
            }
        }

        public static void OverrideProductionIcon(BuildingProductionSlot slot){
            if(toUnassign.Contains(slot.villager.Id)){
                slot.SetStatus(status, ()=> "Producing; Worker set to leave once finished");
            }
        }
    }
}