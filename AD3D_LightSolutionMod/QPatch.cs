using AD3D_LightSolutionMod.BO.Base;
using AD3D_LightSolutionMod.BO.Config;
using QModManager.API.ModLoading;

namespace AD3D_LightSolutionMod
{
    [QModCore]
    public class QPatch
    {
        //internal static LightSolutionConfig Config { get; set; } = new LightSolutionConfig();
        internal static DatabaseConfig Database { get; set; } = new DatabaseConfig();

        internal static LightSwitch LightSwitch { get; } = new LightSwitch();
        internal static LightSourceItem LightSource_1;
        internal static LightSourceItem LightSource_2;
        internal static LightSourceItem LightSource_3;
        internal static LightSourceItem LightSource_4;
        internal static LightSourceItem LightSource_5;
        internal static LightSourceItem LightSource_6;
        internal static LightSourceItem LightSource_7;
        internal static LightSourceItem LightSource_8;

        [QModPatch]
        public static void Patch()
        {
            //Config.Load();
            Database.Load();

            LightSource_1 = new LightSourceItem("LightSource_1", "LightSource_1", LightSourceItemType.Roof);
            LightSource_2 = new LightSourceItem("LightSource_2", "LightSource_2", LightSourceItemType.Roof);
            LightSource_3 = new LightSourceItem("LightSource_3", "LightSource_3", LightSourceItemType.Wall);
            LightSource_4 = new LightSourceItem("LightSource_4", "LightSource_4", LightSourceItemType.Wall);
            LightSource_5 = new LightSourceItem("LightSource_5", "LightSource_5", LightSourceItemType.Floor);
            LightSource_6 = new LightSourceItem("LightSource_6", "LightSource_6", LightSourceItemType.Floor);
            LightSource_7 = new LightSourceItem("LightSource_7", "LightSource_7", LightSourceItemType.Floor);
            LightSource_8 = new LightSourceItem("LightSource_8", "LightSource_8", LightSourceItemType.Wall);

            LightSwitch.Patch();
            LightSource_1.Patch();
            LightSource_2.Patch();
            LightSource_3.Patch();
            LightSource_4.Patch();
            LightSource_5.Patch();
            LightSource_6.Patch();
            LightSource_7.Patch();
            LightSource_8.Patch();

            //CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, DeepEngineKit.TechType, "Energy Solution");

            //Add the databank entry.
            //LanguageHandler.SetLanguageLine($"Ency_{DeepEngine.ClassID}", DeepEngine.FriendlyName);
            //LanguageHandler.SetLanguageLine($"EncyDesc_{DeepEngine.ClassID}", DeepEngine.PDADescription(Config.MaxPowerAllowed));

            AD3D_Common.Helper.Log($"Patched successfully [v1.0.0]");
        }
    }
}