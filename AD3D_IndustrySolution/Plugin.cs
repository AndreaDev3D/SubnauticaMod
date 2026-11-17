using AD3D_Common;
using AD3D_IndustrySolution.Data;
using AD3D_IndustrySolution.Items.Buildable;
using AD3D_IndustrySolution.Runtime;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Crafting;
using Nautilus.Handlers;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AD3D_IndustrySolution
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.snmodding.nautilus")]
    public class Plugin : BaseUnityPlugin
    {
        public new static ManualLogSource Logger { get; private set; }

        private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

        public static AssetBundle AssetBundle { get; private set; }
        public static ModData ModData { get; private set; }

        private void Awake()
        {
            AssetBundle = Helper.GetAssetBundle(Assembly.Location, "industrysolution.asset");

            ModData = SaveDataHandler.RegisterSaveDataCache<ModData>();

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
            var excavatorPrefab = new MachinePrefab(
                $"MechExcavator{Constant.Suffix}",
                "Mech Excavator",
                "Automated drilling rig. Place it on the sea floor, power it and keep it lubricated: it drills resources into its storage.",
                "Mech_Excavator",
                "MechExcavator",
                new RecipeData(new Ingredient(TechType.Titanium, 1)));
            excavatorPrefab.Register();
            excavatorPrefab.OnRegisterCompleted = (prefab) =>
            {
                var animator = prefab.AddComponent<DrillAnimator>();
                animator.SlideName = "EX_Drill_Slide";
                animator.Slide2Name = "EX_Drill_Slide2";
                animator.SpinName = "EX_Drill_Spin";
                animator.BitName = "EX_Bit_low";
                animator.SlideStroke = 0.80f;  // telescope strokes the model was built for
                animator.Slide2Stroke = 1.10f;
                animator.MaxSpinSpeed = 240f;

                // Plants the feet on uneven terrain (needs the rigged model; no-op otherwise)
                var legIK = prefab.AddComponent<LegIK>();
                legIK.Prefix = "EX_";
                legIK.Legs = "FL,FR,RL,RR";
                legIK.MaxDrop = 0.8f;
                legIK.MaxRise = 0.6f;
                legIK.MaxFootTilt = 12f;

                var excavator = prefab.AddComponent<DrillController>();
                excavator.DisplayName = "Mech Excavator";
                excavator.CycleSeconds = 60f;
                excavator.PowerPerSecond = 0.25f;   // 15 power per cycle
                excavator.LubricantPerCycle = 0.02f; // one Lubricant (25%) lasts 12-13 cycles
            };

            // Tripod drill: same drill tables and lubricant, a quarter of the storage, slower and cheaper to run
            var smallDrillPrefab = new MachinePrefab(
                $"SmallDrill{Constant.Suffix}",
                "Small Drill",
                "Compact tripod drill. Place it on the sea floor, power it and keep it lubricated: it slowly drills resources into a small storage.",
                "Small_Drill",
                "Small_Drill",
                new RecipeData(new Ingredient(TechType.Titanium, 1)))
            {
                StorageSize = new Vector2Int(3, 3),
                PowerPoint = new Vector3(0f, 4.5f, 0f), // the yellow cap on top of the column
            };
            smallDrillPrefab.Register();
            smallDrillPrefab.OnRegisterCompleted = (prefab) =>
            {
                var animator = prefab.AddComponent<DrillAnimator>();
                animator.SlideName = "SD_Drill_Slide";
                animator.SpinName = "SD_Drill_Spin";
                animator.BitName = "SD_Bit";
                animator.SlideStroke = 0.80f;  // the shaft's travel in the model
                animator.MaxSpinSpeed = 300f;
                animator.ShakeAmplitude = 0.005f;
                animator.FXScale = 0.6f;

                var legIK = prefab.AddComponent<LegIK>();
                legIK.Prefix = "SD_";
                legIK.Legs = "A,B,C";
                legIK.MaxDrop = 0.5f;  // about the +-15 degrees of hip swing the rams are built for
                legIK.MaxRise = 0.45f;
                legIK.MaxFootTilt = 12f;

                var drill = prefab.AddComponent<DrillController>();
                drill.DisplayName = "Small Drill";
                drill.CycleSeconds = 90f;
                drill.PowerPerSecond = 0.05f;    // 4.5 power per cycle, a fifth of the excavator's rate
                drill.LubricantPerCycle = 0.02f; // one Lubricant (25%) lasts 12-13 cycles
                drill.LubricantLightHeight = 0.6f; // clears the dome, so the light lands on the yellow lubricant cap
            };

            // Placeholder until per-biome tables exist (add those to DrillTables.ByBiome)
            DrillTables.Default = new List<DrillEntry>
            {
                new DrillEntry(TechType.Titanium, 40f),
                new DrillEntry(TechType.Copper, 20f),
                new DrillEntry(TechType.Quartz, 15f),
                new DrillEntry(TechType.Lead, 10f),
                new DrillEntry(TechType.Silver, 7f),
                new DrillEntry(TechType.Gold, 5f),
                new DrillEntry(TechType.Lithium, 2f),
                new DrillEntry(TechType.Diamond, 1f),
            };
        }
    }
}
