using AD3D_HabitatSolutionMod.BO.InGame;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AD3D_HabitatSolutionMod.BO
{

    public class AlterraVendingMachine : Buildable
    {
        public const string _ClassID = "AlterraVendingMachine";
        public const string _FriendlyName = "Alterra Vending Machine";
        public const string _ShortDescription = "Alterra Vending Machine.";
        public AlterraVendingMachine() : base(_ClassID, _FriendlyName, _ShortDescription)
        {
        }

        public override TechGroup GroupForPDA => TechGroup.BasePieces;
        public override TechCategory CategoryForPDA => TechCategory.BaseRoom;
        //public override TechType RequiredForUnlock => TechType.WiringKit;

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(new Ingredient[1]
                {
                  new Ingredient(TechType.Titanium, 1),
                })
            };
        }

        public override GameObject GetGameObject()
        {
            //Instantiates a copy of the prefab that is loaded from the AssetBundle loaded above.
            GameObject _prefab = GameObject.Instantiate(AD3D_HabitatSolutionMod.BO.Utils.Helper.Bundle.LoadAsset<GameObject>("Vending Machine Plus.prefab"));
            _prefab.name = _ClassID;
            //Need a tech tag for most prefabs
            var techTag = _prefab.AddComponent<TechTag>();
            techTag.type = TechType;

            _prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
            _prefab.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;

            //Update all shaders
            ApplySubnauticaShaders(_prefab);

            // Add Constructable
            Constructable constructible = _prefab.AddComponent<Constructable>();
            constructible.constructedAmount = 1;
            constructible.techType = this.TechType;
            constructible.model = _prefab.transform.GetChild(0).gameObject;
            //constructible.builtBoxFX = null;
            constructible.controlModelState = true;
            constructible.allowedOnWall = true;
            constructible.allowedOnGround = false;
            constructible.allowedOnCeiling = false;
            constructible.deconstructionAllowed = true;
            constructible.allowedInSub = true;
            constructible.allowedInBase = true;
            constructible.allowedOutside = true;
            constructible.allowedOnConstructables = false;
            constructible.forceUpright = true;
            constructible.rotationEnabled = true;
            constructible.placeMinDistance = 1.2f;
            constructible.placeMaxDistance = 15f;
            constructible.placeDefaultDistance = 2f;
            constructible.surfaceType = VFXSurfaceTypes.metal;

            _prefab.AddComponent<VendingMachineSystem>();

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
                    material.SetColor(ShaderPropertyID._GlowColor, new Color(1, 1f, 1, 1));
                }
            }

            //This applies the games sky lighting to the object when in the game but also only really works combined with the above code as well.
            SkyApplier skyApplier = gameObject.EnsureComponent<SkyApplier>();
            skyApplier.renderers = Renderers.ToArray();
            skyApplier.anchorSky = Skies.Auto;
        }
        protected override Atlas.Sprite GetItemSprite()
        {
            return AD3D_Common.Helper.GetSpriteFromBundle(Utils.Helper.Bundle, $"{_ClassID}_Icon");
        }
    }

}