using System.Linq;
using Eremite;
using Eremite.Buildings;
using Eremite.Buildings.UI;
using Eremite.MapObjects;
using Eremite.Services;
using Eremite.View;

namespace Stormwalker {

    public class BuildingCopier : GameMB {

        private void Update(){
            if(!Eremite.Controller.GameController.IsGameActive)
                return;

            if(ShouldIgnoreInput())
                return;

            if (!InputService.WasTriggered(MB.InputConfig.CopyBuilding, false))
                return;

            var mapObject = GameInputService.MouseoverObject.Value;
            Plugin.Log(mapObject);
            switch(mapObject){
                case ResourceDeposit deposit:
                    if(deposit.State.isAvailable && DepositsService.HutsMatrix.ContainsKey(deposit.Model)){
                        foreach(var building in DepositsService.HutsMatrix[deposit.Model]){
                            if(BuildConditional(building)) break;
                        }
                    }
                    break;
                case NaturalResource resource when resource.State.isAvailable:
                    BuildConditional(Settings.GetBuilding("Woodcutters Camp"));
                    break;
                case Spring spring when spring.State.isAvailable:
                    BuildConditional(Settings.rainpunkConfig.extractor);
                    break;
                case Field field:
                    if(OreService.HasAvailableOre(field.Pos)){
                        var ore = OreService.GetOre(field.Pos);
                        foreach(var building in OreService.MinesMatrix[ore.Model.refGood.Name]){
                            if(BuildConditional(building)) break;
                        }
                    }
                    break;
            }
        }

        private bool ShouldIgnoreInput()
		{
			return BuildingPanel.currentBuilding != null || !ModeService.Idle.Value || InputService.IsOverUI();
		}

        private bool BuildConditional(BuildingModel building){
            if(GameContentService.IsUnlocked(building) && ConstructionService.CanConstruct(building)){
                GameBlackboardService.BuildingConstructionRequested.OnNext(building);
                return true;
            }
            return false;
        }
    }
}