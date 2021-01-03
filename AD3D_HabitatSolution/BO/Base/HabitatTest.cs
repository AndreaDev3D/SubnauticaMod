using AD3D_Common;
using AD3D_HabitatSolutionMod.BO.InGame;
using AD3D_HabitatSolutionMod.BO.Utils;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AD3D_HabitatSolutionMod.BO
{
    public class HabitatTest : Buildable
    {
        public const string _ClassID = "HabitatTest";
        public const string _FriendlyName = "Habitat Test";
        public const string _ShortDescription = "Underwater Habitat base.";
        public HabitatTest() : base(_ClassID, _FriendlyName, _ShortDescription)
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
            GameObject _prefab = GameObject.Instantiate(Utils.Helper.Bundle.LoadAsset<GameObject>("HabitatTest.prefab"));
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
            var rootModel = GameObjectFinder.FindByName(_prefab, "Root");
            ConstructableBase constructible = _prefab.AddComponent<ConstructableBase>();
            constructible.constructedAmount = 1;
            constructible.techType = this.TechType;
            constructible.model = rootModel;
            //constructible.builtBoxFX = null;
            constructible.controlModelState = true;
            constructible.allowedOnWall = false;
            constructible.allowedOnGround = true;
            constructible.allowedOnCeiling = false;
            constructible.deconstructionAllowed = true;
            constructible.allowedInSub = false;
            constructible.allowedInBase = false;
            constructible.allowedOutside = true;
            constructible.allowedOnConstructables = false;
            constructible.forceUpright = true;
            constructible.rotationEnabled = true;
            constructible.placeDefaultDistance = 10f;
            constructible.placeMinDistance = 5f;
            constructible.placeMaxDistance = 20f;
            constructible.surfaceType = VFXSurfaceTypes.metal;

            //rootModel.AddComponent<Base>();
            //var baseGhost = rootModel.AddComponent<BaseAddCellGhost>();
            //baseGhost.cellType = Base.CellType.Moonpool;
            //baseGhost.minHeightFromTerrain = 2;
            //baseGhost.maxHeightFromTerrain = 10;

            _prefab.AddComponent<HabitatBubble>();

            //_prefab.AddComponent<Rigidbody>();
            //var constructableBase = _prefab.AddComponent<ConstructableBase>();
            //var prefabIdentifier = _prefab.AddComponent<PrefabIdentifier>();


            //PowerRelay baseRoom = CraftData.GetPrefabForTechType(TechType.BaseRoom).GetComponent<PowerRelay>();
            //var baseGhost = GameObjectFinder.FindByName(_prefab, "Habitat_Proxy");
            //var baseG = baseGhost.AddComponent<global::Base>();
            //baseG = baseRoom.GetComponent<global::Base>();
            ////baseG.isGhost = false;
            //var baseGC = baseGhost.AddComponent<BaseAddCellGhost>();
            //baseGC = baseRoom.GetComponent<BaseAddCellGhost>();
            //baseGC.cellType = Base.CellType.Moonpool;
            //baseGC.minHeightFromTerrain = 2;
            //baseGC.maxHeightFromTerrain = 10;
            //_prefab.AddComponent<BehaviourLOD>();
            //_prefab.AddComponent<PowerRelay>();
            //_prefab.AddComponent<LiveMixin>();
            //_prefab.AddComponent<Stabilizer>();
            //_prefab.AddComponent<DealDamageOnImpact>();
            //_prefab.AddComponent<SubWaterPlane>();
            //_prefab.AddComponent<CrushDamage>();

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
