using AD3D_Common.Utils;
using AD3D_EnergySolution.Runtime;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Extensions;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AD3D_EnergySolution.Items.Buildable
{
    public class GenericPowerPrefab
    {
        public int MaxPowerAllowed = 750;
        public float MaxDepth = 200f;

        public Vector2Int? FuelStorageSize;  // a hopper on the prefab's 'Storage' object (Biomass Burner)
        public bool SurfaceOnly;             // can't be placed under water (SurfaceOnlyPatcher)
        public bool BakeRootRotation;        // the bundle prefab's root is rotated: move that into its children, placement resets the root

        public PrefabInfo PrefabInfo { get; }
        public RecipeData RecipeData { get; private set; }

        public Action<GameObject> OnRegisterCompleted;

        public GenericPowerPrefab(string classID, string friendlyName, string shortDescription, RecipeData recipeData)
        {
            PrefabInfo = PrefabInfo
            .WithTechType(classID, friendlyName, shortDescription, unlockAtStart: true)
            .WithIcon(ImageUtils.LoadSpriteFromTexture(
                Plugin.AssetsBundle.LoadAsset<Texture2D>($"{classID}.png") ?? Plugin.AssetsBundle.LoadAsset<Texture2D>("DeepEngine.png")));

            RecipeData = recipeData;
        }

        public void Register()
        {
            var customPrefab = new CustomPrefab(PrefabInfo);
            customPrefab.SetGameObject(GetAssetBundlePrefab);

            customPrefab.SetRecipe(RecipeData);

            customPrefab.SetEquipment(EquipmentType.Hand);
            customPrefab.SetPdaGroupCategory(TechGroup.ExteriorModules, TechCategory.ExteriorModule);

            customPrefab.Register();

            if (SurfaceOnly)
                Patches.SurfaceOnlyPatcher.TechTypes.Add(PrefabInfo.TechType);
        }

        private IEnumerator GetAssetBundlePrefab(IOut<GameObject> clonePrefab)
        {
            GameObject model = Plugin.AssetsBundle.LoadAsset<GameObject>($"{PrefabInfo.ClassID}.prefab");
            GameObject prefab = GameObject.Instantiate(model);
            prefab.SetActive(false);
            if (BakeRootRotation)
                BakeRootRotationIntoChildren(prefab.transform);
            StripColliderProxies(prefab);

            PrefabUtils.AddBasicComponents(prefab, PrefabInfo.ClassID, PrefabInfo.TechType, LargeWorldEntity.CellLevel.Global);

            MaterialUtils.ApplySNShaders(prefab, 4f, 1f, 1f, new SolarGlassModifier());

            SetupConstructable(prefab);

            // Clone solar panel
            CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(TechType.SolarPanel, true);
            yield return task;
            GameObject solarPanelPrefab = task.GetResult();

            SetupAdditionalComponents(prefab, solarPanelPrefab);
            //SetupMaterials(prefab);
            SetupLubricantController(prefab);
            SetupFuelStorage(prefab);

            clonePrefab.Set(prefab);

            OnRegisterCompleted?.Invoke(prefab);
        }

        private void SetupConstructable(GameObject prefab)
        {
            var constructible = PrefabUtils.AddConstructable(prefab, PrefabInfo.TechType, ConstructableFlags.Outside, prefab.transform.GetChild(0).gameObject);

            constructible.constructedAmount = 1;
            constructible.allowedInBase = false;
            constructible.allowedInSub = false;
            constructible.allowedOutside = true;
            constructible.allowedOnCeiling = false;
            constructible.allowedOnGround = true;
            constructible.allowedOnWall = false;
            constructible.allowedOnConstructables = false;
            constructible.rotationEnabled = true;
            constructible.placeDefaultDistance = 6f;
            constructible.placeMinDistance = 0.5f;
            constructible.placeMaxDistance = 15f;
            constructible.surfaceType = VFXSurfaceTypes.metal;
            constructible.forceUpright = true;
        }

        private void SetupMaterials(GameObject prefab)
        {
            try
            {
                foreach (Renderer renderer in prefab.GetComponentsInChildren<Renderer>())
                {
                    foreach (Material material in renderer.materials)
                    {
                        if (material == null) continue;


                        material.EnableKeyword("MARMO_EMISSION");
                        //material.SetColor(ShaderPropertyID._GlowColor, LightColor);
                        material.SetFloat(ShaderPropertyID._GlowStrength, 2.0f);
                        material.SetFloat(ShaderPropertyID._GlowStrengthNight, 3.0f);
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error updating materials: {ex}");
            }
        }

        private void SetupAdditionalComponents(GameObject prefab, GameObject clonePrefab)
        {
            var powerSource = prefab.AddComponent<PowerSource>().CopyComponent(clonePrefab.GetComponent<PowerSource>());

            var powerFX = prefab.AddComponent<PowerFX>().CopyComponent(clonePrefab.GetComponent<PowerFX>());
            powerFX.vfxPrefab = clonePrefab.GetComponent<PowerRelay>().powerFX.vfxPrefab;
            powerFX.attachPoint = prefab.FindByName("PowerPoint")?.transform ?? prefab.gameObject.transform;

            var powerRelay = prefab.AddComponent<PowerRelay>().CopyComponent(clonePrefab.GetComponent<PowerRelay>());
            powerRelay.powerSystemPreviewPrefab = clonePrefab.GetComponent<PowerRelay>().powerSystemPreviewPrefab;
            powerRelay.powerFX = powerFX;
            powerRelay.internalPowerSource = powerSource;
        }

        private void SetupLubricantController(GameObject prefab)
        {

            // A 'Lubricant' collider object in the prefab, else the LubricantContainer model
            var storageRoot = prefab.FindByName("Lubricant") ?? prefab.FindByName("LubricantContainer");
            if (storageRoot != null)
            {
                var childObjectIdentifier = storageRoot.AddComponent<ChildObjectIdentifier>();
                childObjectIdentifier.ClassId = $"{PrefabInfo.ClassID}Container";

                var container = prefab.AddComponent<StorageContainer>();
                container.prefabRoot = prefab;
                container.width = 2;
                container.height = 2;
                container.storageRoot = childObjectIdentifier;
                container.preventDeconstructionIfNotEmpty = true;
                container.hoverText = $"Open Lubricant Storage";

                var lubController = storageRoot.AddComponent<LubricantStorageController>().CopyComponent(container);

                UnityEngine.Object.DestroyImmediate(container);
            }
        }

        private void SetupFuelStorage(GameObject prefab)
        {
            if (FuelStorageSize == null)
                return;

            var point = prefab.FindByName("Storage");
            if (point == null)
            {
                Plugin.Logger.LogWarning($"{PrefabInfo.ClassID}: no 'Storage' object in the prefab, nowhere to load fuel.");
                return;
            }

            // The root must be a direct child of the prefab root, or the saved items lose their parent on load
            var root = new GameObject("FuelRoot");
            root.transform.SetParent(prefab.transform, false);
            var identifier = root.AddComponent<ChildObjectIdentifier>();
            identifier.ClassId = $"{PrefabInfo.ClassID}Fuel";

            var container = point.AddComponent<StorageContainer>();
            container.prefabRoot = prefab;
            container.storageRoot = identifier;
            container.width = FuelStorageSize.Value.x;
            container.height = FuelStorageSize.Value.y;
            container.preventDeconstructionIfNotEmpty = true;
            container.hoverText = "Load fish and plants";
            container.storageLabel = "HOPPER";
        }

        // The solar cells sit under glass: a tighter, stronger highlight than the painted metal around them, and more of
        // it at a glancing angle. (Specular itself comes from the material's specular map, see ApplySNShaders.)
        private class SolarGlassModifier : MaterialModifier
        {
            public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
            {
                if (!material.name.StartsWith("ES_SolarCell"))
                    return;
                material.SetFloat("_Shininess", 7f);
                material.SetFloat("_SpecInt", 2f);
                material.SetFloat("_Fresnel", 0.5f);
            }
        }

        // 'Col_*' objects in a model are collider proxies (solar family): the prefab puts a BoxCollider on each, and
        // their box meshes must never draw, not even in the builder's ghost
        private static void StripColliderProxies(GameObject prefab)
        {
            foreach (var t in prefab.GetComponentsInChildren<Transform>(true))
            {
                if (!t.name.StartsWith("Col_"))
                    continue;
                var renderer = t.GetComponent<MeshRenderer>();
                if (renderer != null)
                    UnityEngine.Object.DestroyImmediate(renderer);
                var filter = t.GetComponent<MeshFilter>();
                if (filter != null)
                    UnityEngine.Object.DestroyImmediate(filter);
            }
        }

        private static void BakeRootRotationIntoChildren(Transform root)
        {
            var rotation = root.localRotation;
            root.localRotation = Quaternion.identity;
            foreach (Transform child in root)
            {
                child.localPosition = rotation * child.localPosition;
                child.localRotation = rotation * child.localRotation;
            }
        }
    }
}
