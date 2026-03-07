using Nautilus.Json;
using System.Collections.Generic;

namespace AD3D_EnergySolution.Config
{
    public class ModData : SaveDataCache
    {
        public Dictionary<string, float> LubricantLevels { get; set; } = new Dictionary<string, float>();
    }
}
