using AD3D_Common.Utils;
using AD3D_EnergySolution.Runtime;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Extensions;
using Nautilus.Utility;
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

        public PrefabInfo PrefabInfo { get; }
        public RecipeData RecipeData { get; private set; }

        public Action<GameObject> OnRegisterCompleted;

        public GenericPowerPrefab(string classID, string friendlyName, string shortDescription, RecipeData recipeData)
        {
            PrefabInfo = PrefabInfo
            .WithTechType(classID, friendlyName, shortDescription, unlockAtStart: true)
            .WithIcon(ImageUtils.LoadSpriteFromTexture(Plugin.AssetsBundle.LoadAsset<Texture2D>($"{classID}.png")));

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
        }

        private IEnumerator GetAssetBundlePrefab(IOut<GameObject> clonePrefab)
        {
            GameObject model = Plugin.AssetsBundle.LoadAsset<GameObject>($"{PrefabInfo.ClassID}.prefab");
            GameObject prefab = GameObject.Instantiate(model);
            prefab.SetActive(false);

            PrefabUtils.AddBasicComponents(prefab, PrefabInfo.ClassID, PrefabInfo.TechType, LargeWorldEntity.CellLevel.Global);

            MaterialUtils.ApplySNShaders(prefab);

            SetupConstructable(prefab);

            // Clone solar panel
            CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(TechType.SolarPanel, true);
            yield return task;
            GameObject solarPanelPrefab = task.GetResult();

            SetupAdditionalComponents(prefab, solarPanelPrefab);
            //SetupMaterials(prefab);
            SetupLubricantController(prefab);

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
            powerFX.attachPoint = prefab.gameObject.transform;

            var powerRelay = prefab.AddComponent<PowerRelay>().CopyComponent(clonePrefab.GetComponent<PowerRelay>());
            powerRelay.powerSystemPreviewPrefab = clonePrefab.GetComponent<PowerRelay>().powerSystemPreviewPrefab;
            powerRelay.powerFX = powerFX;
            powerRelay.internalPowerSource = powerSource;
        }

        private void SetupLubricantController(GameObject prefab)
        {

            // LubricantContainer
            var storageRoot = prefab.FindByName("LubricantContainer");
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
    }
}
