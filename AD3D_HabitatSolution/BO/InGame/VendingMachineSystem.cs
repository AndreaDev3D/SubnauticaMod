using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using AD3D_Common;
using UnityEngine.UI;
using SMLHelper.V2;
using SMLHelper.V2.Utility;
using System.Collections;

namespace AD3D_HabitatSolutionMod.BO.InGame
{
    public class VendingMachineSystem : MonoBehaviour, IHandTarget
    {
        private Text Title;
        private Text TotalEnergy;
        private PowerRelay powerRelay;
        private GameObject InnerPanel;
        private GameObject NoPowerAlert;
        private GameObject Logo;
        private GameObject txtLoadingPercentage;
        private GameObject BootingPanel; 
        private float BootingPanelValue;
        private GameObject MainPanel;
        private Button btnLeft;
        private Button btnRight;
        private Button btnLeftCatg;
        private Button btnRightCatg;
        private float panelwidth = 1.0934f;
        private int CurrentCategory = 1;

        private List<VendingMachineItem> ItemList = new List<VendingMachineItem>();
        public int MaxCategoryCount => (int)Enum.GetValues(typeof(CategoryType)).Cast<CategoryType>().Last(); 
        void Start()
        {
            try
            {
                // Init UI
                InnerPanel = GameObjectFinder.FindByName(this.gameObject, "InnerPanel");
                NoPowerAlert = GameObjectFinder.FindByName(this.gameObject, "NoPowerAlert");
                Logo = GameObjectFinder.FindByName(this.gameObject, "Logo");
                txtLoadingPercentage = GameObjectFinder.FindByName(this.gameObject, "txtLoadingPercentage");
                BootingPanel = GameObjectFinder.FindByName(this.gameObject, "BootingPanel");
                MainPanel = GameObjectFinder.FindByName(this.gameObject, "MainPanel");

                Title = GameObjectFinder.FindByName(this.gameObject, "Title").GetComponent<Text>();
                TotalEnergy = GameObjectFinder.FindByName(this.gameObject, "TotalEnergy").GetComponent<Text>();
                btnLeft = GameObjectFinder.FindByName(this.gameObject, "btnLeft").GetComponent<Button>();
                btnRight = GameObjectFinder.FindByName(this.gameObject, "btnRight").GetComponent<Button>();
                btnLeftCatg = GameObjectFinder.FindByName(this.gameObject, "btnLeftCatg").GetComponent<Button>();
                btnRightCatg = GameObjectFinder.FindByName(this.gameObject, "btnRightCatg").GetComponent<Button>();
                // Base PowerSource
                powerRelay = PowerSource.FindRelay(base.transform);

                btnLeft.onClick.AddListener(() => NextPage());
                btnRight.onClick.AddListener(() => PreviousPage());
                btnLeftCatg.onClick.AddListener(() => PreviousCategory());
                btnRightCatg.onClick.AddListener(() => NextCategory());

                ItemList.Add(new VendingMachineItem("DeepEngine_Kit", 50, CategoryType.Tools));

                ItemList.Add(new VendingMachineItem(TechType.Titanium, categoryType: CategoryType.Resources));
                ItemList.Add(new VendingMachineItem(TechType.Glass, categoryType: CategoryType.Resources));
                ItemList.Add(new VendingMachineItem(TechType.EnameledGlass, categoryType: CategoryType.Resources));
                ItemList.Add(new VendingMachineItem(TechType.Lead, categoryType: CategoryType.Resources));
                ItemList.Add(new VendingMachineItem(TechType.Lithium, categoryType: CategoryType.Resources));
                ItemList.Add(new VendingMachineItem(TechType.EnameledGlass, categoryType: CategoryType.Resources));
                ItemList.Add(new VendingMachineItem(TechType.Quartz, categoryType: CategoryType.Resources));
                ItemList.Add(new VendingMachineItem(TechType.Diamond, categoryType: CategoryType.Resources));
                ItemList.Add(new VendingMachineItem(TechType.Kyanite, categoryType: CategoryType.Resources, cost: 200));
                ItemList.Add(new VendingMachineItem(TechType.Salt, categoryType: CategoryType.Resources));
                ItemList.Add(new VendingMachineItem(TechType.Copper, categoryType: CategoryType.Resources));
                ItemList.Add(new VendingMachineItem(TechType.Gold, categoryType: CategoryType.Resources));
                ItemList.Add(new VendingMachineItem(TechType.Sulphur, categoryType: CategoryType.Resources));
                ItemList.Add(new VendingMachineItem(TechType.OpalGem, 50, CategoryType.Resources));

                ItemList.Add(new VendingMachineItem(TechType.Builder, 80, CategoryType.Tools));
                ItemList.Add(new VendingMachineItem(TechType.Seaglide, 80, CategoryType.Tools));
                ItemList.Add(new VendingMachineItem(TechType.Rebreather, 80, CategoryType.Tools));
                ItemList.Add(new VendingMachineItem(TechType.Compass, 80, CategoryType.Tools));
                ItemList.Add(new VendingMachineItem(TechType.UltraGlideFins, 80, CategoryType.Tools));
                ItemList.Add(new VendingMachineItem(TechType.HighCapacityTank, 80, CategoryType.Tools));
                ItemList.Add(new VendingMachineItem(TechType.LaserCutter, 80, CategoryType.Tools));
                ItemList.Add(new VendingMachineItem(TechType.Gravsphere, 80, CategoryType.Tools));

                ItemList.Add(new VendingMachineItem(TechType.WiringKit, categoryType: CategoryType.Tech));
                ItemList.Add(new VendingMachineItem(TechType.Beacon, categoryType: CategoryType.Tech));
                ItemList.Add(new VendingMachineItem(TechType.Battery, categoryType: CategoryType.Tech));

                ItemList.Add(new VendingMachineItem("Bar1", 10, CategoryType.Food));
                ItemList.Add(new VendingMachineItem(TechType.NutrientBlock, 10,categoryType: CategoryType.Food));
                ItemList.Add(new VendingMachineItem(TechType.BigFilteredWater, categoryType: CategoryType.Food));
                ItemList.Add(new VendingMachineItem(TechType.Snack1, categoryType: CategoryType.Food));
                ItemList.Add(new VendingMachineItem(TechType.Snack2, categoryType: CategoryType.Food));
                ItemList.Add(new VendingMachineItem(TechType.Snack3, categoryType: CategoryType.Food));
                ItemList.Add(new VendingMachineItem(TechType.FilteredWater, categoryType: CategoryType.Food));
                ItemList.Add(new VendingMachineItem(TechType.DisinfectedWater, categoryType: CategoryType.Food));

                ItemList.Add(new VendingMachineItem(TechType.Knife, 50, CategoryType.Weapon));
                ItemList.Add(new VendingMachineItem(TechType.HeatBlade, 50, CategoryType.Weapon));
                ItemList.Add(new VendingMachineItem(TechType.DiamondBlade, 50, CategoryType.Weapon));
                ItemList.Add(new VendingMachineItem(TechType.StasisRifle, 50, CategoryType.Weapon));

                ItemList.Add(new VendingMachineItem(TechType.Peeper, 20, CategoryType.Fish));
                ItemList.Add(new VendingMachineItem(TechType.Boomerang, 20, CategoryType.Fish));
                ItemList.Add(new VendingMachineItem(TechType.Bladderfish, 20, CategoryType.Fish));
                ItemList.Add(new VendingMachineItem(TechType.HoleFish, 20, CategoryType.Fish));
                ItemList.Add(new VendingMachineItem(TechType.Eyeye, 20, CategoryType.Fish));
                ItemList.Add(new VendingMachineItem(TechType.Hoopfish, 20, CategoryType.Fish));
                ItemList.Add(new VendingMachineItem(TechType.Hoverfish, 20, CategoryType.Fish));
                ItemList.Add(new VendingMachineItem(TechType.Oculus, 20, CategoryType.Fish));
                ItemList.Add(new VendingMachineItem(TechType.Reginald, 20, CategoryType.Fish));
                ItemList.Add(new VendingMachineItem(TechType.Spadefish, 20, CategoryType.Fish));
                ItemList.Add(new VendingMachineItem(TechType.GarryFish, 20, CategoryType.Fish));
                ItemList.Add(new VendingMachineItem(TechType.Cutefish, 200, CategoryType.Fish));


                ItemList.Add(new VendingMachineItem(TechType.SafeShallowsEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.KelpForestEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.GrassyPlateausEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.GrandReefsEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.MushroomForestEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.KooshZoneEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.TwistyBridgesEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.LavaZoneEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.StalkerEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.ReefbackEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.SpadefishEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.RabbitrayEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.MesmerEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.JumperEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.SandsharkEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.JellyrayEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.BonesharkEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.CrabsnakeEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.ShockerEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.GasopodEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.RabbitrayEggUndiscovered, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.JellyrayEggUndiscovered, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.StalkerEggUndiscovered, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.ReefbackEggUndiscovered, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.JumperEggUndiscovered, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.BonesharkEggUndiscovered, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.GasopodEggUndiscovered, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.MesmerEggUndiscovered, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.SandsharkEggUndiscovered, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.ShockerEggUndiscovered, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.GenericEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.CrashEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.CrashEggUndiscovered, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.CrabsquidEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.CrabsquidEggUndiscovered, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.CutefishEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.CutefishEggUndiscovered, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.LavaLizardEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.LavaLizardEggUndiscovered, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.CrabsnakeEggUndiscovered, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.SpadefishEggUndiscovered, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.PrecursorPrisonLabEmperorEgg, 50, CategoryType.Eggs));
                ItemList.Add(new VendingMachineItem(TechType.PrecursorLostRiverLabEgg, 50, CategoryType.Eggs));

                ItemList.Add(new VendingMachineItem(TechType.AcidMushroomSpore, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.BluePalmSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.BulboTreePiece, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.CreepvinePiece, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.CreepvineSeedCluster, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.EyesPlantSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.FernPalmSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.GabeSFeatherSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.HangingFruit, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.JellyPlantSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.KooshChunk, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.Melon, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.MelonSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.MembrainTreeSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.OrangeMushroomSpore, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.OrangePetalsPlantSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.PinkFlowerSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.PinkMushroomSpore, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.PurpleBrainCoralPiece, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.PurpleBranchesSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.PurpleFanSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.PurpleRattleSpore, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.PurpleStalkSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.PurpleTentacleSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.PurpleVegetable, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.RedBasketPlantSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.RedBushSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.RedConePlantSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.RedGreenTentacleSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.RedRollPlantSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.SeaCrownSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.ShellGrassSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.SmallFanSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.SmallMelon, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.SnakeMushroomSpore, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.SpikePlantSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.SpottedLeavesPlantSeed, 30, CategoryType.Seed));
                ItemList.Add(new VendingMachineItem(TechType.WhiteMushroomSpore, 30, CategoryType.Seed));

                ItemList.Add(new VendingMachineItem(TechType.LEDLight, 75, CategoryType.Tools));

                LoadPanel();

                NoPowerAlert.SetActive(false);
                StartCoroutine(RebootingSystem());
            }
            catch (Exception ex)
            {
                Helper.Log($"VendingMachineSystem ERROR {ex}");
            }
        }
        IEnumerator RebootingSystem()
        {
            MainPanel.SetActive(false);
            while (BootingPanelValue <= 1.0f)
            {
                yield return new WaitForSeconds(0.6f);
                BootingPanelValue += 0.02f;
                Logo.GetComponent<Image>().fillAmount = BootingPanelValue;
                txtLoadingPercentage.GetComponent<Text>().text = $"{BootingPanelValue:P0}";
            }
            BootingPanel.SetActive(false);
            MainPanel.SetActive(true);
        }

        private void AddItem(VendingMachineItem item)
        {
            var techType = item.TechType;

            var _prefab = GameObject.Instantiate(Utils.Helper.Bundle.LoadAsset<GameObject>("Item.prefab"));
            var itemPrice = GameObjectFinder.FindByName(_prefab, "ItemPrice").GetComponent<Text>();
            itemPrice.text = $"{item.Cost:0}w";

            var toolTip = Language.main.Get($"Tooltip_{techType.AsString()}");
            TooltipFactory.BuildTech(techType, false, out string toolTipText, new List<TooltipIcon> { new TooltipIcon { text = toolTip } });

            var foodIcon = _prefab.transform.GetChild(0).gameObject.EnsureComponent<uGUI_Icon>();
            foodIcon.sprite = SpriteManager.Get(techType);
            _prefab.transform.SetParent(InnerPanel.transform, false);
            _prefab.GetComponent<Button>().onClick.AddListener(() => GiveFood(techType, item.Cost));
        }

        public void GiveFood(TechType techType, float cost)
        {
            if (!CanBuy(cost))
            {
                StartCoroutine(ShowNoPower());
                return;
            }

            CraftData.AddToInventory(techType);
            powerRelay.ModifyPower(cost * -1, out float modified);
        }

        public bool CanBuy(float cost)
        {
            return powerRelay != null && this.powerRelay.IsPowered() && powerRelay.GetPower() >= cost;
        }
        private IEnumerator ShowNoPower() 
        {
            NoPowerAlert.SetActive(true);
            yield return new WaitForSeconds(4.0f);
            NoPowerAlert.SetActive(false);
        }

        void PreviousPage()
        {
            InnerPanel.GetComponent<RectTransform>().localPosition -= new Vector3(panelwidth / 3.0f, 0);
        }

        void NextPage()
        {
            if (InnerPanel.GetComponent<RectTransform>().localPosition.x == 0) return;
            InnerPanel.GetComponent<RectTransform>().localPosition += new Vector3(panelwidth / 3.0f, 0);
        }

        private void PreviousCategory()
        {
            if (CurrentCategory <= 1) return;
            CurrentCategory--;
            LoadPanel();
        }

        private void NextCategory()
        {
            if (CurrentCategory >= MaxCategoryCount) return;
            CurrentCategory++;
            LoadPanel();
        }

        private void LoadPanel()
        {
            // Reset InnerPanel
            InnerPanel.GetComponent<RectTransform>().localPosition = Vector3.zero;
            // Load panel
            var category = (CategoryType)CurrentCategory;
            // Assigne category
            Title.text = $"{category.ToString()}";
            // Clean All
            foreach (Transform child in InnerPanel.transform)
            {
                Destroy(child.gameObject);
            }
            // Add By List
            var list = ItemList.Where(w => w.CategoryType == category);
            list.ForEach(e => AddItem(e));
        }

        public void OnHandHover(GUIHand hand)
        {
            if (powerRelay == null) return;

            var energy = Math.Round(powerRelay.GetPower(), 0, MidpointRounding.AwayFromZero);
            var energyMax = Math.Round(powerRelay.GetMaxPower(), 0, MidpointRounding.AwayFromZero);
            NoPowerAlert.SetActive(energy <= 2.0);
            TotalEnergy.text = $"{energy}/{energyMax}w";

            HandReticle.main.SetInteractText("", "", false, false, HandReticle.Hand.None);
        }

        public void OnHandClick(GUIHand hand)
        {
        }
    }
}
