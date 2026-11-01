using AD3D_Common.Extentions;
using AD3D_LightSolution.Runtime;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Extensions;
using Nautilus.Utility;
using UnityEngine;
using static CraftData;

namespace AD3D_LightSolution.Items.Buildable
{
    public class LightStandPrefab
    {
        public const string _Description = "The light stand item can be used with any \"Light Switch\" or \"Light Source\". Ideal for outdoor spaces.";
        public PrefabInfo PrefabInfo { get; }

        public LightStandPrefab(string classID, string friendlyName, string shortDescription = _Description)
        {
            PrefabInfo = PrefabInfo
            .WithTechType(classID, friendlyName, shortDescription, unlockAtStart: true)
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
                    new Ingredient(TechType.Titanium, 1)
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

            // A stand is only a mount for switches and lights: it has no Light or switch UI of its own
            SetupConstructable(prefab);

            return prefab;
        }

        private void SetupConstructable(GameObject prefab)
        {
            var rootModel = prefab.SearchChild("model");
            var constructable = PrefabUtils.AddConstructable(prefab, PrefabInfo.TechType, ConstructableFlags.Inside, rootModel);

            constructable.allowedOnConstructables = true;
            constructable.allowedOnGround = true;
            constructable.allowedOnCeiling = false;
            constructable.allowedOnWall = false;
            constructable.allowedOutside = true;
            constructable.allowedInSub = true;
            constructable.deconstructionAllowed = true;
            constructable.forceUpright = true;
            constructable.rotationEnabled = true;
            constructable.placeDefaultDistance = 1f;
            constructable.placeMinDistance = 0.1f;
            constructable.placeMaxDistance = 5f;
        }
    }
}
