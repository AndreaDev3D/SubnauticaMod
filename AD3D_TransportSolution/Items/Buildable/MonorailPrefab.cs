using AD3D_Common.Utils;
using AD3D_TransportSolution.Runtime;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Utility;
using System;
using System.Collections;
using UnityEngine;

namespace AD3D_TransportSolution.Items.Buildable
{
    // An outdoor buildable from the bundle: '<ClassID>.prefab' holds the model instance (named like the prefab) and
    // collider children; '<ClassID>.png' is the icon.
    public class MonorailPrefab
    {
        public PrefabInfo PrefabInfo { get; }
        public RecipeData RecipeData { get; }

        public Vector2Int? StorageSize;          // a cargo hold on the prefab's 'Storage' object
        public bool AllowedOnConstructables;     // pods and wagons are aimed at a rail, which may sit over a pylon
        public Action<GameObject> Configure;     // adds the runtime component

        // iconFallback: the class whose icon to use while '<classID>.png' isn't in the bundle yet
        public MonorailPrefab(string classID, string friendlyName, string shortDescription, RecipeData recipeData,
            string iconFallback = null)
        {
            var icon = Plugin.AssetBundle.LoadAsset<Texture2D>($"{classID}.png");
            if (icon == null && iconFallback != null)
                icon = Plugin.AssetBundle.LoadAsset<Texture2D>($"{iconFallback}.png");
            PrefabInfo = PrefabInfo
                .WithTechType(classID, friendlyName, shortDescription, unlockAtStart: true)
                .WithIcon(ImageUtils.LoadSpriteFromTexture(icon));
            RecipeData = recipeData;
        }

        public static bool InBundle(string classID) => Plugin.AssetBundle.LoadAsset<GameObject>($"{classID}.prefab") != null;

        public void Register()
        {
            var customPrefab = new CustomPrefab(PrefabInfo);
            customPrefab.SetGameObject(GetGameObjectAsync);
            customPrefab.SetRecipe(RecipeData);
            customPrefab.SetPdaGroupCategory(TechGroup.ExteriorModules, TechCategory.ExteriorModule);
            customPrefab.Register();
        }

        private IEnumerator GetGameObjectAsync(IOut<GameObject> result)
        {
            // the windows take the game's own glass, which Nautilus loads at the main menu
            yield return GlassModifier.WaitUntilReady();
            result.Set(GetGameObject());
        }

        private GameObject GetGameObject()
        {
            var prefab = UnityEngine.Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>($"{PrefabInfo.ClassID}.prefab"));
            prefab.SetActive(false);

            PrefabUtils.AddBasicComponents(prefab, PrefabInfo.ClassID, PrefabInfo.TechType, LargeWorldEntity.CellLevel.Global);
            MaterialUtils.ApplySNShaders(prefab, 4f, 1f, 1f, new GlassModifier());

            var model = prefab.transform.Find(PrefabInfo.ClassID)?.gameObject ?? prefab.transform.GetChild(0).gameObject;
            var constructable = PrefabUtils.AddConstructable(prefab, PrefabInfo.TechType, ConstructableFlags.Outside, model);
            constructable.allowedInBase = false;
            constructable.allowedInSub = false;
            constructable.allowedOutside = true;
            constructable.allowedUnderwater = true;
            constructable.allowedOnGround = true;
            constructable.allowedOnWall = false;
            constructable.allowedOnCeiling = false;
            constructable.allowedOnConstructables = AllowedOnConstructables;
            constructable.rotationEnabled = true;
            constructable.forceUpright = true;
            constructable.placeDefaultDistance = 8f;
            constructable.placeMinDistance = 2f;
            constructable.placeMaxDistance = 25f;
            constructable.surfaceType = VFXSurfaceTypes.metal;

            if (StorageSize != null)
                SetupStorage(prefab);

            Configure?.Invoke(prefab);
            return prefab;
        }

        private void SetupStorage(GameObject prefab)
        {
            var point = prefab.FindByName("Storage");
            if (point == null)
            {
                Plugin.Logger.LogWarning($"{PrefabInfo.ClassID}: no 'Storage' object in the prefab, no cargo hold.");
                return;
            }

            // The root must be a direct child of the prefab root, or the saved items lose their parent on load
            var root = new GameObject("CargoRoot");
            root.transform.SetParent(prefab.transform, false);
            var identifier = root.AddComponent<ChildObjectIdentifier>();
            identifier.ClassId = $"{PrefabInfo.ClassID}Cargo";

            var container = point.AddComponent<StorageContainer>();
            container.prefabRoot = prefab;
            container.storageRoot = identifier;
            container.width = StorageSize.Value.x;
            container.height = StorageSize.Value.y;
            container.preventDeconstructionIfNotEmpty = true;
            container.hoverText = "Open cargo";
            container.storageLabel = "CARGO";
        }
    }
}
