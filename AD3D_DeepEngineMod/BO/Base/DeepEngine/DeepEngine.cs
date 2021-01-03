using AD3D_DeepEngineMod;
using AD3D_DeepEngineMod.BO.Utils;
using AD3D_LightSolutionMod.BO.InGame;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UWE;

namespace AD3D_LightSolutionMod.BO.Patch.DeepEngine
{
    public class DeepEngine : Buildable
    {
        public const string _ClassID = "DeepEngine_MK2";
        public const string _FriendlyName = "Deep Engine MK1";
        public const string _Description = "High efficiency electric generator that runs in deep water.";
        public const string _PDAKey = _ClassID;

        public DeepEngine() : base(_ClassID, _FriendlyName, _Description)
        {
        }


        public override WorldEntityInfo EntityInfo => new WorldEntityInfo() { cellLevel = LargeWorldEntity.CellLevel.Global, classId = this.ClassID, localScale = Vector3.one, prefabZUp = false, slotType = EntitySlot.Type.Small, techType = this.TechType };

        public override TechGroup GroupForPDA => TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.ExteriorModule;
        //public override TechType RequiredForUnlock => TechType.;

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(new Ingredient[1]
                {
                  new Ingredient(QPatch.DeepEngineKit.TechType, 1),
                })
            };
        }

        public override GameObject GetGameObject()
        {
            //Instantiates a copy of the prefab that is loaded from the AssetBundle loaded above.
            GameObject _prefab = GameObject.Instantiate(Helper.Bundle.LoadAsset<GameObject>($"{_ClassID}.prefab"));
            _prefab.name = _ClassID;
            //Need a tech tag for most prefabs
            var techTag = _prefab.AddComponent<TechTag>();
            techTag.type = TechType;

            _prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
            _prefab.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;

            //Collider for the turbine pole and builder tool
            //var collider = _prefab.AddComponent<BoxCollider>();
            //collider.center = new Vector3(-0.1f, 1.2f, 00f);
            //collider.size = new Vector3(3f, 2.35f, 2f);

            //Update all shaders
            ApplySubnauticaShaders(_prefab);

            // Add constructable - This prefab normally isn't constructed.
            Constructable constructible = _prefab.AddComponent<Constructable>();
            constructible.constructedAmount = 1;
            constructible.allowedInBase = false;
            constructible.allowedInSub = false;
            constructible.allowedOutside = true;
            constructible.allowedOnCeiling = false;
            constructible.allowedOnGround = true;
            constructible.allowedOnWall = false;
            constructible.allowedOnConstructables = false;
            constructible.techType = this.TechType;
            constructible.rotationEnabled = true;
            constructible.placeDefaultDistance = 6f;
            constructible.placeMinDistance = 0.5f;
            constructible.placeMaxDistance = 15f;
            constructible.surfaceType = VFXSurfaceTypes.metal;
            constructible.model = _prefab.transform.GetChild(0).gameObject;
            constructible.forceUpright = true;

            var deepEngineAnim = _prefab.AddComponent<DeepEngineAnim>();
            var deepEngineAction = _prefab.AddComponent<DeepEngineAction>();

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
            return AD3D_Common.Helper.GetSpriteFromBundle(Helper.Bundle, "Icon");
        }

        public static Atlas.Sprite GetItemIcon()
        {
            return AD3D_Common.Helper.GetSpriteFromBundle(Helper.Bundle ,"Icon");
        }

        public static string PDADescription(int MaxPower)
        {
            return $"The Deep Engine MK1 is a depth pressure generator, helps to provide electricity for new bases in hostile territory where sun or wind don't reach.{Environment.NewLine}Producing a max power of {MaxPower}W is capabilities is pretty much unlimited, since no maintenance is required is even pretty autonomous.{Environment.NewLine}You can place it outside the base or on a 15m range from the base.";
        }

        public override PDAEncyclopedia.EntryData EncyclopediaEntryData
        {
            get
            {
                PDAEncyclopedia.EntryData entry = new PDAEncyclopedia.EntryData();
                entry.key = _PDAKey;
                entry.path = "Tech/Power";
                entry.nodes = new[] { "Tech", "Power" };
                entry.unlocked = false;
                return entry;
            }
        }
    }
}

