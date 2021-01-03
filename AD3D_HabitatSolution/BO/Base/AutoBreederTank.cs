using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AD3D_Common;
using DevExpress.Xpo;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace AD3D_HabitatSolutionMod.BO
{

    public class AutoBreederTank : Buildable
    {
        public const string _ClassID = "AutoBreederTank";
        public const string _FriendlyName = "Auto Breeder Tank";
        public const string _ShortDescription = "Outdoor tank alimented with base power is mainly used for breeding marine fishes.";
        public AutoBreederTank() : base(_ClassID, _FriendlyName, _ShortDescription)
        {
        }

        public override TechGroup GroupForPDA => TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.ExteriorModule;
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
            GameObject _prefab = GameObject.Instantiate(Utils.Helper.Bundle.LoadAsset<GameObject>("AutoBreederTank.prefab"));
            _prefab.name = _ClassID;
            //Need a tech tag for most prefabs
            var techTag = _prefab.EnsureComponent<TechTag>();
            techTag.type = TechType;

            //_prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
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
            constructible.placeMinDistance = 3f;
            constructible.placeMaxDistance = 15f;
            constructible.placeDefaultDistance = 5f;
            constructible.surfaceType = VFXSurfaceTypes.metal;

            var storageRoot = GameObjectFinder.FindByName(_prefab, "StorageRoot");
            var storageRootChild = storageRoot.AddComponent<ChildObjectIdentifier>();


             var storageContainer = _prefab.AddComponent<StorageContainer>();
            storageContainer.prefabRoot = _prefab;
            storageContainer.width = 4;
            storageContainer.height = 4;
            storageContainer.hoverText = "Use AutoBreederTank";
            storageContainer.modelSizeRadius = 3.5f;
            storageContainer.storageLabel = "AutoBreederTank Storage";
            storageContainer.storageRoot = storageRootChild;
            storageContainer.preventDeconstructionIfNotEmpty = true;
            // Aquarium
            var trackObjectList = new List<GameObject>();
            for (int i = 1; i <= 16; i++)
            {
                trackObjectList.Add(GameObjectFinder.FindByName(_prefab, $"FishAnchor ({i})"));
            }

            var aquarium = _prefab.AddComponent<Aquarium>();
            aquarium.storageContainer = storageContainer;
            aquarium.fishRoot = GameObjectFinder.FindByName(_prefab, "FishRoot");
            aquarium.trackObjects = trackObjectList.ToArray();
            aquarium.spreadInfectionInterval = 10;

            var constructableBound = _prefab.AddComponent<ConstructableBounds>();
            constructableBound.bounds.position = new Vector3(0.0f, 3.0f, -1.35f);
            constructableBound.bounds.extents = new Vector3(5.5f, 3.125f, 5.5f);

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
                    //overwrites your prefabs shader with the shader system from the game.
                    material.shader = shader;
                    Console.WriteLine(material.name);
                    if (material.name == "Auto-fish catcher Transparent (Instance)")
                    {
                        material.DisableKeyword("FX_ANIMATEDGLOW");
                        material.DisableKeyword("FX_BLEEDER");
                        material.DisableKeyword("FX_BUILDING");
                        material.DisableKeyword("FX_BURST");
                        material.DisableKeyword("FX_DEFORM");
                        material.DisableKeyword("FX_IONCRYSTAL");
                        material.DisableKeyword("FX_KELP");
                        material.DisableKeyword("FX_MESMER");
                        material.DisableKeyword("FX_PROPULSIONCANNON");
                        material.DisableKeyword("FX_ROPE");
                        material.DisableKeyword("FX_SINWAVE");
                        material.DisableKeyword("GLOW_UV_FROM_VERTECCOLOR");
                        material.DisableKeyword("MARMO_ALPHA_CLIP");
                        material.DisableKeyword("MARMO_EMISSION");
                        material.DisableKeyword("MARMO_NORMALMAP");
                        material.EnableKeyword("MARMO_SIMPLE_GLASS");
                        material.EnableKeyword("MARMO_SPECMAP");
                        material.DisableKeyword("MARMO_VERTEX_COLOR");
                        material.DisableKeyword("UWE_3COLOR");
                        material.DisableKeyword("UWE_DETAILMAP");
                        material.DisableKeyword("UWE_DITHERALPHA");
                        material.DisableKeyword("UWE_INFECTION");
                        material.DisableKeyword("UWE_LIGHTMAP");
                        material.DisableKeyword("UWE_PLAYERINFECTION");
                        material.DisableKeyword("UWE_SCHOOLFISH");
                        material.DisableKeyword("UWE_SIG");
                        material.DisableKeyword("UWE_VR_FADEOUT");
                        material.DisableKeyword("UWE_WAVING");

                        material.SetColor("_Color", new Color(1f, 0.94f, 0f, 0.22f));
                        material.SetColor("_Color2", new Color(1f, 1f, 1f, 1f));
                        material.SetColor("_Color3", new Color(1f, 1f, 1f, 1f));
                        material.SetFloat("_Mode", 3f);
                        material.SetFloat("_Fresnel", 0.7f);
                        material.SetFloat("_Shininess", 7f);
                        material.SetFloat("_SpecInt", 7f);
                        material.SetFloat("_EnableGlow", 0f);
                        material.SetFloat("_EnableLighting", 1f);
                        material.SetColor("_SpecColor", new Color(0.79f, 0.9785799f, 1f, 1f));
                        material.SetFloat("_SrcBlend", 1f);
                        material.SetFloat("_DstBlend", 1f);
                        material.SetFloat("_SrcBlend2", 0f);
                        material.SetFloat("_DstBlend2", 10f);
                        material.SetFloat("_AddSrcBlend", 1f);
                        material.SetFloat("_AddDstBlend", 1f);
                        material.SetFloat("_AddSrcBlend2", 0f);
                        material.SetFloat("_AddDstBlend2", 10f);
                        material.SetFloat("_EnableMisc", 1f);
                        material.SetFloat("_ZWrite", 0f);
                        material.SetFloat("_ZOffset", 0f);
                        material.SetFloat("_EnableCutOff", 0f);
                        material.SetFloat("_Cutoff", 0f);
                        material.SetFloat("_EnableDitherAlpha", 0f);
                        material.SetFloat("_EnableVrFadeOut", 0f);
                        material.SetFloat("_IBLreductionAtNight", 0f);
                        material.SetFloat("_EnableSimpleGlass", 1f);
                        material.SetFloat("_EnableVertexColor", 0f);
                        material.SetFloat("_EnableSchoolFish", 0f);
                        material.SetFloat("_EnableMainMaps", 1f);
                        material.SetColor("_GlowColor", new Color(1f, 1f, 1f, 1f));
                        material.SetFloat("_GlowUVfromVC", 0f);
                        material.SetFloat("_GlowStrength", 1f);
                        material.SetFloat("_GlowStrengthNight", 1f);
                        material.SetFloat("_EmissionLM", 0f);
                        material.SetFloat("_EmissionLMNight", 0f);
                        material.SetFloat("_EnableDetailMaps", 0f);
                        material.SetVector("_DetailIntensities", new Vector4(0.2f, 0.2f, 0.2f, 0f));
                        material.SetFloat("_EnableLightmap", 0f);
                        material.SetFloat("_LightmapStrength", 2.65f);
                        material.SetFloat("_Enable3Color", 0f);
                        material.SetColor("_SpecColor2", new Color(1f, 1f, 1f, 1f));
                        material.SetColor("_SpecColor3", new Color(1f, 1f, 1f, 1f));
                        material.SetVector("_DeformParams", new Vector4(1f, 1f, 0.2f, 0.2f));
                        material.SetFloat("_FillSack", 0f);
                        material.SetFloat("_OverlayStrength", 1f);
                        material.SetColor("_GlowScrollColor", new Color(1f, 1f, 1f, 1f));
                        material.SetFloat("_Hypnotize", 1f);
                        material.SetColor("_ScrollColor", new Color(1f, 1f, 1f, 1f));
                        material.SetVector("_ColorStrength", new Vector4(1f, 1f, 1f, 1f));
                        material.SetVector("_GlowMaskSpeed", new Vector4(1f, 1f, 1f, 1f));
                        material.SetVector("_ScrollSpeed", new Vector4(0.1f, 0.1f, 0f, 0f));
                        material.SetColor("_DetailsColor", new Color(1f, 1f, 1f, 1f));
                        material.SetColor("_SquaresColor", new Color(1f, 1f, 1f, 1f));
                        material.SetFloat("_SquaresTile", 30f);
                        material.SetFloat("_SquaresSpeed", 0.5f);
                        material.SetFloat("_SquaresIntensityPow", 2f);
                        material.SetVector("_NoiseSpeed", new Vector4(1f, 1f, 1f, 1f));
                        material.SetVector("_FakeSSSparams", new Vector4(1f, 1f, 1f, 1f));
                        material.SetVector("_FakeSSSSpeed", new Vector4(1f, 1f, 1f, 1f));
                        material.SetColor("_BorderColor", new Color(1f, 1f, 1f, 1f));
                        material.SetFloat("_Built", 0f);
                        material.SetVector("_BuildParams", new Vector4(1f, 1f, 5f, 4f));
                        material.SetFloat("_BuildLinear", 0f);
                        material.SetFloat("_NoiseThickness", 0.05f);
                        material.SetFloat("_NoiseStr", 0.2f);
                        material.SetVector("_Scale", new Vector4(0.12f, 0.05f, 0.12f, 0.1f));
                        material.SetVector("_Frequency", new Vector4(0.6f, 0.5f, 0.75f, 0.8f));
                        material.SetVector("_Speed", new Vector4(0.6f, 0.3f, 0f, 0f));
                        material.SetVector("_ObjectUp", new Vector4(0f, 0f, 1f, 0f));
                        material.SetFloat("_WaveUpMin", 0f);
                        material.SetFloat("_Fallof", 1f);
                        material.SetFloat("_RopeGravity", 1f);
                        material.SetFloat("_minYpos", 0.5f);
                        material.SetFloat("_maxYpos", 0.5f);
                        material.SetFloat("_EnableBurst", 0f);
                        material.SetFloat("_Displacement", 5f);
                        material.SetFloat("_BurstStrength", 0f);
                        material.SetVector("_Range", new Vector4(0f, 70f, 0f, 1f));
                        material.SetFloat("_ClipRange", 0.6f);
                        material.SetFloat("_EnableInfection", 0f);
                        material.SetFloat("_EnablePlayerInfection", 0f);
                        material.SetFloat("_InfectionHeightStrength", 0f);
                        material.SetVector("_InfectionScale", new Vector4(1f, 1f, 1f, 0f));
                        material.SetVector("_InfectionOffset", new Vector4(0f, 0f, 0f, 0f));
                        material.SetVector("_InfectionSpeed", new Vector4(1f, 1f, 1f, 0f));
                        material.SetFloat("_MyCullVariable", 2f);

                    }
                    else 
                    {
                        //get the old emission before overwriting the shader
                        Texture emissionTexture = material.GetTexture("_EmissionMap");


                        //These enable the item to emit a glow of its own using Subnauticas shader system.
                        material.EnableKeyword("MARMO_EMISSION");
                        material.SetFloat(ShaderPropertyID._EnableGlow, 1f);
                        material.SetTexture(ShaderPropertyID._Illum, emissionTexture);
                        material.SetColor(ShaderPropertyID._GlowColor, new Color(1, 1f, 1, 1));
                    }
                }
            }

            //This applies the games sky lighting to the object when in the game but also only really works combined with the above code as well.
            SkyApplier skyApplier = gameObject.EnsureComponent<SkyApplier>();
            skyApplier.renderers = Renderers.ToArray();
            skyApplier.anchorSky = Skies.Auto;


            var glass = GameObjectFinder.FindByName(gameObject, "Tank").GetComponent<Renderer>();
            var glass1 = GameObjectFinder.FindByName(gameObject, "Tank.001").GetComponent<Renderer>();
            SkyApplier skyApplier1 = gameObject.EnsureComponent<SkyApplier>();
            skyApplier1.renderers = new Renderer[] { glass, glass1 };
            skyApplier1.anchorSky = Skies.BaseGlass;
        }
        protected override Atlas.Sprite GetItemSprite()
        {
            return AD3D_Common.Helper.GetSpriteFromBundle(Utils.Helper.Bundle, $"AutoBreederTank_Icon");
        }
    }

}