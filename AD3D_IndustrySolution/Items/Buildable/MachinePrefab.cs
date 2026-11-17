using AD3D_IndustrySolution.Runtime;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Extensions;
using Nautilus.Utility;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace AD3D_IndustrySolution.Items.Buildable
{
    // An outdoor drilling machine from an asset bundle prefab: constructable, power relay, storage and lubricant.
    // The behaviour components (DrillController, DrillAnimator, LegIK) are added per machine in Plugin.cs.
    public class MachinePrefab
    {
        public PrefabInfo PrefabInfo { get; }
        public RecipeData RecipeData { get; private set; }

        public Vector2Int StorageSize = new Vector2Int(6, 6);
        public Vector2Int LubricantSize = new Vector2Int(2, 2);
        public Vector3? PowerPoint; // where power beams attach, if the prefab has no 'PowerPoint' object; null = the Storage point

        public Action<GameObject> OnRegisterCompleted;

        private readonly string _bundlePrefabName;
        private readonly string _friendlyName;

        public MachinePrefab(string classID, string friendlyName, string shortDescription, string bundlePrefabName, string iconName, RecipeData recipeData)
        {
            _bundlePrefabName = bundlePrefabName;
            _friendlyName = friendlyName;

            // Fall back to a vanilla icon until the icon is tagged into the bundle
            var icon = Plugin.AssetBundle.LoadAsset<Texture2D>($"{iconName}.png");

            PrefabInfo = PrefabInfo
            .WithTechType(classID, friendlyName, shortDescription, unlockAtStart: true)
            .WithIcon(icon != null ? ImageUtils.LoadSpriteFromTexture(icon) : SpriteManager.Get(TechType.ExosuitDrillArmModule));

            RecipeData = recipeData;
        }

        public void Register()
        {
            var customPrefab = new CustomPrefab(PrefabInfo);
            customPrefab.SetGameObject(GetAssetBundlePrefab);

            customPrefab.SetRecipe(RecipeData);

            customPrefab.SetPdaGroupCategory(TechGroup.ExteriorModules, TechCategory.ExteriorModule);

            customPrefab.Register();
        }

        private IEnumerator GetAssetBundlePrefab(IOut<GameObject> outPrefab)
        {
            // The bundle prefab's root carries the FBX axis conversion (-90 on X), and the builder
            // overwrites the root rotation on placement, so wrap it under a clean root
            var prefab = new GameObject(PrefabInfo.ClassID);
            prefab.SetActive(false);

            var model = GameObject.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>($"{_bundlePrefabName}.prefab"), prefab.transform, false);
            model.name = "model";
            model.transform.localPosition = Vector3.zero; // a prefab saved away from the scene origin would build offset

            PrefabUtils.AddBasicComponents(prefab, PrefabInfo.ClassID, PrefabInfo.TechType, LargeWorldEntity.CellLevel.Global);

            MaterialUtils.ApplySNShaders(prefab);

            SetupConstructable(prefab, model);

            // Power Transmitter: carries its own PowerRelay, accepts nearby sources and links on to a base
            CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(TechType.PowerTransmitter, true);
            yield return task;

            var storagePoint = FindChild(prefab, "Storage");
            var lubricantPoint = FindChild(prefab, "Lubricant");

            SetupPowerRelay(prefab, task.GetResult(), GetPowerPoint(prefab, storagePoint));
            SetupStorage(prefab, storagePoint);
            SetupLubricant(prefab, lubricantPoint);

            outPrefab.Set(prefab);

            OnRegisterCompleted?.Invoke(prefab);
        }

        private void SetupConstructable(GameObject prefab, GameObject model)
        {
            var constructible = PrefabUtils.AddConstructable(prefab, PrefabInfo.TechType, ConstructableFlags.Outside, model);

            constructible.allowedInBase = false;
            constructible.allowedInSub = false;
            constructible.allowedOutside = true;
            constructible.allowedOnCeiling = false;
            constructible.allowedOnGround = true;
            constructible.allowedOnWall = false;
            constructible.allowedOnConstructables = false;
            constructible.rotationEnabled = true;
            constructible.placeDefaultDistance = 10f;
            constructible.placeMinDistance = 2f;
            constructible.placeMaxDistance = 25f;
            constructible.surfaceType = VFXSurfaceTypes.metal;
            constructible.forceUpright = true;
        }

        // A 'PowerPoint' object in the prefab wins, then the PowerPoint position, then the Storage point
        private Transform GetPowerPoint(GameObject prefab, GameObject storagePoint)
        {
            var prefabPoint = FindChild(prefab, "PowerPoint");
            if (prefabPoint != null)
                return prefabPoint.transform;

            if (PowerPoint == null)
                return storagePoint != null ? storagePoint.transform : prefab.transform;

            var point = new GameObject("PowerPoint").transform;
            point.SetParent(prefab.transform, false);
            point.localPosition = PowerPoint.Value;
            return point;
        }

        private void SetupPowerRelay(GameObject prefab, GameObject transmitter, Transform attachPoint)
        {
            var source = transmitter != null ? transmitter.GetComponentInChildren<PowerRelay>(true) : null;
            if (source == null)
            {
                Plugin.Logger.LogError($"{_friendlyName}: no PowerRelay found on the Power Transmitter prefab, the machine won't receive power.");
                return;
            }

            var powerFX = prefab.AddComponent<PowerFX>();
            powerFX.vfxPrefab = source.powerFX != null ? source.powerFX.vfxPrefab : null;
            powerFX.attachPoint = attachPoint;

            // A relay with no internal source: links on to the nearest relay and draws from it (MachinePowerPatcher)
            var powerRelay = prefab.AddComponent<PowerRelay>();
            powerRelay.powerSystemPreviewPrefab = source.powerSystemPreviewPrefab;
            powerRelay.maxOutboundDistance = source.maxOutboundDistance;
            powerRelay.dontConnectToRelays = source.dontConnectToRelays;
            powerRelay.powerFX = powerFX;
            powerRelay.internalPowerSource = null;
        }

        private void SetupStorage(GameObject prefab, GameObject storagePoint)
        {
            if (storagePoint == null)
            {
                Plugin.Logger.LogWarning($"{_friendlyName}: no 'Storage' object in the prefab, drilled resources have nowhere to go.");
                return;
            }

            var container = storagePoint.AddComponent<StorageContainer>();
            container.prefabRoot = prefab;
            container.storageRoot = CreateStorageRoot(prefab, "StorageRoot", $"{PrefabInfo.ClassID}Storage");
            container.width = StorageSize.x;
            container.height = StorageSize.y;
            container.preventDeconstructionIfNotEmpty = true;
            container.hoverText = $"Open {_friendlyName} Storage";
            container.storageLabel = _friendlyName.ToUpperInvariant();
        }

        private void SetupLubricant(GameObject prefab, GameObject lubricantPoint)
        {
            if (lubricantPoint == null)
            {
                Plugin.Logger.LogWarning($"{_friendlyName}: no 'Lubricant' object in the prefab, the machine can't be lubricated.");
                return;
            }

            // 2x2 = 4 Lubricant items, 25% each
            var lubricant = lubricantPoint.AddComponent<LubricantController>();
            lubricant.prefabRoot = prefab;
            lubricant.storageRoot = CreateStorageRoot(prefab, "LubricantRoot", $"{PrefabInfo.ClassID}Lubricant");
            lubricant.width = LubricantSize.x;
            lubricant.height = LubricantSize.y;
            lubricant.hoverText = "Add Lubricant";
            lubricant.storageLabel = "LUBRICANT";
        }

        // The root must be a DIRECT child of the prefab root (the PrefabIdentifier): the save stores a
        // ChildObjectIdentifier's parent as its direct parent's UniqueIdentifier only. Under a model child
        // that parent is null, and on load the items land in a temporary "MISSING PARENT OBJECT" and are lost.
        private static ChildObjectIdentifier CreateStorageRoot(GameObject prefab, string name, string classId)
        {
            var root = new GameObject(name);
            root.transform.SetParent(prefab.transform, false);

            var identifier = root.AddComponent<ChildObjectIdentifier>();
            identifier.ClassId = classId;
            return identifier;
        }

        private static GameObject FindChild(GameObject root, string name) =>
            root.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => string.Equals(t.name, name, StringComparison.OrdinalIgnoreCase))?.gameObject;
    }
}
