using AD3D_Common;
using AD3D_StorageSolution.Items.Storage;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Utility;
using System.Reflection;
using UnityEngine;

namespace AD3D_StorageSolution
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.snmodding.nautilus")]
    public class Plugin : BaseUnityPlugin
    {
        public new static ManualLogSource Logger { get; private set; }

        private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

        public static AssetBundle AssetBundle { get; private set; }

        private void Awake()
        {
            AssetBundle = Helper.GetAssetBundle(Assembly.Location, "storagesolution.asset");

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
            var _baseDescription = "Programmable dark matter storage to allocate quantum particle efficiently. \n";

            new StorageItem(
                "QuantumStorage_floor_S",
                "Quantum Crate",
                $"{_baseDescription} Store 64 items in a reduced space."
                , new Vector2Int(8, 8), true)
                .Register();


            new StorageItem(
                "QuantumStorage_floor_L",
                "Large Quantum Crate",
                $"{_baseDescription} Store 90 items in a reduced space."
                , new Vector2Int(9, 10), true)
                .Register();


            new StorageItem(
                "QuantumStorage_wall_S",
                "Quantum Storage",
                $"{_baseDescription} Store 64 items in a reduced space."
                , new Vector2Int(8, 8), false)
                .Register();


            new StorageItem(
                "QuantumStorage_wall_L",
                "Large Quantum Storage",
                $"{_baseDescription} Store 90 items in a reduced space."
                , new Vector2Int(9, 10), false)
                .Register();
        }
    }
}