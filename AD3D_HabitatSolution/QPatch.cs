﻿using AD3D_HabitatSolution.BO.Base;
using AD3D_HabitatSolution.BO.Utils;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;

namespace AD3D_HabitatSolution
{
    [QModCore]
    public class QPatch
    {
        internal static HabitatTest HabitatTest { get; } = new HabitatTest();

        [QModPatch]
        public static void Patch()
        {
            //Helper.LoadConfig();

            HabitatTest.Patch();

            Helper.LogEvent($"Patched successfully [v1.0.0]");
        }
    }
}