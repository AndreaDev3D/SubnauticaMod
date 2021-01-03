using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UWE;

namespace AD3D_LightSolutionMod.BO.Patch.DeepEngine
{
    public class DeepEngineKit : Craftable
    {
        public const string AssetName = "deepengineasset";
        public const string _ClassID = "DeepEngine_Kit";
        public const string _FriendlyName = "Deep Engine MK1";
        public const string _Description = "High efficiency electric generator that runs in deep water.";

        public DeepEngineKit() : base(_ClassID, _FriendlyName, _Description)
        {
        }

        //public override string[] StepsToFabricatorTab => new string[] { "Energy Solution", "Electronics" };

        public override WorldEntityInfo EntityInfo => new WorldEntityInfo() { cellLevel = LargeWorldEntity.CellLevel.Global, classId = this.ClassID, localScale = Vector3.one, prefabZUp = false, slotType = EntitySlot.Type.Small, techType = this.TechType };


        protected override TechData GetBlueprintRecipe()
        {
            return new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                  new Ingredient(TechType.Titanium, 2),
                  new Ingredient(TechType.Lubricant, 1),
                  new Ingredient(TechType.WiringKit, 1),
                }
            };
        }

        public override GameObject GetGameObject()
        {
            //Instantiates a copy of the prefab that is loaded from the AssetBundle loaded above.
            GameObject _prefab = GameObject.Instantiate(AD3D_DeepEngineMod.BO.Utils.Helper.Bundle.LoadAsset<GameObject>("DeepEngine_Kit.prefab"));
            _prefab.name = _ClassID;
            //Need a tech tag for most prefabs
            var techTag = _prefab.EnsureComponent<TechTag>();
            techTag.type = TechType;

            _prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
            _prefab.EnsureComponent<PrefabIdentifier>().ClassId = _ClassID;
            _prefab.EnsureComponent<Pickupable>().isPickupable = true;

            // Add fabricating animation
            var fabricatingA = _prefab.EnsureComponent<VFXFabricating>();
            fabricatingA.localMinY = -0.1f;
            fabricatingA.localMaxY = 0.6f;
            fabricatingA.posOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.eulerOffset = new Vector3(0f, 0f, 0f);
            fabricatingA.scaleFactor = 0.4f;

            //Update all shaders
            ApplySubnauticaShaders(_prefab);

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
            return AD3D_Common.Helper.GetPrefabKitSprite();
        }
    }
}
