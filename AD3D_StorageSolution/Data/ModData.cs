using Nautilus.Json;
using System.Collections.Generic;

namespace AD3D_StorageSolution.Data
{
    public class ModData : SaveDataCache
    {
        public Dictionary<string, TechType> StorageFilters { get; set; } = new Dictionary<string, TechType>();
    }
}
