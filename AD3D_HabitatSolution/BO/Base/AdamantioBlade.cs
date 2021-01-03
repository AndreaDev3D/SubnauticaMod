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
    public class AdamantioBlade : Equipable
    {
        public const string _ClassID = "AdamantioBlade";
        public const string _FriendlyName = "Adamantio Blade";
        public const string _Description = "We all know WTF is going on.";
        public AdamantioBlade() : base(_ClassID, _FriendlyName, _Description)
        {
        }

        public override WorldEntityInfo EntityInfo => new WorldEntityInfo() 
        { 
            cellLevel = LargeWorldEntity.CellLevel.Global, 
            classId = this.ClassID, 
            localScale = Vector3.one, 
            prefabZUp = false, slotType = EntitySlot.Type.Small, 
            techType = this.TechType 
        };

        public override EquipmentType EquipmentType => EquipmentType.Hand;
        public override QuickSlotType QuickSlotType => QuickSlotType.None;

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                  new Ingredient(TechType.TitaniumIngot, 1),
                  new Ingredient(TechType.DiamondBlade, 1)
                }
            };
        }

        public override GameObject GetGameObject()
        {
            //Instantiates a copy of the prefab that is loaded from the AssetBundle loaded above.
            GameObject _prefab = CraftData.GetPrefabForTechType(TechType.DiamondBlade);
            _prefab.name = _ClassID;

            var myknife = _prefab.GetComponent<Knife>();
            myknife.bleederDamage = 6.0f;
            myknife.damage = 60.0f;

            return _prefab;
        }
        protected override Atlas.Sprite GetItemSprite()
        {
            return AD3D_Common.Helper.GetPrefabKitSprite();
        }
    }
}
