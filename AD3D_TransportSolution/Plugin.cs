using AD3D_Common;
using AD3D_TransportSolution.Config;
using AD3D_TransportSolution.Items.Buildable;
using AD3D_TransportSolution.Patches;
using AD3D_TransportSolution.Runtime;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Crafting;
using Nautilus.Handlers;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if SN
using static CraftData;
#endif

namespace AD3D_TransportSolution
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
            AssetBundle = Helper.GetAssetBundle(Assembly.Location, "transportsolutions.asset");
            // set project-scoped logger instance
            Logger = base.Logger;

            ModData = SaveDataHandler.RegisterSaveDataCache<ModData>();

            // Initialize custom prefabs
            InitializePrefabs();

            // register harmony patches, if there are any
            Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void InitializePrefabs()
        {
            var pylon = new MonorailPrefab("Monorail_Pylon", "Monorail Pylon",
                "Carries the monorail rail. Build pylons up to 50 m apart, on land or on the seabed: each new pylon lays a rail to the nearest free one.",
                new RecipeData
                {
                    craftAmount = 1,
                    Ingredients = new List<Ingredient> { new Ingredient(TechType.Titanium, 4) }
                })
            {
                Configure = go => go.AddComponent<MonorailPylon>(),
            };
            pylon.Register();

            var pod = new MonorailPrefab("Monorail_Pod", "Monorail Pod",
                "An enclosed cabin that runs along the monorail. Build it aimed at a rail, board it and drive.",
                new RecipeData
                {
                    craftAmount = 1,
                    Ingredients = new List<Ingredient>
                    {
                        new Ingredient(TechType.Titanium, 6),
                        new Ingredient(TechType.Glass, 2),
                        new Ingredient(TechType.WiringKit, 1),
                        new Ingredient(TechType.Lubricant, 1),
                    }
                })
            {
                AllowedOnConstructables = true,
            };
            pod.Configure = go =>
            {
                go.AddComponent<MonorailPod>();
                BuilderPatcher.OnRail[pod.PrefabInfo.TechType] = MonorailPod.MeasureShape(go).Forward;
            };
            BuilderPatcher.OnRail[pod.PrefabInfo.TechType] = Vector3.forward; // until the prefab is first made
            pod.Register();

            var wagon = new MonorailPrefab("Monorail_Wagon", "Monorail Cargo Wagon",
                "A cargo wagon for the monorail. Build it aimed at a rail, then back a pod into it to couple it.",
                new RecipeData
                {
                    craftAmount = 1,
                    Ingredients = new List<Ingredient>
                    {
                        new Ingredient(TechType.Titanium, 5),
                        new Ingredient(TechType.Lubricant, 1),
                    }
                })
            {
                AllowedOnConstructables = true,
                StorageSize = new Vector2Int(6, 6),
            };
            wagon.Configure = go =>
            {
                go.AddComponent<MonorailWagon>();
                BuilderPatcher.OnRail[wagon.PrefabInfo.TechType] = MonorailWagon.MeasureShape(go).Forward;
            };
            BuilderPatcher.OnRail[wagon.PrefabInfo.TechType] = Vector3.forward;
            wagon.Register();

            // only once the bundle has its prefab (an older bundle would break the buildable)
            if (MonorailPrefab.InBundle("Monorail_PrawnCarrier"))
            {
                var carrier = new MonorailPrefab("Monorail_PrawnCarrier", "Monorail Prawn Carrier",
                    "Carries a Prawn Suit along the monorail and slowly recharges it. Walk the Prawn onto the deck and get out to lock it on; get back in to unload.",
                    new RecipeData
                    {
                        craftAmount = 1,
                        Ingredients = new List<Ingredient>
                        {
                            new Ingredient(TechType.Titanium, 6),
                            new Ingredient(TechType.WiringKit, 1),
                            new Ingredient(TechType.Lubricant, 1),
                        }
                    }, iconFallback: "Monorail_Wagon")
                {
                    AllowedOnConstructables = true,
                };
                carrier.Configure = go =>
                {
                    go.AddComponent<MonorailPrawnCarrier>();
                    BuilderPatcher.OnRail[carrier.PrefabInfo.TechType] = MonorailPrawnCarrier.MeasureCarrier(go).Forward;
                };
                BuilderPatcher.OnRail[carrier.PrefabInfo.TechType] = Vector3.forward;
                carrier.Register();
            }
            else
                Logger.LogInfo("Monorail_PrawnCarrier is not in the bundle yet: the Prawn carrier is not available");
        }
    }
}
