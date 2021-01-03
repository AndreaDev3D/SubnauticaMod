using AD3D_Common;
using AD3D_HabitatSolutionMod.BO.InGame;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UWE;

namespace AD3D_HabitatSolutionMod.BO
{
    public class SeaCrosser : Buildable
    {
        public const string _ClassID = "SeaCrosser";
        public const string _FriendlyName = "Sea Crosser";
        public const string _ShortDescription = "Underwater submarine.";
        public SeaCrosser() : base(_ClassID, _FriendlyName, _ShortDescription)
        {
        }
        public override TechGroup GroupForPDA => TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.ExteriorModule;

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
            GameObject _prefab = GameObject.Instantiate(Utils.Helper.Bundle.LoadAsset<GameObject>($"{_ClassID}.prefab"));
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
            ApplySubnauticaShaders(_prefab, false);
            ApplySubnauticaSky(_prefab);

            // Add constructable - This prefab normally isn't constructed.
            var rootModel = GameObjectFinder.FindByName(_prefab, "Root");
            Constructable constructible = _prefab.AddComponent<Constructable>();
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
            constructible.allowedOnConstructables = true;
            constructible.forceUpright = true;
            constructible.rotationEnabled = true;
            constructible.placeDefaultDistance = 10f;
            constructible.placeMinDistance = 5f;
            constructible.placeMaxDistance = 20f;
            constructible.surfaceType = VFXSurfaceTypes.metal;

            var seamothRef = CraftData.GetPrefabForTechType(TechType.Seamoth);
            // SeaMoth
            var seaMoth = _prefab.AddComponent<SeaMoth>();
            seaMoth.handLabel = "EnterSeaCrosser";

             var BatterySlot = GameObjectFinder.FindByName(_prefab, "BatterySlot");
            BatterySlot.AddComponent<ChildObjectIdentifier>();
            var BatteryInput = GameObjectFinder.FindByName(_prefab, "BatteryInput");
            // GenericHandTarget
            var genericHandTarget = BatteryInput.AddComponent<GenericHandTarget>();
            // OxygenManager
            var oxygenManager = _prefab.AddComponent<OxygenManager>();
            // DealDamageOnImpact
            _prefab.AddComponent<DealDamageOnImpact>();
            // LiveMixin
            var liveMixin = _prefab.AddComponent<LiveMixin>(); 
            liveMixin.data = ScriptableObject.CreateInstance<LiveMixinData>();
            liveMixin.data.maxHealth = 300;
            liveMixin = seamothRef.GetComponent<LiveMixin>();
            seaMoth.liveMixin = liveMixin;
            // VFXConstructing
            var vFXConstructing = _prefab.AddComponent<VFXConstructing>();
            vFXConstructing = seamothRef.GetComponent<VFXConstructing>();
            // FMOD_StudioEventEmitter
            _prefab.AddComponent<FMOD_StudioEventEmitter>();
            var eMx = _prefab.AddComponent<EnergyMixin>();
            eMx = seamothRef.GetComponent<EnergyMixin>();
            eMx.storageRoot = BatterySlot.GetComponent<ChildObjectIdentifier>();
            eMx.compatibleBatteries.Add(TechType.PowerCell);
            eMx.compatibleBatteries.Add(TechType.PrecursorIonBattery);
            eMx.allowBatteryReplacement = true;
            //eMx.batteryModels.ToList().Add(new GameObject(new GameObject(), TechType.PowerCell)));

            //genericHandTarget.onHandHover.AddListener(eMx.HandHover);
            // WorldForces
            var WorldForces = _prefab.AddComponent<WorldForces>();
            WorldForces.useRigidbody = _prefab.GetComponent<Rigidbody>();
            // GameInfoIcon
            var gameInfoIcon = _prefab.AddComponent<GameInfoIcon>();
            gameInfoIcon.techType = TechType.Seamoth;
            // CrushDamage
            var crushDamage =  _prefab.AddComponent<CrushDamage>();
            crushDamage.liveMixin = liveMixin;
            crushDamage.vehicle = seaMoth;
#warning add CrushDepth and Notification
            // ConditionRules
            var conditionRules = _prefab.AddComponent<ConditionRules>();
            // DepthAlarms
           var depthAlarms = _prefab.AddComponent<DepthAlarms>();
            // FMOD_CustomEmitter
            var fMOD_CustomEmitter = _prefab.AddComponent<FMOD_CustomEmitter>();
            fMOD_CustomEmitter = seamothRef.GetComponent<FMOD_CustomEmitter>();
            // FMOD_StudioEventEmitter
            _prefab.AddComponent<FMOD_StudioEventEmitter>();
            // EnergyEffect
            var energyEffect = _prefab.AddComponent<EnergyEffect>();




            return _prefab;
        }

        private static void ApplySubnauticaShaders(GameObject gameObject, bool hasLight)
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
                    if (hasLight)
                    {
                        //These enable the item to emit a glow of its own using Subnauticas shader system.
                        material.EnableKeyword("MARMO_EMISSION");
                        material.SetFloat(ShaderPropertyID._EnableGlow, 1f);
                        material.SetTexture(ShaderPropertyID._Illum, emissionTexture);
                        material.SetColor(ShaderPropertyID._GlowColor, new Color(1, 1f, 1, 1));
                    }
                }
            }

        }
        private static void ApplySubnauticaSky(GameObject gameObject)
        {
            List<Renderer> Renderers = gameObject.GetComponentsInChildren<Renderer>().ToList();
            foreach (Renderer renderer in Renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    //This applies the games sky lighting to the object when in the game but also only really works combined with the above code as well.
                    SkyApplier skyApplier = gameObject.EnsureComponent<SkyApplier>();
                    skyApplier.renderers = Renderers.ToArray();
                    skyApplier.anchorSky = Skies.Auto;
                }
            }

        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return AD3D_Common.Helper.GetSpriteFromBundle(Utils.Helper.Bundle, $"{_ClassID}_Icon");
        }
    }
}
