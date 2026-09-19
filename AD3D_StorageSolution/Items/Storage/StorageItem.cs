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
    public class StorageItem
    {
        public Vector2Int StorageSize { get; }

        public bool IsOnFloor;

        public PrefabInfo PrefabInfo { get; }

        private string _displayName;

        public StorageItem(string classID, string friendlyName, string shortDescription, Vector2Int storageSize, bool isOnFloor)
        {
            StorageSize = storageSize;
            IsOnFloor = isOnFloor;
            _displayName = friendlyName;

            PrefabInfo = PrefabInfo
            .WithTechType(classID, friendlyName, shortDescription, unlockAtStart: true)
            .WithIcon(ImageUtils.LoadSpriteFromTexture(Plugin.AssetBundle.LoadAsset<Texture2D>($"{classID}.png")));
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
                    new Ingredient(TechType.Titanium, 4),
                    new Ingredient(TechType.Quartz, 2),
                    new Ingredient(TechType.Copper, 2),
                },
            };

            customPrefab.SetRecipe(recipe);

            customPrefab.SetEquipment(EquipmentType.Hand);
            customPrefab.SetPdaGroupCategory(TechGroup.InteriorModules, TechCategory.InteriorModule);

            customPrefab.Register();
        }

        private GameObject GetAssetBundlePrefab()
        {
            var prefab = Plugin.AssetBundle.LoadAsset<GameObject>($"{PrefabInfo.ClassID}.prefab");
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
            constructable.allowedOnGround = IsOnFloor;
            constructable.allowedOnWall = !IsOnFloor;
            constructable.allowedOutside = false;
            constructable.allowedInSub = true;
            constructable.deconstructionAllowed = true;
            constructable.forceUpright = IsOnFloor;
            constructable.rotationEnabled = true;
#if SN
            constructable.ExcludeFromSubParentRigidbody();
#endif
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
            container.width = StorageSize.x;
            container.height = StorageSize.y;
            container.storageRoot = childObjectIdentifier;
            container.preventDeconstructionIfNotEmpty = true;
            container.hoverText = $"Open {_displayName}";

            prefab.AddComponent<StorageController>().CopyComponent(container);
            
            UnityEngine.Object.DestroyImmediate(container);

            if (wasActive) prefab.SetActive(true);
        }
    }
}
