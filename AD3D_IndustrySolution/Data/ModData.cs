using Nautilus.Json;
using System.Collections.Generic;

namespace AD3D_IndustrySolution.Data
{
    public class ModData : SaveDataCache
    {
        // Keyed by the machine's PrefabIdentifier.Id
        public Dictionary<string, float> LubricantLevels { get; set; } = new Dictionary<string, float>();
    }
}
