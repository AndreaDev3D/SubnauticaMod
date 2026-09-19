using AD3D_Common.Extentions;
using AD3D_LightSolution.Runtime;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Extensions;
using Nautilus.Utility;
using UnityEngine;
using static AD3D_LightSolution.Base.Enumerators;
#if SN
using static CraftData;
#endif

namespace AD3D_LightSolution.Items.Buildable
{
    public class LightSourcePrefab
    {
        public const string _Description = "The light source item can be linked and handled by any \"Light Switch\". average use 0.06 W/s";

        public LightSourceItemType _IsWallMounted;

        public PrefabInfo PrefabInfo { get; }

        public LightSourcePrefab(string classID, string friendlyName, LightSourceItemType isWallMounted = LightSourceItemType.Roof)
        {
            _IsWallMounted = isWallMounted;

            PrefabInfo = PrefabInfo
            .WithTechType(classID, friendlyName, _Description, unlockAtStart: true)
            .WithIcon(ImageUtils.LoadSpriteFromTexture(Plugin.AssetBundle.LoadAsset<Texture2D>($"{classID.RemoveSuffix()}.png")));
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
                    new Ingredient(TechType.Lithium, 1)
                },
            };

            customPrefab.SetRecipe(recipe);

            customPrefab.SetEquipment(EquipmentType.Hand);
            customPrefab.SetPdaGroupCategory(TechGroup.Miscellaneous, TechCategory.Misc);

            customPrefab.Register();
        }

        private GameObject GetAssetBundlePrefab()
        {
            var prefab = Plugin.AssetBundle.LoadAsset<GameObject>($"{PrefabInfo.ClassID.RemoveSuffix()}.prefab");
            PrefabUtils.AddBasicComponents(prefab, PrefabInfo.ClassID, PrefabInfo.TechType, LargeWorldEntity.CellLevel.Medium);
            MaterialUtils.ApplySNShaders(prefab);

            SetupConstructable(prefab);

            prefab.AddComponent<LightSource>();

            return prefab;
        }

        private void SetupConstructable(GameObject prefab)
        {
            var rootModel = prefab.SearchChild("model");
            var constructable = PrefabUtils.AddConstructable(prefab, PrefabInfo.TechType, ConstructableFlags.Inside, rootModel);
            
            constructable.allowedOnConstructables = true;
            constructable.allowedInSub = true;
            constructable.deconstructionAllowed = true;
            constructable.forceUpright = true;
            constructable.rotationEnabled = true;
            constructable.placeDefaultDistance = 1f;
            constructable.placeMinDistance = 0.1f;
            constructable.placeMaxDistance = 5f;

            switch (_IsWallMounted)
            {
                case LightSourceItemType.Wall:
                    constructable.allowedOutside = true;
                    constructable.allowedOnCeiling = false;
                    constructable.allowedOnGround = false;
                    constructable.allowedOnWall = true;
                    break;
                case LightSourceItemType.Roof:
                    constructable.allowedOutside = false;
                    constructable.allowedOnCeiling = true;
                    constructable.allowedOnGround = false;
                    constructable.allowedOnWall = false;
                    break;
                case LightSourceItemType.Floor:
                    constructable.allowedOutside = true;
                    constructable.allowedOnCeiling = false;
                    constructable.allowedOnGround = true;
                    constructable.allowedOnWall = false;
                    break;
            }
        }
    }
}