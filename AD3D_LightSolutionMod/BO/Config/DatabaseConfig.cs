using AD3D_LightSolutionMod.BO.Base;
using SMLHelper.V2.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD3D_LightSolutionMod.BO.Config
{
    public class DatabaseConfig : ConfigFile
    {
        public List<DataItem> SwitchItemList { get; set; } = new List<DataItem>();
    }
}
