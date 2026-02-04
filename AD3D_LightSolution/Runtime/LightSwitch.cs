using AD3D_Common.Utils;
using AD3D_Common.Extentions;
using AD3D_LightSolution.Base;
using Nautilus.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static AD3D_LightSolution.Base.Enumerators;

namespace AD3D_LightSolution.Runtime
{
    public class LightSwitch : MonoBehaviour, IProtoEventListener, IHandTarget
    {
        // Event
        public static event Action<int, bool, Color, float> OnStatusChanged;

        // db
        public string Id => gameObject.GetComponent<PrefabIdentifier>().Id;

        public DataItem DbItem => Plugin.Database.SwitchItemList.FirstOrDefault(w => w.Id == Id);

        // Ingame
        public int SyncCode => DbItem.SyncCode;
        public bool IsEnabled { get; private set; }
        public Color LightColor { get; private set; }
        public float Intensity { get; private set; } = 0.5f;

        public float MinIntensity { get; set; } = 0.5f;
        public float MaxIntensity { get; set; } = 3.0f;

        // UI
        private GameObject _mainDisplay;
        private GameObject _settingsDisplay;
        private Text _txtIntensity;
        private Text _txtSyncCode;
        private Image _btnSwitchImage;
        private Sprite _btnOn;
        private Sprite _btnOff;
        private Slider _sliderR;
        private Slider _sliderG;
        private Slider _sliderB;
        private Image _colorPicker;

        public Button _btnOpenSetting;
        public Button _btnHome;
        public Button _btnBasePower;
        public Button _btnLessPower;
        public Button _btnMorePower;


        void Awake()
        {
        }

        void Start()
        {
            InitDb();
            CacheUIComponents();
            RegisterUIEvents();
            InitializeUI();
            LightSource.OnSyncLight += SyncLightWithSource;
        }

        private void CacheUIComponents()
        {
            _mainDisplay = this.gameObject.FindByName("MainDisplay");
            _settingsDisplay = this.gameObject.FindByName("SettingsDisplay");

            _btnSwitchImage = this.gameObject.FindComponentByName<Image>("btnSwitch");

            var btnSwitch = this.gameObject.FindComponentByName<Button>("btnSwitch");
            btnSwitch.onClick.AddListener(ToggleLight);

            var btnSyncCode = this.gameObject.FindComponentByName<Button>("btnSyncCode");
            btnSyncCode.onClick.AddListener(CopySyncCodeToClipboard);

            _txtIntensity = this.gameObject.FindComponentByName<Text>("txtIntensity");
            _txtSyncCode = this.gameObject.FindComponentByName<Text>("txtSyncCode");

            _sliderR = this.gameObject.FindComponentByName<Slider>("SliderR");
            _sliderG = this.gameObject.FindComponentByName<Slider>("SliderG");
            _sliderB = this.gameObject.FindComponentByName<Slider>("SliderB");

            _colorPicker = this.gameObject.FindComponentByName<Image>("ColorPicker");


            // Load Sprites
#if SN
            _btnOn = Plugin.AssetBundle.LoadAsset<Texture2D>($"btnOn.png").ToSprite();
            _btnOff = Plugin.AssetBundle.LoadAsset<Texture2D>($"btnOff.png").ToSprite();
#elif BZ

            _btnOn = ImageUtils.LoadSpriteFromTexture(Plugin.AssetBundle.LoadAsset<Texture2D>($"btnOn.png"));
            _btnOff = ImageUtils.LoadSpriteFromTexture(Plugin.AssetBundle.LoadAsset<Texture2D>($"btnOff.png"));
#endif

            _btnOpenSetting = this.gameObject.FindComponentByName<Button>("btnOpenSetting");
            _btnOpenSetting.onClick.AddListener(() => ToggleSettingsDisplay(true));

            _btnHome = this.gameObject.FindComponentByName<Button>("btnHome");
            _btnHome.onClick.AddListener(() => ToggleSettingsDisplay(false));

            _btnBasePower = this.gameObject.FindComponentByName<Button>("btnBasePower");

            _btnLessPower = this.gameObject.FindComponentByName<Button>("btnLessPower"); 
            _btnLessPower.onClick.AddListener(() => SetIntensity(-0.25f));

            _btnMorePower = this.gameObject.FindComponentByName<Button>("btnMorePower");
            _btnMorePower.onClick.AddListener(() => SetIntensity(0.25f));

        }

        private void RegisterUIEvents()
        {
            _sliderR.onValueChanged.AddListener(SetMainColor);
            _sliderG.onValueChanged.AddListener(SetMainColor);
            _sliderB.onValueChanged.AddListener(SetMainColor);
        }

        private void InitializeUI()
        {
            _sliderR.value = LightColor.r;
            _sliderG.value = LightColor.g;
            _sliderB.value = LightColor.b;
            _colorPicker.color = LightColor;

            ToggleSettingsDisplay(false);
            UpdateSyncCodeDisplay();
            UpdateSwitchButton();
            UpdateIntensityDisplay();
        }

        private void SyncLightWithSource()
        {
            OnStatusChanged?.Invoke(DbItem.SyncCode, IsEnabled, LightColor, Intensity);
        }

        private void CopySyncCodeToClipboard()
        {
            GUIUtility.systemCopyBuffer = DbItem.SyncCode.ToString();
        }

        private void ToggleSettingsDisplay(bool isSettingsVisible)
        {
            _settingsDisplay.SetActive(isSettingsVisible);
            _mainDisplay.SetActive(!isSettingsVisible);
        }

        private void ToggleLight()
        {
            IsEnabled = !IsEnabled;
            UpdateSwitchButton();
            UpdateIntensityDisplay();
            OnStatusChanged?.Invoke(DbItem.SyncCode, IsEnabled, LightColor, Intensity);
        }

        private void SetMainColor(float _)
        {
            LightColor = new Color(_sliderR.value, _sliderG.value, _sliderB.value, 1.0f);
            _colorPicker.color = LightColor;
            OnStatusChanged?.Invoke(DbItem.SyncCode, IsEnabled, LightColor, Intensity);
        }

        private void SetIntensity(float intensityDelta)
        {
            Intensity = Mathf.Clamp(Intensity + intensityDelta, MinIntensity, MaxIntensity);
            UpdateIntensityDisplay();
            OnStatusChanged?.Invoke(DbItem.SyncCode, IsEnabled, LightColor, Intensity);
        }

        private void UpdateSyncCodeDisplay()
        {
            _txtSyncCode.text = $"Sync Code: {DbItem.SyncCode}";
        }

        private void UpdateSwitchButton()
        {
            _btnSwitchImage.sprite = IsEnabled ? _btnOn : _btnOff;
        }

        private void UpdateIntensityDisplay()
        {
            _txtIntensity.text = Intensity.ToString("F2");
        }

        public void OnDestroy()
        {
            LightSource.OnSyncLight -= SyncLightWithSource;
            Plugin.Logger.LogInfo($"Destroying LightSwitch with ID: {Id}");
            Plugin.Database.SwitchItemList.Remove(DbItem);
        }

        // Saving
        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            DbItem.SetSyncCode(SyncCode);
            DbItem.SetEnable(IsEnabled);
            DbItem.SetIntensity(Intensity);
            DbItem.SetColor(LightColor);
            Plugin.Database.Save();
        }

        // Loading
        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            InitDb();
            //SyncCode = DbItem.SyncCode;
            IsEnabled = DbItem.IsEnable;
            Intensity = DbItem.Intensity;
            LightColor = new Color(DbItem.R, DbItem.G, DbItem.B, 1.0f);
        }

        private void InitDb()
        {
            if (Plugin.Database.SwitchItemList == null)
            {
                Plugin.Database.SwitchItemList = new List<DataItem>();
            }

            Plugin.Logger.LogInfo($"Initializing database with ID: {Id}");

            if (!Plugin.Database.SwitchItemList.Exists(w => w.Id == Id))
            {
                var newSwitch = new DataItem(Id, SwitchItemType.Switch);
                Plugin.Database.SwitchItemList.Add(newSwitch);
                Plugin.Logger.LogInfo($"Created new LightSwitch with ID: {Id}[{newSwitch.SyncCode}]");
            }
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, $"LMB on Sync Button and Select your lights \n Sync Code : {SyncCode}");
            HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1.25f);
        }

        public void OnHandClick(GUIHand hand)
        {
        }
    }
}
