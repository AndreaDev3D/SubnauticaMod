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
        public static DeepEngineConfig DeepEngineConfig { get; private set; }

        private void Awake()
        {
            AssetsBundle = Helper.GetAssetBundle(Assembly.Location, "energysolution.asset");
            
            DeepEngineConfig = OptionsPanelHandler.RegisterModOptions<DeepEngineConfig>();
            DeepEngineConfig.Load();

            // set project-scoped logger instance
            Logger = base.Logger;

            // Initialize custom prefabs
            InitializePrefabs();

            // register harmony patches, if there are any
            Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void InitializePrefabs()
        {
            var solarPanelPrefab = new GenericPowerPrefab(
                classID:"PowerSolarPanel",
                friendlyName: "Power Solar Panel",
                shortDescription: "A high efficiency Solar Panel that produce 0.125 W/sec.",
                recipeData: new RecipeData(
                    new Ingredient(TechType.Titanium, 2),
                    new Ingredient(TechType.WiringKit, 1))
                );
            solarPanelPrefab.Register();
            solarPanelPrefab.OnRegisterCompleted = (prefab) =>
            {
                var pow = prefab.AddComponent<GenericPowerController>();
                pow.MaxPowerAllowed = 250;
                pow.CurrentEmitRate = 0.25f;
                pow.CurrentEmitIntervalSec = 2f;
            };

            var solarPanelFloorPrefab = new GenericPowerPrefab(
                classID: "PowerSolarPanel_floor",
                friendlyName: "Power Solar Panel Flat",
                shortDescription: "A high efficiency Solar Panel that produce 0.125 W/sec.",
                recipeData: new RecipeData(
                    new Ingredient(TechType.Titanium, 2),
                    new Ingredient(TechType.WiringKit, 1))
                );
            solarPanelFloorPrefab.Register();
            solarPanelFloorPrefab.OnRegisterCompleted = (prefab) =>
            {
                var pow = prefab.AddComponent<GenericPowerController>();
                pow.MaxPowerAllowed = 250;
                pow.CurrentEmitRate = 0.25f;
                pow.CurrentEmitIntervalSec = 2f;
            };

            var powerWindTurbinePrefab = new GenericPowerPrefab(
                classID: "PowerWindTurbine",
                friendlyName: "Power Wind Turbine",
                shortDescription: "High efficiency Wind Turbine that produce 0.25 W/sec.",
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
            };

            var deepEnginePrefab = new GenericPowerPrefab(
                classID: "DeepEngine",
                friendlyName: "Deep Engine v2",
                shortDescription: "High efficiency electric generator that produce 750w of energy in deep water.",
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

            };
        }
    }
}