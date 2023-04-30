using Eremite;
using Eremite.MapObjects;

namespace Stormwalker {

    public class BuildingCopier : GameMB {

        private void Update(){
            if(!Eremite.Controller.GameController.IsGameActive)
                return;

            if (MB.InputService.WasTriggered(MB.InputConfig.CopyBuilding, false))
			{
				var deposit = GameInputService.MouseoverObject.Value as ResourceDeposit;
                if(deposit != null && DepositsService.HutsMatrix.ContainsKey(deposit.Model)){
                    foreach (var building in DepositsService.HutsMatrix[deposit.Model])
                    {
                        if(GameContentService.IsUnlocked(building) && ConstructionService.CanConstruct(building)){
                            GameBlackboardService.BuildingConstructionRequested.OnNext(building);
                            break;
                        }
                    }
                }
			}
        }
    }
}