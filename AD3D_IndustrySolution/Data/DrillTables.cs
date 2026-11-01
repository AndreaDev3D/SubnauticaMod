using AD3D_IndustrySolution.Runtime;
using System;
using System.Collections.Generic;

namespace AD3D_IndustrySolution.Data
{
    // What the excavator can drill, per biome. Kept static rather than on the controller: Unity can't serialize
    // a list of a mod-defined type, so a table set on the prefab is lost when each excavator is instantiated.
    public static class DrillTables
    {
        // Used for any biome without its own table
        public static List<DrillEntry> Default { get; set; } = new List<DrillEntry>();

        // Keyed by the biome name LargeWorld reports (e.g. "safeShallows"), case-insensitive
        public static Dictionary<string, List<DrillEntry>> ByBiome { get; } = new Dictionary<string, List<DrillEntry>>(StringComparer.OrdinalIgnoreCase);

        public static List<DrillEntry> Get(string biome) =>
            !string.IsNullOrEmpty(biome) && ByBiome.TryGetValue(biome, out var table) ? table : Default;
    }
}
