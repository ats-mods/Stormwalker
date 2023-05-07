using System.Collections.Generic;
using System.ComponentModel;
using Eremite;
using Eremite.Buildings;
using Eremite.Buildings.UI;
using Eremite.Characters.Villagers;

namespace Stormwalker {

    public static class WorkerSlotPatches {
        private static HashSet<int> toUnassign = new();

        public static void QueueUnassign(Villager villager){
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
    }
}