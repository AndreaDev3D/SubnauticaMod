using AD3D_Common;
using AD3D_EnergySolution.BO.Runtime;
using AD3D_EnergySolution.Config;
using AD3D_EnergySolution.Items.Buildable;
using AD3D_EnergySolution.Runtime;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using System.Reflection;
using UnityEngine;
#if SN
using static CraftData;
#endif

namespace AD3D_EnergySolution
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.snmodding.nautilus")]
    public class Plugin : BaseUnityPlugin
    {
        public new static ManualLogSource Logger { get; private set; }
        private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
        public static AssetBundle AssetsBundle { get; private set; }
        public static ModConfig ModConfig { get; private set; }
        public static ModData ModData { get; private set; }

        private void Awake()
        {
            AssetsBundle = Helper.GetAssetBundle(Assembly.Location, "energysolution.asset");
            
            ModConfig = OptionsPanelHandler.RegisterModOptions<ModConfig>();
            ModConfig.Load();

            ModData = SaveDataHandler.RegisterSaveDataCache<ModData>();

            // set project-scoped logger instance
            Logger = base.Logger;

            // Initialize custom prefabs
            InitializePrefabs();

            // register harmony patches, if there are any
            Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private const float SolarRatePerPanel = 0.125f;   // W/sec in full sun, at the surface
        private const int SolarBufferPerPanel = 250;

        private void RegisterSolar(string classID, string friendlyName, int panels, string description, RecipeData recipe)
        {
            // only once the bundle has its prefab (an older bundle would give a buildable that can't be made)
            if (AssetsBundle.LoadAsset<GameObject>($"{classID}.prefab") == null)
            {
                Logger.LogInfo($"{classID} is not in the bundle yet: {friendlyName} is not available");
                return;
            }
            var prefab = new GenericPowerPrefab(classID, friendlyName,
                $"{description} Works best between -10 m and the surface (peaks at the surface).", recipe);
            prefab.Register();
            prefab.OnRegisterCompleted = go =>
            {
                var pow = go.AddComponent<GenericPowerController>();
                pow.MaxPowerAllowed = SolarBufferPerPanel * panels;
                pow.CurrentEmitIntervalSec = 2f;
                pow.CurrentEmitRate = SolarRatePerPanel * panels * pow.CurrentEmitIntervalSec;
                pow.MinDepth = -10f;
                pow.MaxDepth = 0f; // full output at sea level and above
                pow.IsSunSusceptible = true;
            };
        }

        private void InitializePrefabs()
        {
            // Solar family: output and buffer scale with the number of panels (0.125 /s and 250 per panel). The two
            // single panels keep their class IDs, so panels in existing saves stay.
            RegisterSolar("PowerSolarPanel", "Power Solar Panel", 1,
                "A tilted solar panel that produces 0.125 W/sec in full sun.",
                new RecipeData(
                    new Ingredient(TechType.Titanium, 2),
                    new Ingredient(TechType.WiringKit, 1)));

            RegisterSolar("PowerSolarPanel_floor", "Power Solar Panel Flat", 1,
                "A low, flat solar panel that produces 0.125 W/sec in full sun.",
                new RecipeData(
                    new Ingredient(TechType.Titanium, 2),
                    new Ingredient(TechType.WiringKit, 1)));

            RegisterSolar("SolarArray_Medium", "Power Solar Array Medium", 4,
                "Four solar panels on one mast, with a battery. Produces 0.5 W/sec in full sun.",
                new RecipeData(
                    new Ingredient(TechType.Titanium, 4),
                    new Ingredient(TechType.Glass, 2),
                    new Ingredient(TechType.WiringKit, 1),
                    new Ingredient(TechType.Battery, 1)));

            RegisterSolar("SolarArray_Large", "Power Solar Array Large", 9,
                "Nine solar panels on two masts, with a battery bank. Produces 1.125 W/sec in full sun.",
                new RecipeData(
                    new Ingredient(TechType.TitaniumIngot, 1),
                    new Ingredient(TechType.Glass, 4),
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(TechType.PowerCell, 1)));

            var powerWindTurbinePrefab = new GenericPowerPrefab(
                classID: "PowerWindTurbine",
                friendlyName: "Power Wind Turbine",
                shortDescription: "High efficiency Wind Turbine that produce 0.25 W/sec. Works best between -5 m and the surface (peaks at the surface).",
                recipeData: new RecipeData(
                    new Ingredient(TechType.Titanium, 3),
                    new Ingredient(TechType.WiringKit, 1))
                );
            powerWindTurbinePrefab.Register();
            powerWindTurbinePrefab.OnRegisterCompleted = (prefab) =>
            {
                var pow = prefab.AddComponent<PowerWindTurbineController>();
                pow.MaxPowerAllowed = 500;
                pow.CurrentEmitRate = 0.5f;
                pow.CurrentEmitIntervalSec = 2f;
                pow.MinDepth = -5f;
                pow.MaxDepth = 0f; // full output at sea level and above
                pow.IsSunSusceptible = true;
            };

            var deepEnginePrefab = new GenericPowerPrefab(
                classID: "DeepEngine",
                friendlyName: "Deep Engine v2",
                shortDescription: "High efficiency electric generator that produce 750w of energy in deep water. Works best between -5 and -200 (deeper = stronger).",
                recipeData: new RecipeData(
                    new Ingredient(TechType.Titanium, 5),
                    new Ingredient(TechType.WiringKit, 1))
                );
            deepEnginePrefab.Register();
            deepEnginePrefab.OnRegisterCompleted = (prefab) =>
            {
                prefab.AddComponent<DeepEngineAnimations>();

                var pow = prefab.AddComponent<DeepEngineController>();
                pow.MaxPowerAllowed = 750;
                pow.CurrentEmitRate = 0.75f;
                pow.CurrentEmitIntervalSec = 2f;
                pow.MinDepth = -5f;
                pow.MaxDepth = -200f;
                pow.IsSunSusceptible = false;
            };

            var biomassBurnerPrefab = new GenericPowerPrefab(
                classID: "BiomassBurner",
                friendlyName: "Biomass Burner",
                shortDescription: "Burns fish and plants to produce 0.5 W/sec. Surface only: it needs air. Each item gives half the energy it would give a Bioreactor.",
                recipeData: new RecipeData(
                    new Ingredient(TechType.Titanium, 4),
                    new Ingredient(TechType.WiringKit, 1),
                    new Ingredient(TechType.Quartz, 2))
                )
            {
                FuelStorageSize = new Vector2Int(3, 3),
                SurfaceOnly = true,
                BakeRootRotation = true,
            };
            biomassBurnerPrefab.Register();
            biomassBurnerPrefab.OnRegisterCompleted = (prefab) =>
            {
                var pow = prefab.AddComponent<BiomassBurnerController>();
                pow.MaxPowerAllowed = 500;
                pow.CurrentEmitRate = 1.0f;
                pow.CurrentEmitIntervalSec = 2f;
                pow.FuelEfficiency = 0.5f;
                pow.IsSunSusceptible = false;
            };
        }
    }
}