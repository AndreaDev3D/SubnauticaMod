using AD3D_Common;
using AD3D_LightSolutionMod.BO.InGame;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UWE;

namespace AD3D_DeepEngineMod.BO.Base.SolarSource
{
    public class SolarPanelItem : Buildable
    {
        public const string _AssetName = "deepengineasset";
        public const string _ClassID = "SolarPanel_1";
        public const string _FriendlyName = "Solar Panel 1";
        public const string _Description = "High efficiency solar panel with a capacity of 150W.";

        public SolarPanelItem() : base(_ClassID, _FriendlyName, _Description)
        {
        }
        public override WorldEntityInfo EntityInfo => new WorldEntityInfo() { cellLevel = LargeWorldEntity.CellLevel.Global, classId = this.ClassID, localScale = Vector3.one, prefabZUp = false, slotType = EntitySlot.Type.Small, techType = this.TechType };
        public override TechGroup GroupForPDA => TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.ExteriorModule;
        protected override TechData GetBlueprintRecipe()
        {
            return new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(new Ingredient[2]
                {
                  new Ingredient(TechType.Quartz, 1),
                  new Ingredient(TechType.Lithium, 2),
                })
            };
        }
        public override GameObject GetGameObject()
        {
            //Instantiates a copy of the prefab that is loaded from the AssetBundle loaded above.
            GameObject _prefab = GameObject.Instantiate(BO.Utils.Helper.Bundle.LoadAsset<GameObject>($"{_ClassID}.prefab"));
            _prefab.name = _ClassID;
            //Need a tech tag for most prefabs
            var techTag = _prefab.AddComponent<TechTag>();
            techTag.type = TechType;

            _prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
            _prefab.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;

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
            constructible.placeDefaultDistance = 5f;
            constructible.placeMinDistance = 1.2f;
            constructible.placeMaxDistance = 5f;
            constructible.surfaceType = VFXSurfaceTypes.metal;
            constructible.model = GameObjectFinder.FindByName(_prefab, "Ghost");//.transform.GetChild(0).gameObject;
            constructible.forceUpright = false;

            var solarPower = CraftData.GetPrefabForTechType(TechType.SolarPanel);
            PowerRelay solarPowerRelay = solarPower.GetComponent<PowerRelay>();
            var PowerSource = _prefab.AddComponent<PowerSource>();
            PowerSource.maxPower = 900;

            var PowerFX = _prefab.AddComponent<PowerFX>();
            PowerFX.vfxPrefab = solarPowerRelay.powerFX.vfxPrefab;
            PowerFX.attachPoint = GameObjectFinder.FindByName(_prefab, "powerFX_AttachPoint").transform;

            var PowerRelay = _prefab.AddComponent<PowerRelay>();
            PowerRelay.maxOutboundDistance = 15;
            PowerRelay.internalPowerSource = PowerSource;
            PowerRelay.powerFX = PowerFX;

            var PowerSystemPreview = _prefab.AddComponent<PowerSystemPreview>();
            PowerSystemPreview.previewPowerFX = PowerFX;
            PowerSystemPreview.powerRelay = PowerRelay;

            var PowerFXPreview = _prefab.AddComponent<PowerFX>();
            PowerFX.vfxPrefab = solarPowerRelay.powerFX.vfxPrefab;
            PowerFXPreview.attachPoint = GameObjectFinder.FindByName(_prefab, "powerFX_AttachPoint").transform;

            var LiveMixin = _prefab.AddComponent<LiveMixin>();
            LiveMixin.health = 50;
            LiveMixin.startHealthPercent = 1.0f;
            LiveMixin.data = solarPower.GetComponent<LiveMixin>().data;

            var SolarPanel = _prefab.AddComponent<SolarPanel>();
            SolarPanel.powerSource = PowerSource;
            SolarPanel.maxDepth = 400;

            var SubModuleHandler = _prefab.AddComponent<SubModuleHandler>();

            return _prefab;
        }

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
            return AD3D_Common.Helper.GetSpriteFromBundle(AD3D_DeepEngineMod.BO.Utils.Helper.Bundle, "Icon");
        }
    }
}
