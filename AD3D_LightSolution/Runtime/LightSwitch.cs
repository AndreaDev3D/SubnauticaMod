using AD3D_Common.Utils;
using AD3D_Common.Extentions;
using AD3D_LightSolution.Base;
using Nautilus.Utility;
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
        public static event System.Action<int, bool, Color, float> OnStatusChanged;

        // db
        public string Id => gameObject.GetComponent<PrefabIdentifier>().Id;

        public DataItem DbItem => Plugin.ModData.SwitchItemList.FirstOrDefault(w => w.Id == Id);

        // Ingame
        private static readonly System.Reflection.FieldInfo _isLightsOnField = typeof(SubRoot).GetField("subLightsOn", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        public int SyncCode => DbItem.SyncCode;
        public bool IsEnabled { get; private set; }
        public bool IsBaseEnabled { get; private set; }
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
            _btnBasePower.onClick.AddListener(ToggleBasePower);

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
            ToggleLightSwitch();
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

        private void ToggleBasePower()
        {
            IsBaseEnabled = !IsBaseEnabled;
            ToggleLightSwitch();
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

        public SubRoot GetSubRoot()
        {
            SubRoot subRoot = this.GetComponentInParent<SubRoot>();
            if ((Object) subRoot == (Object) null)
                subRoot = this.gameObject?.transform?.parent?.GetComponent<SubRoot>();
            if ((Object) subRoot == (Object) null)
                subRoot = (SubRoot) this.GetComponentInParent<BaseRoot>();
            if ((Object) subRoot == (Object) null)
                subRoot = (SubRoot) this.gameObject?.transform?.parent?.GetComponent<BaseRoot>();
            return subRoot;
        }

        public void ToggleLightSwitch()
        {
            if (!this.enabled)
                return;
            SubRoot subRoot = this.GetSubRoot();
            if ((Object)subRoot == (Object)null)
                return;
            Constructable component = this.GetComponent<Constructable>();
            if ((UnityEngine.Object)component == (UnityEngine.Object)null || !component.constructed)
                return;

            bool isCurrentlyOn = (bool)_isLightsOnField.GetValue(subRoot);

            if (isCurrentlyOn != IsBaseEnabled)
            {
                subRoot.ForceLightingState(IsBaseEnabled);
                if (IsBaseEnabled)
                {
                    FMODAsset asset = new FMODAsset();
                    asset.id = "2103";
                    asset.path = "event:/sub/cyclops/lights_on";
                    asset.name = "5384ec29-f493-4ac1-9f74-2c0b14d61440";
                    asset.hideFlags = HideFlags.None;
                    FMODUWE.PlayOneShot(asset, MainCamera.camera.transform.position);
                }
                else
                {
                    FMODAsset asset = new FMODAsset();
                    asset.id = "2102";
                    asset.path = "event:/sub/cyclops/lights_off";
                    asset.name = "95b877e8-2ccd-451d-ab5f-fc654feab173";
                    asset.hideFlags = HideFlags.None;
                    FMODUWE.PlayOneShot(asset, MainCamera.camera.transform.position);
                }
            }
        }

        public void OnDestroy()
        {
            LightSource.OnSyncLight -= SyncLightWithSource;
            Plugin.Logger.LogInfo($"Destroying LightSwitch with ID: {Id}");
            Plugin.ModData.SwitchItemList.Remove(DbItem);
        }

        // Saving
        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            DbItem.SetSyncCode(SyncCode);
            DbItem.SetEnable(IsEnabled);
            DbItem.SetBaseEnable(IsBaseEnabled);
            DbItem.SetIntensity(Intensity);
            DbItem.SetColor(LightColor);
            Plugin.ModData.Save();
        }

        // Loading
        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            InitDb();
            //SyncCode = DbItem.SyncCode;
            IsEnabled = DbItem.IsEnable;
            IsBaseEnabled = DbItem.IsBaseEnabled;
            Intensity = DbItem.Intensity;
            LightColor = new Color(DbItem.R, DbItem.G, DbItem.B, 1.0f);
        }

        private void InitDb()
        {
            if (Plugin.ModData.SwitchItemList == null)
            {
                Plugin.ModData.SwitchItemList = new List<DataItem>();
            }

            Plugin.Logger.LogInfo($"Initializing database with ID: {Id}");

            if (!Plugin.ModData.SwitchItemList.Exists(w => w.Id == Id))
            {
                var newSwitch = new DataItem(Id, SwitchItemType.Switch);
                Plugin.ModData.SwitchItemList.Add(newSwitch);
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
