using AD3D_Common;
using AD3D_TransportSolution.Items.Drivable;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Utility;
using System.Reflection;
using UnityEngine;

namespace AD3D_TransportSolution
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
            AssetBundle = Helper.GetAssetBundle(Assembly.Location, "transportsolutions.asset");
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
            //YeetKnifePrefab.Register();
            var Monorail_Straight = new RailItem("Monorail_Straight", "Monorail Straight", "Streight Rail");
            Monorail_Straight.Register();

            var Monorail_Curve = new RailItem("Monorail_Curve", "Monorail_Curve", "Monorail Curve");
            Monorail_Curve.Register();

            var Monorail_Pod = new RailItem("Monorail_Pod", "Monorail Pod", "Monorail Pod");
            Monorail_Pod.Register();
        }
    }
}