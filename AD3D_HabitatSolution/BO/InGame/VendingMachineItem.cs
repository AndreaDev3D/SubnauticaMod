using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD3D_HabitatSolutionMod.BO.InGame
{
    public class VendingMachineItem
    {
        public TechType TechType { get; set; }
        public float Cost { get; set; }
        public CategoryType CategoryType { get; set; }
        public bool IsSaleable { get; set; } = false;

        public VendingMachineItem(TechType type, float cost = 10.0f, CategoryType categoryType = CategoryType.None)
        {
            TechType = type;
            Cost = cost;
            CategoryType = categoryType;
        }
        public VendingMachineItem(string techType, float cost = 10.0f, CategoryType categoryType = CategoryType.None)
        {
            var tech = SMLHelper.V2.Handlers.TechTypeHandler.TryGetModdedTechType(techType, out TechType type);
            TechType = type;
            Cost = cost;
            CategoryType = categoryType;
        }

        internal void SetInfo(float cost = 10.0f, CategoryType categoryType = CategoryType.None)
        {
            Cost = cost;
            CategoryType = categoryType;
        }

        /*public static VendingMachineItem GetItemList(TechType techType)
        {
            VendingMachineItem item = new VendingMachineItem(techType);
            switch (techType)
            {
                case TechType.None:
                    break;
                case TechType.Quartz: item.SetInfo();// = new VendingMachineItem(techType, 5, CategoryType.Resources);
                    break;
                case TechType.ScrapMetal:
                    break;
                case TechType.FiberMesh:
                    item =  new VendingMachineItem(techType, 5, CategoryType.Resources);
                    break;
                    break;
                case TechType.LimestoneChunk:
                    break;
                //case TechType.CalciteOld:
                //    break;
                //case TechType.DolomiteOld:
                //    break;
                case TechType.Copper: return new VendingMachineItem(techType, 5, CategoryType.Resources);
                case TechType.Lead: return new VendingMachineItem(techType, 5, CategoryType.Resources);
                    break;
                case TechType.Salt:
                    break;
                case TechType.FlintOld:
                    break;
                case TechType.EmeryOld:
                    break;
                case TechType.MercuryOre:
                    break;
                case TechType.CalciumChunk:
                    break;
                case TechType.Placeholder:
                    break;
                case TechType.Glass:
                    break;
                case TechType.Titanium:
                    break;
                case TechType.Silicone:
                    break;
                case TechType.CarbonOld:
                    break;
                case TechType.EthanolOld:
                    break;
                case TechType.EthyleneOld:
                    break;
                case TechType.Gold:
                    break;
                case TechType.Magnesium:
                    break;
                case TechType.Sulphur:
                    break;
                case TechType.HydrogenOld:
                    break;
                case TechType.Lodestone:
                    break;
                case TechType.SandLoot:
                    break;
                case TechType.Bleach:
                    break;
                case TechType.Silver:
                    break;
                case TechType.BatteryAcidOld:
                    break;
                case TechType.TitaniumIngot:
                    break;
                case TechType.SandstoneChunk:
                    break;
                case TechType.CopperWire:
                    break;
                case TechType.WiringKit:
                    break;
                case TechType.AdvancedWiringKit:
                    break;
                case TechType.CrashPowder:
                    break;
                case TechType.Diamond:
                    break;
                case TechType.BasaltChunk:
                    break;
                case TechType.ShaleChunk:
                    break;
                case TechType.ObsidianChunk:
                    break;
                case TechType.Lithium:
                    break;
                case TechType.PlasteelIngot:
                    break;
                case TechType.EnameledGlass:
                    break;
                case TechType.PowerCell:
                    break;
                case TechType.ComputerChip:
                    break;
                case TechType.Fiber:
                    break;
                case TechType.Enamel:
                    break;
                case TechType.AcidOld:
                    break;
                case TechType.VesselOld:
                    break;
                case TechType.CombustibleOld:
                    break;
                case TechType.OpalGem:
                    break;
                case TechType.Uranium:
                    break;
                case TechType.AluminumOxide:
                    break;
                case TechType.HydrochloricAcid:
                    break;
                case TechType.Magnetite:
                    break;
                case TechType.AminoAcids:
                    break;
                case TechType.Polyaniline:
                    break;
                case TechType.AramidFibers:
                    break;
                case TechType.Graphene:
                    break;
                case TechType.Aerogel:
                    break;
                case TechType.Nanowires:
                    break;
                case TechType.Benzene:
                    break;
                case TechType.Lubricant:
                    break;
                case TechType.UraniniteCrystal:
                    break;
                case TechType.ReactorRod:
                    break;
                case TechType.DepletedReactorRod:
                    break;
                case TechType.PrecursorIonCrystal:
                    break;
                case TechType.PrecursorIonCrystalMatrix:
                    break;
                case TechType.Kyanite:
                    break;
                case TechType.Nickel:
                    break;
                case TechType.DrillableSalt:
                    break;
                case TechType.DrillableQuartz:
                    break;
                case TechType.DrillableCopper:
                    break;
                case TechType.DrillableTitanium:
                    break;
                case TechType.DrillableLead:
                    break;
                case TechType.DrillableSilver:
                    break;
                case TechType.DrillableDiamond:
                    break;
                case TechType.DrillableGold:
                    break;
                case TechType.DrillableMagnetite:
                    break;
                case TechType.DrillableLithium:
                    break;
                case TechType.DrillableMercury:
                    break;
                case TechType.DrillableUranium:
                    break;
                case TechType.DrillableAluminiumOxide:
                    break;
                case TechType.DrillableNickel:
                    break;
                case TechType.DrillableSulphur:
                    break;
                case TechType.DrillableKyanite:
                    break;
                case TechType.DiveSuit:
                    break;
                case TechType.ShipComputerOld:
                    break;
                case TechType.Fins:
                    break;
                case TechType.Tank:
                    break;
                case TechType.Battery:
                    break;
                case TechType.Knife:
                    break;
                case TechType.Drill:
                    break;
                case TechType.Flashlight:
                    break;
                case TechType.Beacon:
                    break;
                case TechType.Builder:
                    break;
                case TechType.PDA:
                    break;
                case TechType.EscapePod:
                    break;
                case TechType.Compass:
                    break;
                case TechType.AirBladder:
                    break;
                case TechType.Terraformer:
                    break;
                case TechType.Pipe:
                    break;
                case TechType.Thermometer:
                    break;
                case TechType.DiveReel:
                    break;
                case TechType.Rebreather:
                    break;
                case TechType.RadiationSuit:
                    break;
                case TechType.RadiationHelmet:
                    break;
                case TechType.RadiationGloves:
                    break;
                case TechType.ReinforcedDiveSuit:
                    break;
                case TechType.Scanner:
                    break;
                case TechType.FireExtinguisher:
                    break;
                case TechType.MapRoomHUDChip:
                    break;
                case TechType.PipeSurfaceFloater:
                    break;
                case TechType.CyclopsDecoy:
                    break;
                case TechType.DoubleTank:
                    break;
                case TechType.ReinforcedGloves:
                    break;
                case TechType.Welder:
                    break;
                case TechType.Seaglide:
                    break;
                case TechType.Constructor:
                    break;
                case TechType.Transfuser:
                    break;
                case TechType.Flare:
                    break;
                case TechType.StasisRifle:
                    break;
                case TechType.BuildBot:
                    break;
                case TechType.PropulsionCannon:
                    break;
                case TechType.Gravsphere:
                    break;
                case TechType.SmallStorage:
                    break;
                case TechType.StasisSphere:
                    break;
                case TechType.LaserCutter:
                    break;
                case TechType.LEDLight:
                    break;
                case TechType.DiamondBlade:
                    break;
                case TechType.HeatBlade:
                    break;
                case TechType.LithiumIonBattery:
                    break;
                case TechType.PlasteelTank:
                    break;
                case TechType.HighCapacityTank:
                    break;
                case TechType.UltraGlideFins:
                    break;
                case TechType.SwimChargeFins:
                    break;
                case TechType.RepulsionCannon:
                    break;
                case TechType.Stillsuit:
                    break;
                case TechType.PowerGlide:
                    break;
                case TechType.CompostCreepvine:
                    break;
                case TechType.ProcessUranium:
                    break;
                case TechType.PrecursorIonEnergyBlueprint:
                    break;
                case TechType.FabricatorBlueprintOld:
                    break;
                case TechType.ConstructorBlueprint:
                    break;
                case TechType.CyclopsBlueprint:
                    break;
                case TechType.FragmentAnalyzerBlueprintOld:
                    break;
                case TechType.LockerBlueprint:
                    break;
                case TechType.SpecialHullPlateBlueprintOld:
                    break;
                case TechType.BikemanHullPlateBlueprintOld:
                    break;
                case TechType.EatMyDictionHullPlateBlueprintOld:
                    break;
                case TechType.DevTestItemBlueprintOld:
                    break;
                case TechType.SeamothBlueprint:
                    break;
                case TechType.StasisRifleBlueprint:
                    break;
                case TechType.ExosuitBlueprint:
                    break;
                case TechType.TransfuserBlueprint:
                    break;
                case TechType.TerraformerBlueprint:
                    break;
                case TechType.ReinforceHullBlueprint:
                    break;
                case TechType.WorkbenchBlueprint:
                    break;
                case TechType.PropulsionCannonBlueprint:
                    break;
                case TechType.SpecimenAnalyzerBlueprint:
                    break;
                case TechType.BioreactorBlueprint:
                    break;
                case TechType.ThermalPlantBlueprint:
                    break;
                case TechType.NuclearReactorBlueprint:
                    break;
                case TechType.MoonpoolBlueprint:
                    break;
                case TechType.FiltrationMachineBlueprint:
                    break;
                case TechType.TechlightBlueprint:
                    break;
                case TechType.LEDLightBlueprint:
                    break;
                case TechType.CyclopsHullBlueprint:
                    break;
                case TechType.CyclopsBridgeBlueprint:
                    break;
                case TechType.CyclopsEngineBlueprint:
                    break;
                case TechType.CyclopsDockingBayBlueprint:
                    break;
                case TechType.SpotlightBlueprint:
                    break;
                case TechType.RadioBlueprint:
                    break;
                case TechType.StarshipCargoCrateBlueprint:
                    break;
                case TechType.StarshipCircuitBoxBlueprint:
                    break;
                case TechType.StarshipDeskBlueprint:
                    break;
                case TechType.StarshipChairBlueprint:
                    break;
                case TechType.StarshipMonitorBlueprint:
                    break;
                case TechType.SolarPanelBlueprint:
                    break;
                case TechType.PowerTransmitterBlueprint:
                    break;
                case TechType.BaseUpgradeConsoleBlueprint:
                    break;
                case TechType.BaseObservatoryBlueprint:
                    break;
                case TechType.BaseWaterParkBlueprint:
                    break;
                case TechType.PictureFrameBlueprint:
                    break;
                case TechType.BaseRoomBlueprint:
                    break;
                case TechType.BaseBulkheadBlueprint:
                    break;
                case TechType.SeaglideBlueprint:
                    break;
                case TechType.BatteryChargerBlueprint:
                    break;
                case TechType.PowerCellChargerBlueprint:
                    break;
                case TechType.FarmingTrayBlueprint:
                    break;
                case TechType.SignBlueprint:
                    break;
                case TechType.BenchBlueprint:
                    break;
                case TechType.PlanterPotBlueprint:
                    break;
                case TechType.PlanterBoxBlueprint:
                    break;
                case TechType.PlanterShelfBlueprint:
                    break;
                case TechType.AquariumBlueprint:
                    break;
                case TechType.ReinforcedDiveSuitBlueprint:
                    break;
                case TechType.RadiationSuitBlueprint:
                    break;
                case TechType.StillsuitBlueprint:
                    break;
                case TechType.ScannerRoomBlueprint:
                    break;
                case TechType.BasePlanterBlueprint:
                    break;
                case TechType.PlanterPot2Blueprint:
                    break;
                case TechType.PlanterPot3Blueprint:
                    break;
                case TechType.MedicalCabinetBlueprint:
                    break;
                case TechType.BaseMapRoomBlueprint:
                    break;
                case TechType.SeamothFragment:
                    break;
                case TechType.StasisRifleFragment:
                    break;
                case TechType.ExosuitFragment:
                    break;
                case TechType.TransfuserFragment:
                    break;
                case TechType.TerraformerFragment:
                    break;
                case TechType.ReinforceHullFragment:
                    break;
                case TechType.WorkbenchFragment:
                    break;
                case TechType.PropulsionCannonFragment:
                    break;
                case TechType.BioreactorFragment:
                    break;
                case TechType.ThermalPlantFragment:
                    break;
                case TechType.NuclearReactorFragment:
                    break;
                case TechType.MoonpoolFragment:
                    break;
                case TechType.BaseFiltrationMachineFragment:
                    break;
                case TechType.CyclopsHullFragment:
                    break;
                case TechType.CyclopsBridgeFragment:
                    break;
                case TechType.CyclopsEngineFragment:
                    break;
                case TechType.CyclopsDockingBayFragment:
                    break;
                case TechType.SeaglideFragment:
                    break;
                case TechType.ConstructorFragment:
                    break;
                case TechType.SolarPanelFragment:
                    break;
                case TechType.PowerTransmitterFragment:
                    break;
                case TechType.BaseUpgradeConsoleFragment:
                    break;
                case TechType.BaseObservatoryFragment:
                    break;
                case TechType.BaseWaterParkFragment:
                    break;
                case TechType.RadioFragment:
                    break;
                case TechType.BaseRoomFragment:
                    break;
                case TechType.BaseBulkheadFragment:
                    break;
                case TechType.BatteryChargerFragment:
                    break;
                case TechType.PowerCellChargerFragment:
                    break;
                case TechType.ScannerRoomFragment:
                    break;
                case TechType.SpecimenAnalyzerFragment:
                    break;
                case TechType.FarmingTrayFragment:
                    break;
                case TechType.SignFragment:
                    break;
                case TechType.PictureFrameFragment:
                    break;
                case TechType.BenchFragment:
                    break;
                case TechType.PlanterPotFragment:
                    break;
                case TechType.PlanterBoxFragment:
                    break;
                case TechType.PlanterShelfFragment:
                    break;
                case TechType.AquariumFragment:
                    break;
                case TechType.ReinforcedDiveSuitFragment:
                    break;
                case TechType.RadiationSuitFragment:
                    break;
                case TechType.StillsuitFragment:
                    break;
                case TechType.BuilderFragment:
                    break;
                case TechType.LEDLightFragment:
                    break;
                case TechType.TechlightFragment:
                    break;
                case TechType.SpotlightFragment:
                    break;
                case TechType.BaseMapRoomFragment:
                    break;
                case TechType.BaseBioReactorFragment:
                    break;
                case TechType.BaseNuclearReactorFragment:
                    break;
                case TechType.LaserCutterFragment:
                    break;
                case TechType.BeaconFragment:
                    break;
                case TechType.GravSphereFragment:
                    break;
                case TechType.SafeShallowsEgg:
                    break;
                case TechType.KelpForestEgg:
                    break;
                case TechType.GrassyPlateausEgg:
                    break;
                case TechType.GrandReefsEgg:
                    break;
                case TechType.MushroomForestEgg:
                    break;
                case TechType.KooshZoneEgg:
                    break;
                case TechType.TwistyBridgesEgg:
                    break;
                case TechType.LavaZoneEgg:
                    break;
                case TechType.StalkerEgg:
                    break;
                case TechType.ReefbackEgg:
                    break;
                case TechType.SpadefishEgg:
                    break;
                case TechType.RabbitrayEgg:
                    break;
                case TechType.MesmerEgg:
                    break;
                case TechType.JumperEgg:
                    break;
                case TechType.SandsharkEgg:
                    break;
                case TechType.JellyrayEgg:
                    break;
                case TechType.BonesharkEgg:
                    break;
                case TechType.CrabsnakeEgg:
                    break;
                case TechType.ShockerEgg:
                    break;
                case TechType.GasopodEgg:
                    break;
                case TechType.RabbitrayEggUndiscovered:
                    break;
                case TechType.JellyrayEggUndiscovered:
                    break;
                case TechType.StalkerEggUndiscovered:
                    break;
                case TechType.ReefbackEggUndiscovered:
                    break;
                case TechType.JumperEggUndiscovered:
                    break;
                case TechType.BonesharkEggUndiscovered:
                    break;
                case TechType.GasopodEggUndiscovered:
                    break;
                case TechType.MesmerEggUndiscovered:
                    break;
                case TechType.SandsharkEggUndiscovered:
                    break;
                case TechType.ShockerEggUndiscovered:
                    break;
                case TechType.GenericEgg:
                    break;
                case TechType.CrashEgg:
                    break;
                case TechType.CrashEggUndiscovered:
                    break;
                case TechType.CrabsquidEgg:
                    break;
                case TechType.CrabsquidEggUndiscovered:
                    break;
                case TechType.CutefishEgg:
                    break;
                case TechType.CutefishEggUndiscovered:
                    break;
                case TechType.LavaLizardEgg:
                    break;
                case TechType.LavaLizardEggUndiscovered:
                    break;
                case TechType.CrabsnakeEggUndiscovered:
                    break;
                case TechType.SpadefishEggUndiscovered:
                    break;
                case TechType.ReefbackShell:
                    break;
                case TechType.ReefbackTissue:
                    break;
                case TechType.ReefbackAdvancedStructure:
                    break;
                case TechType.ReefbackDNA:
                    break;
                case TechType.Workbench:
                    break;
                case TechType.HullReinforcementModule:
                    break;
                case TechType.Fabricator:
                    break;
                case TechType.Aquarium:
                    break;
                case TechType.Locker:
                    break;
                case TechType.Spotlight:
                    break;
                case TechType.DiveHatch:
                    break;
                case TechType.CurrentGenerator:
                    break;
                case TechType.FragmentAnalyzer:
                    break;
                case TechType.SpecialHullPlate:
                    break;
                case TechType.BikemanHullPlate:
                    break;
                case TechType.EatMyDictionHullPlate:
                    break;
                case TechType.DevTestItem:
                    break;
                case TechType.SpecimenAnalyzer:
                    break;
                case TechType.HullReinforcementModule2:
                    break;
                case TechType.HullReinforcementModule3:
                    break;
                case TechType.PowerUpgradeModule:
                    break;
                case TechType.SolarPanel:
                    break;
                case TechType.Sign:
                    break;
                case TechType.PowerTransmitter:
                    break;
                case TechType.Accumulator:
                    break;
                case TechType.Bioreactor:
                    break;
                case TechType.ThermalPlant:
                    break;
                case TechType.NuclearReactor:
                    break;
                case TechType.SmallLocker:
                    break;
                case TechType.Bench:
                    break;
                case TechType.PictureFrame:
                    break;
                case TechType.PlanterPot:
                    break;
                case TechType.PlanterBox:
                    break;
                case TechType.PlanterShelf:
                    break;
                case TechType.FarmingTray:
                    break;
                case TechType.FiltrationMachine:
                    break;
                case TechType.Techlight:
                    break;
                case TechType.Radio:
                    break;
                case TechType.PlanterPot2:
                    break;
                case TechType.PlanterPot3:
                    break;
                case TechType.MedicalCabinet:
                    break;
                case TechType.CyclopsHullModule1:
                    break;
                case TechType.CyclopsHullModule2:
                    break;
                case TechType.SingleWallShelf:
                    break;
                case TechType.WallShelves:
                    break;
                case TechType.Bed1:
                    break;
                case TechType.Bed2:
                    break;
                case TechType.NarrowBed:
                    break;
                case TechType.BatteryCharger:
                    break;
                case TechType.PowerCellCharger:
                    break;
                case TechType.Incubator:
                    break;
                case TechType.HatchingEnzymes:
                    break;
                case TechType.EnyzmeCloud:
                    break;
                case TechType.EnzymeCureBall:
                    break;
                case TechType.Centrifuge:
                    break;
                case TechType.CyclopsShieldModule:
                    break;
                case TechType.CyclopsSonarModule:
                    break;
                case TechType.CyclopsSeamothRepairModule:
                    break;
                case TechType.CyclopsDecoyModule:
                    break;
                case TechType.CyclopsFireSuppressionModule:
                    break;
                case TechType.CyclopsFabricator:
                    break;
                case TechType.CyclopsThermalReactorModule:
                    break;
                case TechType.CyclopsHullModule3:
                    break;
                case TechType.StarshipCargoCrate:
                    break;
                case TechType.StarshipCircuitBox:
                    break;
                case TechType.StarshipDesk:
                    break;
                case TechType.StarshipChair:
                    break;
                case TechType.StarshipMonitor:
                    break;
                case TechType.StarshipChair2:
                    break;
                case TechType.StarshipChair3:
                    break;
                case TechType.LuggageBag:
                    break;
                case TechType.ArcadeGorgetoy:
                    break;
                case TechType.LabEquipment1:
                    break;
                case TechType.LabEquipment2:
                    break;
                case TechType.LabEquipment3:
                    break;
                case TechType.CoffeeVendingMachine:
                    break;
                case TechType.BarTable:
                    break;
                case TechType.Cap1:
                    break;
                case TechType.Cap2:
                    break;
                case TechType.LabContainer:
                    break;
                case TechType.LabContainer2:
                    break;
                case TechType.LabContainer3:
                    break;
                case TechType.Trashcans:
                    break;
                case TechType.LabTrashcan:
                    break;
                case TechType.VendingMachine:
                    break;
                case TechType.LabCounter:
                    break;
                case TechType.StarshipSouvenir:
                    break;
                case TechType.PosterAurora:
                    break;
                case TechType.PosterExoSuit1:
                    break;
                case TechType.PosterExoSuit2:
                    break;
                case TechType.PosterKitty:
                    break;
                case TechType.ToyCar:
                    break;
                case TechType.Seamoth:
                    break;
                case TechType.Exosuit:
                    break;
                case TechType.CrashedShip:
                    break;
                case TechType.Cyclops:
                    break;
                case TechType.Audiolog:
                    break;
                case TechType.Signal:
                    break;
                case TechType.SeamothReinforcementModule:
                    break;
                case TechType.VehiclePowerUpgradeModule:
                    break;
                case TechType.SeamothSolarCharge:
                    break;
                case TechType.VehicleStorageModule:
                    break;
                case TechType.SeamothElectricalDefense:
                    break;
                case TechType.VehicleArmorPlating:
                    break;
                case TechType.LootSensorMetal:
                    break;
                case TechType.LootSensorLithium:
                    break;
                case TechType.LootSensorFragment:
                    break;
                case TechType.SeamothTorpedoModule:
                    break;
                case TechType.SeamothSonarModule:
                    break;
                case TechType.WhirlpoolTorpedo:
                    break;
                case TechType.VehicleHullModule1:
                    break;
                case TechType.VehicleHullModule2:
                    break;
                case TechType.VehicleHullModule3:
                    break;
                case TechType.ExosuitJetUpgradeModule:
                    break;
                case TechType.ExosuitDrillArmModule:
                    break;
                case TechType.ExosuitThermalReactorModule:
                    break;
                case TechType.ExosuitClawArmModule:
                    break;
                case TechType.GasTorpedo:
                    break;
                case TechType.ExosuitPropulsionArmModule:
                    break;
                case TechType.ExosuitGrapplingArmModule:
                    break;
                case TechType.ExosuitTorpedoArmModule:
                    break;
                case TechType.ExosuitDrillArmFragment:
                    break;
                case TechType.ExosuitPropulsionArmFragment:
                    break;
                case TechType.ExosuitGrapplingArmFragment:
                    break;
                case TechType.ExosuitTorpedoArmFragment:
                    break;
                case TechType.ExosuitClawArmFragment:
                    break;
                case TechType.ExoHullModule1:
                    break;
                case TechType.ExoHullModule2:
                    break;
                case TechType.MapRoomUpgradeScanRange:
                    break;
                case TechType.MapRoomUpgradeScanSpeed:
                    break;
                case TechType.Creepvine:
                    break;
                case TechType.HoleFish:
                    break;
                case TechType.Jumper:
                    break;
                case TechType.CreepvineSeedCluster:
                    break;
                case TechType.Peeper:
                    break;
                case TechType.Oculus:
                    break;
                case TechType.RabbitRay:
                    break;
                case TechType.GarryFish:
                    break;
                case TechType.Slime:
                    break;
                case TechType.Crash:
                    break;
                case TechType.Boomerang:
                    break;
                case TechType.LavaLarva:
                    break;
                case TechType.Stalker:
                    break;
                case TechType.Eyeye:
                    break;
                case TechType.Bloom:
                    break;
                case TechType.Bladderfish:
                    break;
                case TechType.Hoverfish:
                    break;
                case TechType.Jellyray:
                    break;
                case TechType.Reefback:
                    break;
                case TechType.Reginald:
                    break;
                case TechType.Spadefish:
                    break;
                case TechType.Grabcrab:
                    break;
                case TechType.Floater:
                    break;
                case TechType.Gasopod:
                    break;
                case TechType.Sandshark:
                    break;
                case TechType.Player:
                    break;
                case TechType.Bleeder:
                    break;
                case TechType.Rockgrub:
                    break;
                case TechType.CrashHome:
                    break;
                case TechType.CreepvinePiece:
                    break;
                case TechType.GasPod:
                    break;
                case TechType.Hoopfish:
                    break;
                case TechType.HoopfishSchool:
                    break;
                case TechType.RockPuncher:
                    break;
                case TechType.BoneShark:
                    break;
                case TechType.Mesmer:
                    break;
                case TechType.SeaTreader:
                    break;
                case TechType.SeaEmperor:
                    break;
                case TechType.Cutefish:
                    break;
                case TechType.Crabsnake:
                    break;
                case TechType.ReaperLeviathan:
                    break;
                case TechType.CaveCrawler:
                    break;
                case TechType.Skyray:
                    break;
                case TechType.Biter:
                    break;
                case TechType.SkyrayNonRoosting:
                    break;
                case TechType.Shocker:
                    break;
                case TechType.Spinefish:
                    break;
                case TechType.Shuttlebug:
                    break;
                case TechType.Blighter:
                    break;
                case TechType.Warper:
                    break;
                case TechType.CrabSquid:
                    break;
                case TechType.LavaLizard:
                    break;
                case TechType.SpineEel:
                    break;
                case TechType.SeaDragon:
                    break;
                case TechType.LavaBoomerang:
                    break;
                case TechType.LavaEyeye:
                    break;
                case TechType.SeaEmperorBaby:
                    break;
                case TechType.WarperSpawner:
                    break;
                case TechType.GhostRayBlue:
                    break;
                case TechType.GhostRayRed:
                    break;
                case TechType.ReefbackBaby:
                    break;
                case TechType.PrecursorDroid:
                    break;
                case TechType.GhostLeviathan:
                    break;
                case TechType.SeaEmperorLeviathan:
                    break;
                case TechType.SeaEmperorJuvenile:
                    break;
                case TechType.GhostLeviathanJuvenile:
                    break;
                case TechType.HoleFishAnalysis:
                    break;
                case TechType.PeeperAnalysis:
                    break;
                case TechType.BladderfishAnalysis:
                    break;
                case TechType.GarryFishAnalysis:
                    break;
                case TechType.HoverfishAnalysis:
                    break;
                case TechType.ReginaldAnalysis:
                    break;
                case TechType.SpadefishAnalysis:
                    break;
                case TechType.BoomerangAnalysis:
                    break;
                case TechType.EyeyeAnalysis:
                    break;
                case TechType.OculusAnalysis:
                    break;
                case TechType.HoopfishAnalysis:
                    break;
                case TechType.AnalysisTreeOld:
                    break;
                case TechType.SpinefishAnalysis:
                    break;
                case TechType.PlantPlaceholder:
                    break;
                case TechType.BallClusters:
                    break;
                case TechType.BarnacleSuckers:
                    break;
                case TechType.BlueBarnacle:
                    break;
                case TechType.BlueBarnacleCluster:
                    break;
                case TechType.BlueCoralTubes:
                    break;
                case TechType.RedGrass:
                    break;
                case TechType.GreenGrass:
                    break;
                case TechType.Mohawk:
                    break;
                case TechType.GreenReeds:
                    break;
                case TechType.JellyPlant:
                    break;
                case TechType.BlueJeweledDisk:
                    break;
                case TechType.GreenJeweledDisk:
                    break;
                case TechType.PurpleJeweledDisk:
                    break;
                case TechType.RedJeweledDisk:
                    break;
                case TechType.SmallKoosh:
                    break;
                case TechType.MediumKoosh:
                    break;
                case TechType.LargeKoosh:
                    break;
                case TechType.HugeKoosh:
                    break;
                case TechType.MembrainTree:
                    break;
                case TechType.PurpleFan:
                    break;
                case TechType.AcidMushroom:
                    break;
                case TechType.PurpleTentacle:
                    break;
                case TechType.RedSeaweed:
                    break;
                case TechType.CoralOldPlaceholder:
                    break;
                case TechType.CoralShellPlate:
                    break;
                case TechType.SmallFan:
                    break;
                case TechType.SmallFanCluster:
                    break;
                case TechType.BigCoralTubes:
                    break;
                case TechType.TreeMushroom:
                    break;
                case TechType.BlueCluster:
                    break;
                case TechType.BrownTubes:
                    break;
                case TechType.BloodGrass:
                    break;
                case TechType.HeatArea:
                    break;
                case TechType.BloodOil:
                    break;
                case TechType.WhiteMushroom:
                    break;
                case TechType.BloodRoot:
                    break;
                case TechType.BloodVine:
                    break;
                case TechType.PinkFlower:
                    break;
                case TechType.PinkMushroom:
                    break;
                case TechType.PurpleRattle:
                    break;
                case TechType.BulboTree:
                    break;
                case TechType.PurpleVasePlant:
                    break;
                case TechType.OrangeMushroom:
                    break;
                case TechType.FernPalm:
                    break;
                case TechType.HangingFruitTree:
                    break;
                case TechType.PurpleVegetablePlant:
                    break;
                case TechType.MelonPlant:
                    break;
                case TechType.BluePalm:
                    break;
                case TechType.GabeSFeather:
                    break;
                case TechType.SeaCrown:
                    break;
                case TechType.OrangePetalsPlant:
                    break;
                case TechType.EyesPlant:
                    break;
                case TechType.RedGreenTentacle:
                    break;
                case TechType.PurpleStalk:
                    break;
                case TechType.RedBasketPlant:
                    break;
                case TechType.RedBush:
                    break;
                case TechType.RedConePlant:
                    break;
                case TechType.ShellGrass:
                    break;
                case TechType.SpottedLeavesPlant:
                    break;
                case TechType.RedRollPlant:
                    break;
                case TechType.PurpleBranches:
                    break;
                case TechType.SnakeMushroom:
                    break;
                case TechType.SeaTreaderPoop:
                    break;
                case TechType.GenericJeweledDisk:
                    break;
                case TechType.FloatingStone:
                    break;
                case TechType.BlueAmoeba:
                    break;
                case TechType.RedTipRockThings:
                    break;
                case TechType.BlueTipLostRiverPlant:
                    break;
                case TechType.BlueLostRiverLilly:
                    break;
                case TechType.LargeFloater:
                    break;
                case TechType.PiecePlaceholder:
                    break;
                case TechType.JeweledDiskPiece:
                    break;
                case TechType.CoralChunk:
                    break;
                case TechType.KooshChunk:
                    break;
                case TechType.StalkerTooth:
                    break;
                case TechType.TreeMushroomPiece:
                    break;
                case TechType.BulboTreePiece:
                    break;
                case TechType.OrangeMushroomSpore:
                    break;
                case TechType.PurpleVasePlantSeed:
                    break;
                case TechType.AcidMushroomSpore:
                    break;
                case TechType.WhiteMushroomSpore:
                    break;
                case TechType.PinkMushroomSpore:
                    break;
                case TechType.PurpleRattleSpore:
                    break;
                case TechType.HangingFruit:
                    break;
                case TechType.PurpleVegetable:
                    break;
                case TechType.SmallMelon:
                    break;
                case TechType.Melon:
                    break;
                case TechType.MelonSeed:
                    break;
                case TechType.PurpleBrainCoralPiece:
                    break;
                case TechType.SpikePlantSeed:
                    break;
                case TechType.BluePalmSeed:
                    break;
                case TechType.PurpleFanSeed:
                    break;
                case TechType.SmallFanSeed:
                    break;
                case TechType.PurpleTentacleSeed:
                    break;
                case TechType.JellyPlantSeed:
                    break;
                case TechType.GabeSFeatherSeed:
                    break;
                case TechType.SeaCrownSeed:
                    break;
                case TechType.MembrainTreeSeed:
                    break;
                case TechType.PinkFlowerSeed:
                    break;
                case TechType.FernPalmSeed:
                    break;
                case TechType.OrangePetalsPlantSeed:
                    break;
                case TechType.EyesPlantSeed:
                    break;
                case TechType.RedGreenTentacleSeed:
                    break;
                case TechType.PurpleStalkSeed:
                    break;
                case TechType.RedBasketPlantSeed:
                    break;
                case TechType.RedBushSeed:
                    break;
                case TechType.RedConePlantSeed:
                    break;
                case TechType.ShellGrassSeed:
                    break;
                case TechType.SpottedLeavesPlantSeed:
                    break;
                case TechType.RedRollPlantSeed:
                    break;
                case TechType.PurpleBranchesSeed:
                    break;
                case TechType.SnakeMushroomSpore:
                    break;
                case TechType.EnvironmentPlaceholder:
                    break;
                case TechType.Boulder:
                    break;
                case TechType.PurpleBrainCoral:
                    break;
                case TechType.HangingStinger:
                    break;
                case TechType.SpikePlant:
                    break;
                case TechType.BrainCoral:
                    break;
                case TechType.CoveTree:
                    break;
                case TechType.MonsterSkeleton:
                    break;
                case TechType.SeaDragonSkeleton:
                    break;
                case TechType.ReaperSkeleton:
                    break;
                case TechType.CaveSkeleton:
                    break;
                case TechType.HugeSkeleton:
                    break;
                case TechType.PrecursorKey_Red:
                    break;
                case TechType.PrecursorKey_Blue:
                    break;
                case TechType.PrecursorKey_Orange:
                    break;
                case TechType.PrecursorKey_White:
                    break;
                case TechType.PrecursorKey_Purple:
                    break;
                case TechType.PrecursorKey_PurpleFragment:
                    break;
                case TechType.PrecursorKeyTerminal:
                    break;
                case TechType.PrecursorTeleporter:
                    break;
                case TechType.PrecursorEnergyCore:
                    break;
                case TechType.PrecursorIonPowerCell:
                    break;
                case TechType.PrecursorIonBattery:
                    break;
                case TechType.PrecursorThermalPlant:
                    break;
                case TechType.PrecursorWarper:
                    break;
                case TechType.PrecursorFishSkeleton:
                    break;
                case TechType.PrecursorScanner:
                    break;
                case TechType.PrecursorLabCacheContainer1:
                    break;
                case TechType.PrecursorLabCacheContainer2:
                    break;
                case TechType.PrecursorLabTable:
                    break;
                case TechType.PrecursorSeaDragonSkeleton:
                    break;
                case TechType.PrecursorSensor:
                    break;
                case TechType.PrecursorPrisonArtifact1:
                    break;
                case TechType.PrecursorPrisonArtifact2:
                    break;
                case TechType.PrecursorPrisonArtifact3:
                    break;
                case TechType.PrecursorPrisonArtifact4:
                    break;
                case TechType.PrecursorPrisonArtifact5:
                    break;
                case TechType.PrecursorPrisonArtifact6:
                    break;
                case TechType.PrecursorPrisonArtifact7:
                    break;
                case TechType.PrecursorPrisonArtifact8:
                    break;
                case TechType.PrecursorPrisonArtifact9:
                    break;
                case TechType.PrecursorPrisonArtifact10:
                    break;
                case TechType.PrecursorPrisonArtifact11:
                    break;
                case TechType.PrecursorPrisonArtifact12:
                    break;
                case TechType.PrecursorPipeRoomIncomingPipe:
                    break;
                case TechType.PrecursorPipeRoomOutgoingPipe:
                    break;
                case TechType.PrecursorPrisonLabEmperorFetus:
                    break;
                case TechType.PrecursorPrisonLabEmperorEgg:
                    break;
                case TechType.PrecursorPrisonAquariumPipe:
                    break;
                case TechType.PrecursorPrisonAquariumFinalTeleporter:
                    break;
                case TechType.PrecursorPrisonAquariumIncubatorEggs:
                    break;
                case TechType.PrecursorPrisonAquariumIncubator:
                    break;
                case TechType.PrecursorSurfacePipe:
                    break;
                case TechType.PrecursorPrisonArtifact13:
                    break;
                case TechType.PrecursorPrisonIonGenerator:
                    break;
                case TechType.PrecursorPrisonOutposts:
                    break;
                case TechType.ObservatoryOld:
                    break;
                case TechType.PrecursorLostRiverBrokenAnchor:
                    break;
                case TechType.PrecursorLostRiverLabRays:
                    break;
                case TechType.PrecursorLostRiverLabBones:
                    break;
                case TechType.PrecursorLostRiverLabEgg:
                    break;
                case TechType.PrecursorLostRiverProductionLine:
                    break;
                case TechType.PrecursorLostRiverWarperParts:
                    break;
                case TechType.FilteredWater:
                    break;
                case TechType.DisinfectedWater:
                    break;
                case TechType.CookedPeeper:
                    break;
                case TechType.CookedHoleFish:
                    break;
                case TechType.CookedGarryFish:
                    break;
                case TechType.CookedReginald:
                    break;
                case TechType.CookedBladderfish:
                    break;
                case TechType.CookedHoverfish:
                    break;
                case TechType.CookedSpadefish:
                    break;
                case TechType.CookedBoomerang:
                    break;
                case TechType.CookedEyeye:
                    break;
                case TechType.CookedOculus:
                    break;
                case TechType.CookedHoopfish:
                    break;
                case TechType.NutrientBlock:
                    break;
                case TechType.FirstAidKit:
                    break;
                case TechType.StillsuitWater:
                    break;
                case TechType.BigFilteredWater:
                    break;
                case TechType.CookedSpinefish:
                    break;
                case TechType.CookedLavaEyeye:
                    break;
                case TechType.CookedLavaBoomerang:
                    break;
                case TechType.Snack1:
                    break;
                case TechType.Snack2:
                    break;
                case TechType.Snack3:
                    break;
                case TechType.Coffee:
                    break;
                case TechType.CuredPeeper:
                    break;
                case TechType.CuredHoleFish:
                    break;
                case TechType.CuredGarryFish:
                    break;
                case TechType.CuredReginald:
                    break;
                case TechType.CuredBladderfish:
                    break;
                case TechType.CuredHoverfish:
                    break;
                case TechType.CuredSpadefish:
                    break;
                case TechType.CuredBoomerang:
                    break;
                case TechType.CuredEyeye:
                    break;
                case TechType.CuredOculus:
                    break;
                case TechType.CuredHoopfish:
                    break;
                case TechType.CuredSpinefish:
                    break;
                case TechType.CuredLavaEyeye:
                    break;
                case TechType.CuredLavaBoomerang:
                    break;
                case TechType.MembraneOld:
                    break;
                case TechType.Unobtanium:
                    break;
                case TechType.BaseRoom:
                    break;
                case TechType.BaseHatch:
                    break;
                case TechType.BaseWall:
                    break;
                case TechType.BaseDoor:
                    break;
                case TechType.BaseLadder:
                    break;
                case TechType.BaseWindow:
                    break;
                case TechType.PowerGeneratorOld:
                    break;
                case TechType.UnusedOld:
                    break;
                case TechType.BaseCorridor:
                    break;
                case TechType.BaseFoundation:
                    break;
                case TechType.BaseCorridorI:
                    break;
                case TechType.BaseCorridorL:
                    break;
                case TechType.BaseCorridorT:
                    break;
                case TechType.BaseCorridorX:
                    break;
                case TechType.BaseReinforcement:
                    break;
                case TechType.BaseBulkhead:
                    break;
                case TechType.BaseCorridorGlassI:
                    break;
                case TechType.BaseCorridorGlassL:
                    break;
                case TechType.BaseObservatory:
                    break;
                case TechType.BaseConnector:
                    break;
                case TechType.BaseMoonpool:
                    break;
                case TechType.BaseCorridorGlass:
                    break;
                case TechType.BaseUpgradeConsole:
                    break;
                case TechType.BasePlanter:
                    break;
                case TechType.BaseFiltrationMachine:
                    break;
                case TechType.BaseWaterPark:
                    break;
                case TechType.BaseMapRoom:
                    break;
                case TechType.MapRoomCamera:
                    break;
                case TechType.BaseBioReactor:
                    break;
                case TechType.BaseNuclearReactor:
                    break;
                case TechType.BasePipeConnector:
                    break;
                case TechType.RocketBase:
                    break;
                case TechType.RocketBaseLadder:
                    break;
                case TechType.RocketStage1:
                    break;
                case TechType.RocketStage2:
                    break;
                case TechType.RocketStage3:
                    break;
                case TechType.TimeCapsule:
                    break;
                case TechType.DioramaHullPlate:
                    break;
                case TechType.MarkiplierHullPlate:
                    break;
                case TechType.MuyskermHullPlate:
                    break;
                case TechType.LordMinionHullPlate:
                    break;
                case TechType.JackSepticEyeHullPlate:
                    break;
                case TechType.Poster:
                    break;
                case TechType.IGPHullPlate:
                    break;
                case TechType.GilathissHullPlate:
                    break;
                case TechType.Marki1:
                    break;
                case TechType.Marki2:
                    break;
                case TechType.JackSepticEye:
                    break;
                case TechType.EatMyDiction:
                    break;
                case TechType.RadiationLeakPoint:
                    break;
                case TechType.SomethingPlaceholder:
                    break;
                case TechType.Fragment:
                    break;
                case TechType.Wreck:
                    break;
                case TechType.CountOld:
                    break;
                case TechType.Databox:
                    break;
                default:
                    break;
            }
        }*/
    }
    public enum CategoryType
    {
        None, Resources, Tech, Tools, Food, Weapon, Fish, Eggs, Seed, Gardening
    }
}
