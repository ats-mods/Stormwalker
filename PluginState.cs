using System.Collections.Generic;

namespace Stormwalker {

    public class PluginState {
        public Dictionary<int, int> slotsPerHouse = new();

        public HashSet<int> workersToUnassign = new(); 
    }
}