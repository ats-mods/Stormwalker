using Eremite.Services;

namespace Stormwalker {

    public static class SettingsPatches{

        public static void Apply(){
            EditTexts();
        }

        public static void EditTexts(){
            var service = Serviceable.TextsService as TextsService;
            service.texts["MetaReward_ConsumptionControlActivation_Name"] += " (I)";
        }

    }
}