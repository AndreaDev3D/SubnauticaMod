using AD3D_DeepEngineMod;
using QModManager.Utility;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options;
using SMLHelper.V2.Options.Attributes;

namespace AD3D_DeepEngineMod.BO.Config
{
    [Menu("DeepEngine Settings")]
    public class DeepEngineConfig : ConfigFile
    {
        [Slider("Max Power Allowed", 500, 750, DefaultValue = 500, Step =5,Tooltip ="Max power capacity for each generator")]
        public int MaxPowerAllowed { get; set; }
        [Slider("Power Multiplier", 1, 3, DefaultValue = 1, Tooltip = "Power multiplier for depth algo")]
        public int PowerMultiplier { get; set; }

        [Toggle("Makes Noise"), OnChange(nameof(ConfigChanged))]
        public bool MakesNoise { get; set; } = false;
        [Toggle("Verboso",Tooltip ="Log info in QMod log"), OnChange(nameof(ConfigChanged))]
        public bool LogEvent { get; set; } = true;


        private void ConfigChanged(ToggleChangedEventArgs e)
        {
            // Reload if value changed
            QPatch.Config.Load();
            // Log
            Logger.Log(Logger.Level.Info, "Config value was changed!");
            Logger.Log(Logger.Level.Info, $"{e.Value}");
        }
    }
}
