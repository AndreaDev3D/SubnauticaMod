using AD3D_HabitatSolutionMod.BO;
using AD3D_HabitatSolutionMod.BO.Base;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;

namespace AD3D_HabitatSolutionMod
{
    [QModCore]
    public class QPatch
    {
        public const string _AssetName = "habitatasset";
        public const string _ModName = "AD3D_HabitatSolutionMod";
        public const string _Mod_Version = "1.0.0";

        internal static HabitatTest HabitatTest { get; } = new HabitatTest();
        internal static AlterraVendingMachine AlterraVendingMachine { get; } = new AlterraVendingMachine();
        internal static AutoBreederTank AutoBreederTank { get; } = new AutoBreederTank();

        internal static AdamantioBlade AdamantioBlade { get; } = new AdamantioBlade();

        internal static SeaCrosser SeaCrosser { get; } = new SeaCrosser();

        internal static FoodableItem Bar1 { get; set; }

        [QModPatch]
        public static void Patch()
        {
            //Helper.LoadConfig();

            HabitatTest.Patch();
            AlterraVendingMachine.Patch();
            AutoBreederTank.Patch();

            AdamantioBlade.Patch();
            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, AdamantioBlade.TechType, "Personal", "Tools");

            // Food
            Bar1 = new FoodableItem("Bar1", "Bar1", "Alterra Nutrient Bar");
            Bar1.FoodValue = 75;
            Bar1.WaterValue = 75;
            Bar1.Patch();
            // Veichle
            SeaCrosser.Patch();

            AD3D_Common.Helper.Log($"Patched successfully [{_Mod_Version}]");
        }
    }
}