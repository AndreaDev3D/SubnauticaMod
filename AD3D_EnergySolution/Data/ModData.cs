using Nautilus.Json;
using System.Collections.Generic;

namespace AD3D_EnergySolution.Config
{
    public class ModData : SaveDataCache
    {
        public Dictionary<string, float> LubricantLevels { get; set; } = new Dictionary<string, float>();

        // Generators the player switched off, by prefab id (everything else is on)
        public HashSet<string> SwitchedOff { get; set; } = new HashSet<string>();

        // Biomass Burners: energy left in the item that is burning, by prefab id
        public Dictionary<string, float> BurnerFuel { get; set; } = new Dictionary<string, float>();
    }
}
