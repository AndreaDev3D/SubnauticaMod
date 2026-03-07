using AD3D_StorageSolution.Runtime;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Extensions;
using Nautilus.Utility;
using UnityEngine;

#if SN
using static CraftData;
#endif

namespace AD3D_StorageSolution.Items.Storage
{
    public class StorageMonitor
    {
        public PrefabInfo PrefabInfo { get; }

        private string shortDescription = "";

        public StorageMonitor()
        {
            PrefabInfo = PrefabInfo
            .WithTechType("AD3D_StorageMonitor", "Wall Storage Monitor", shortDescription, unlockAtStart: true)
            .WithIcon(ImageUtils.LoadSpriteFromTexture(Plugin.AssetBundle.LoadAsset<Texture2D>($"StorageMonitor.png")));
        }

        public void Register()
        {
            var customPrefab = new CustomPrefab(PrefabInfo);

            customPrefab.SetGameObject(GetAssetBundlePrefab());

            var recipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients =
                {
                    new Ingredient(TechType.Titanium, 1),
                    //new Ingredient(TechType.Quartz, 2),
                    //new Ingredient(TechType.Copper, 2),
                },
            };

            customPrefab.SetRecipe(recipe)
                .WithFabricatorType(CraftTree.Type.Constructor)
                .WithStepsToFabricatorTab("Interior Modules");

            customPrefab.SetEquipment(EquipmentType.Hand);
            customPrefab.SetPdaGroupCategory(TechGroup.InteriorModules, TechCategory.InteriorModule);

            customPrefab.Register();
            Plugin.Logger.LogInfo($"StorageMonitor is Registered!");
        }

        private GameObject GetAssetBundlePrefab()
        {
            var prefab = Plugin.AssetBundle.LoadAsset<GameObject>($"StorageMonitor.prefab");
            var prefabItem = Plugin.AssetBundle.LoadAsset<GameObject>($"ItemButtonPrefab.prefab");
            PrefabUtils.AddBasicComponents(prefab, PrefabInfo.ClassID, PrefabInfo.TechType, LargeWorldEntity.CellLevel.Medium);
            MaterialUtils.ApplySNShaders(prefab);

            SetupConstructable(prefab);
            SetupStorageMonitor(prefab, prefabItem);

            return prefab;
        }

        private void SetupConstructable(GameObject prefab)
        {
            var rootModel = prefab.SearchChild("model");
            var constructable = PrefabUtils.AddConstructable(prefab, PrefabInfo.TechType, ConstructableFlags.Inside, rootModel);
            constructable.allowedOnConstructables = true;
            constructable.allowedOnGround = false;
            constructable.allowedOnWall = true;
            constructable.allowedOutside = false;
            constructable.allowedInSub = true;
            constructable.deconstructionAllowed = true;
            constructable.forceUpright = true;
            constructable.rotationEnabled = true;
        }

        private void SetupStorageMonitor(GameObject storageMonitor, GameObject buttonPrefab)
        {
            var storageMonitorController = storageMonitor.AddComponent<StorageMonitorController>();
            storageMonitorController.Init(buttonPrefab);
        }
    }
}
