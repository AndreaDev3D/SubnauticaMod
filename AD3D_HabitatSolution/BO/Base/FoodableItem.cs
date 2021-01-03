using System;
using System.Collections.Generic;
using System.Linq;
using AD3D_Common;
using DevExpress.Xpo;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using UnityEngine;

namespace AD3D_HabitatSolutionMod.BO.Base
{

    public class FoodableItem : Craftable
    {
        public int FoodValue = 15;
        public int WaterValue = 0;
        public int StomachVolume = 0;
        public bool Decomposes = false;
        public bool Despawns = false;
        public bool AllowOverfill = false;
        public int KDecayRate = 0;

        internal ICraftTreeHandler CraftTreeHandler { get; set; } = SMLHelper.V2.Handlers.CraftTreeHandler.Main;
        public virtual CraftTree.Type FabricatorType => CraftTree.Type.Fabricator;
        public virtual string[] StepsToFabricatorTab => new string[]{"Sustenance",""}; 
        public virtual float CraftingTime => 0.5f;

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(new Ingredient[2]
                {
                  new Ingredient(TechType.FilteredWater, 1),
                  new Ingredient(TechType.Peeper, 1)
                })
            };
        }

        public FoodableItem(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
        {
            CraftDataHandler.SetCraftingTime(this.TechType, this.CraftingTime); 
            CraftTreeHandler.AddCraftingNode(this.FabricatorType, this.TechType, this.StepsToFabricatorTab);
        }

        public override GameObject GetGameObject() 
        {
            var _prefab = GameObject.Instantiate(Utils.Helper.Bundle.LoadAsset<GameObject>($"{ClassID}.prefab"));

            var pick = _prefab.AddComponent<Pickupable>();
            pick.isPickupable = true;
            pick.destroyOnDeath = true;
            pick.cubeOnPickup = false;
            pick.randomizeRotationWhenDropped = true;
            pick.usePackUpIcon = false;

            var rb = _prefab.AddComponent<Rigidbody>();
            rb.mass = 1;
            rb.drag = 1;
            rb.angularDrag = 1;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            _prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;

            var techTag = _prefab.AddComponent<TechTag>();
            techTag.type = TechType;

            _prefab.AddComponent<ItemPrefabData>();

            var wf = _prefab.AddComponent<WorldForces>();
            wf.useRigidbody = rb;

            ApplySubnauticaShaders(_prefab);

            var eat = _prefab.AddComponent<Eatable>();
            eat.foodValue = FoodValue;
            eat.waterValue = WaterValue;
            eat.stomachVolume = StomachVolume;
            eat.decomposes = Decomposes;
            eat.despawns = Despawns;
            eat.allowOverfill = AllowOverfill;
            eat.kDecayRate = KDecayRate;
            eat.despawnDelay = 300;


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
            return Helper.GetSpriteFromBundle(Utils.Helper.Bundle, ClassID);
        }
    }

}