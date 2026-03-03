using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AD3D_EnergySolution.Config
{
    [Menu(PluginInfo.PLUGIN_NAME +" Settings")]
    public class DatabaseConfig : ConfigFile
    {
        [JsonIgnore]
        public Action OnConfigChanged;

        [Slider("Max Power Allowed", 500, 750, DefaultValue = 500, Step = 5, Tooltip = "Max power capacity for each generator")]
        public int MaxPowerAllowed { get; set; } = 500;

        [Slider("Power Multiplier", 1, 3, DefaultValue = 1, Tooltip = "Power multiplier for depth algorithm")]
        public int PowerMultiplier { get; set; } = 1;

        [Toggle("Makes Noise"), OnChange(nameof(ConfigChanged))]
        public bool MakesNoise { get; set; } = false;

        [Toggle("Verboso", Tooltip = "Log info in log"), OnChange(nameof(ConfigChanged))]
        public bool LogEvent { get; set; } = true;

        public Dictionary<string, float> LubricantLevels { get; set; } = new Dictionary<string, float>();

        private void ConfigChanged(ToggleChangedEventArgs e)
        {
            Plugin.DatabaseConfig.Load();
            OnConfigChanged?.Invoke();
        }
    }
}
