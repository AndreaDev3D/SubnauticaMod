using AD3D_Common;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AD3D_LightSolutionMod.BO.Base
{
    public class LightSourceItem : Buildable
    {
        public const string _AssetName = "lightsolutionasset";
        //public const string _ClassID = "LightSource_1";
        //public const string _FriendlyName = "Light Source 1";
        public const string _Description = "The light source item can be linked and handled by any \"Light Switch\". average use 0.06 W/s";

        public LightSourceItemType _IsWallMounted;

        public LightSourceItem(string classId, string friendlyName, LightSourceItemType isWallMounted) : base(classId, friendlyName, _Description)
        {
            ClassID = classId;
            FriendlyName = friendlyName;
            _IsWallMounted = isWallMounted;
        }

        public override TechGroup GroupForPDA => TechGroup.Miscellaneous;
        public override TechCategory CategoryForPDA => TechCategory.Misc;
        //public override TechType RequiredForUnlock => TechType.WiringKit;

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(new Ingredient[2]
                {
                  new Ingredient(TechType.Titanium, 1),
                  new Ingredient(TechType.Lithium, 1)
                })
            };
        }

        public override GameObject GetGameObject()
        {
            // Instantiates a copy of the prefab that is loaded from the AssetBundle loaded above.
            GameObject _prefab = GameObject.Instantiate(Utils.Helper.Bundle.LoadAsset<GameObject>($"{ClassID}.prefab"));
            _prefab.name = ClassID;
            //Need a tech tag for most prefabs
            var techTag = _prefab.AddComponent<TechTag>();
            techTag.type = TechType;

            _prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
            _prefab.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;

            // Update all shaders
            ApplySubnauticaShaders(_prefab);

            // Add constructable - This prefab normally isn't constructed.
            Constructable constructible = _prefab.AddComponent<Constructable>();
            constructible.constructedAmount = 1;
            constructible.allowedInBase = true;
            constructible.allowedInSub = true;
            switch (_IsWallMounted)
            {
                case LightSourceItemType.Wall:
                    constructible.allowedOutside = true;
                    constructible.allowedOnCeiling = false;
                    constructible.allowedOnGround = false;
                    constructible.allowedOnWall = true;
                    break;
                case LightSourceItemType.Roof:
                    constructible.allowedOutside = false;
                    constructible.allowedOnCeiling = true;
                    constructible.allowedOnGround = false;
                    constructible.allowedOnWall = false;
                    break;
                case LightSourceItemType.Floor:
                    constructible.allowedOutside = true;
                    constructible.allowedOnCeiling = false;
                    constructible.allowedOnGround = true;
                    constructible.allowedOnWall = false;
                    break;
            }

            constructible.allowedOnConstructables = false;
            constructible.techType = this.TechType;
            constructible.rotationEnabled = true;
            constructible.placeDefaultDistance = 1f;
            constructible.placeMinDistance = 0.1f;
            constructible.placeMaxDistance = 5f;
            constructible.surfaceType = VFXSurfaceTypes.metal;
            constructible.model = _prefab.FindChild("Model").gameObject;
            constructible.forceUpright = true;

            _prefab.AddComponent<InGame.LightSource>();

            return _prefab;
        }

        /// <summary>
        /// This game uses its own shader system and as such the shaders from UnityEditor do not work and will leave you with a black object unless in direct sunlight.
        /// Note: When copying prefabs from the game itself this is already setup and is only needed when importing new prefabs to the game.
        /// </summary>
        /// <param name="gameObject"></param>
        private static void ApplySubnauticaShaders(GameObject gameObject)
        {
            Shader shader = Shader.Find("MarmosetUBER");
            List<Renderer> Renderers = gameObject.GetComponentsInChildren<Renderer>().ToList();

            foreach (Renderer renderer in Renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    //get the old emission before overwriting the shader
                    Texture emissionTexture = material.GetTexture("_EmissionMap");

                    //overwrites your prefabs shader with the shader system from the game.
                    material.shader = shader;

                    //These enable the item to emit a glow of its own using Subnauticas shader system.
                    material.EnableKeyword("MARMO_EMISSION");
                    material.SetFloat(ShaderPropertyID._EnableGlow, 1f);
                    material.SetTexture(ShaderPropertyID._Illum, emissionTexture);
                    material.SetColor(ShaderPropertyID._GlowColor, new Color(1f, 1f, 1f, 1f));
                    material.SetFloat(ShaderPropertyID._GlowStrength, 0.0f);
                    material.SetFloat(ShaderPropertyID._GlowStrengthNight, 0.0f);
                }
            }

            //This applies the games sky lighting to the object when in the game but also only really works combined with the above code as well.
            SkyApplier skyApplier = gameObject.EnsureComponent<SkyApplier>();
            skyApplier.renderers = Renderers.ToArray();
            skyApplier.anchorSky = Skies.Auto;
        }
        protected override Atlas.Sprite GetItemSprite()
        {
            return Helper.GetSpriteFromBundle(Utils.Helper.Bundle, ClassID);
        }
    }

    public enum LightSourceItemType 
    {
        Wall, Roof, Floor
    }
}