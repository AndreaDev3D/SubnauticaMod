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
    public class ResourceUploader
    {
        public PrefabInfo PrefabInfo { get; }

        public ResourceUploader()
        {
            PrefabInfo = PrefabInfo
                .WithTechType("AD3D_ResourceUploader", "Resource Uploader", "Automatically uploads items to filtered storage on close.", unlockAtStart: true)
                .WithIcon(ImageUtils.LoadSpriteFromTexture(Plugin.AssetBundle.LoadAsset<Texture2D>("ResourceUploader.png")));
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
                    new Ingredient(TechType.Titanium, 2),
                    new Ingredient(TechType.Copper, 1),
                },
            };

            customPrefab.SetRecipe(recipe)
                .WithFabricatorType(CraftTree.Type.Constructor)
                .WithStepsToFabricatorTab("Interior Modules");

            customPrefab.SetEquipment(EquipmentType.Hand);
            customPrefab.SetPdaGroupCategory(TechGroup.InteriorModules, TechCategory.InteriorModule);

            customPrefab.Register();
        }

        private GameObject GetAssetBundlePrefab()
        {
            var prefab = Plugin.AssetBundle.LoadAsset<GameObject>("ResourceUploader.prefab");
            PrefabUtils.AddBasicComponents(prefab, PrefabInfo.ClassID, PrefabInfo.TechType, LargeWorldEntity.CellLevel.Medium);
            MaterialUtils.ApplySNShaders(prefab);

            SetupConstructable(prefab);
            SetupStorage(prefab);

            foreach (var rb in prefab.GetComponentsInChildren<Rigidbody>(true))
                UnityEngine.Object.DestroyImmediate(rb);

            return prefab;
        }

        private void SetupConstructable(GameObject prefab)
        {
            var rootModel = prefab.SearchChild("model");
            var constructable = PrefabUtils.AddConstructable(prefab, PrefabInfo.TechType, ConstructableFlags.Inside, rootModel);
            constructable.allowedOnConstructables = true;
            constructable.allowedOnGround = false;
            constructable.allowedOnWall = true;
            constructable.allowedOutside = true;
            constructable.allowedInSub = true;
            constructable.deconstructionAllowed = true;
            constructable.forceUpright = false;
            constructable.rotationEnabled = true;
        }

        private void SetupStorage(GameObject prefab)
        {
            var wasActive = prefab.activeSelf;
            if (wasActive) prefab.SetActive(false);

            var storageRoot = prefab.FindChild("StorageRoot");
            var childObjectIdentifier = storageRoot.AddComponent<ChildObjectIdentifier>();
            childObjectIdentifier.ClassId = $"{PrefabInfo.ClassID}Container";

            var container = prefab.AddComponent<StorageContainer>();
            container.prefabRoot = prefab;
            container.width = 4;
            container.height = 4;
            container.storageRoot = childObjectIdentifier;
            container.preventDeconstructionIfNotEmpty = true;
            container.hoverText = "Deposit to Uploader";

            prefab.AddComponent<ResourceUploaderController>().CopyComponent(container);
            
            UnityEngine.Object.DestroyImmediate(container);

            if (wasActive) prefab.SetActive(true);
        }
    }
}
