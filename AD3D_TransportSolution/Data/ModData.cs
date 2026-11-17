using Nautilus.Json;
using System.Collections.Generic;

namespace AD3D_TransportSolution.Config
{
    public class ModData : SaveDataCache
    {
        // Monorail rails between pylons, by prefab id: "idA|idB" (a pylon has at most two)
        public List<string> Links { get; set; } = new List<string>();

        // Telescoped pylons, by prefab id: height level 1..4 (missing = 0, the lowest)
        public Dictionary<string, int> Heights { get; set; } = new Dictionary<string, int>();

        // Prawn Suits riding on carriers: carrier prefab id -> Prawn prefab id
        public Dictionary<string, string> Carried { get; set; } = new Dictionary<string, string>();
    }
}
