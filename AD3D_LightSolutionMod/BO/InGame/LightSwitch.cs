using AD3D_LightSolutionMod.BO.Base;
using AD3D_Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AD3D_LightSolutionMod.BO.InGame
{
    class LightSwitch : MonoBehaviour, IProtoEventListener
    {
        // Events
        public delegate void StatusChanged(int SyncCode, bool IsEnable, Color MainColor, float Intensity);
        public static event StatusChanged OnStatusChanged;
        // db
        public string _Id;
        //public DataItem dbItem => QPatch.Database.SwitchItemList.Where(w => w.Id == _Id).FirstOrDefault();

        // Ingame
        public int SyncCode = 0;
        public bool IsEnable;
        public Color Color;
        public float Intensity = 0.5f;
        //
        public float MinIntensity = 0.5f;
        public float MaxIntensity = 3.0f;
        // UI
        public GameObject MainDisplay;
        public GameObject SettingsDisplay;
        public Text txtIntensity;

        public Button btnSwitch;
        public Image btnSwitchImage;
        public Sprite btnOn;
        public Sprite btnOff;

        public Button btnOpenSetting;
        public Button btnHome;
        public Button btnLessPower;
        public Button btnMorePower;
        public Text txtSyncCode;
        public Button btnSyncCode;

        public Slider SliderR;
        public Slider SliderG;
        public Slider SliderB;
        public Image ColorPicker;

        private void Awake()
        {
            //QPatch.Database.Load();
        }

        private void InitUI()
        {
            MainDisplay = GameObjectFinder.FindByName(this.gameObject, "MainDisplay");
            SettingsDisplay = GameObjectFinder.FindByName(this.gameObject, "SettingsDisplay");

            btnSwitch = GameObjectFinder.FindByName(this.gameObject, "btnSwitch").GetComponent<Button>();
            btnSwitchImage = GameObjectFinder.FindByName(this.gameObject, "btnSwitch").GetComponent<Image>();
            var btnOnTex = AD3D_Common.Helper.GetTextureFromBundle(AD3D_LightSolutionMod.BO.Utils.Helper.Bundle, "btnOn"); 
            btnOn = Sprite.Create(btnOnTex, new Rect(0.0f, 95.0f, 529.0f, 322.0f), new Vector2(0.5f, 0.5f));
            var btnOffTex = AD3D_Common.Helper.GetTextureFromBundle(AD3D_LightSolutionMod.BO.Utils.Helper.Bundle, "btnOff");
            btnOff = Sprite.Create(btnOffTex, new Rect(0.0f, 95.0f, 529.0f, 322.0f), new Vector2(0.5f, 0.5f));


            btnOpenSetting = GameObjectFinder.FindByName(this.gameObject, "btnOpenSetting").GetComponent<Button>();
            btnHome = GameObjectFinder.FindByName(this.gameObject, "btnHome").GetComponent<Button>();

            btnLessPower = GameObjectFinder.FindByName(this.gameObject, "btnLessPower").GetComponent<Button>();
            btnMorePower = GameObjectFinder.FindByName(this.gameObject, "btnMorePower").GetComponent<Button>();

            txtIntensity = GameObjectFinder.FindByName(this.gameObject, "txtIntensity").GetComponent<Text>();
            txtSyncCode = GameObjectFinder.FindByName(this.gameObject, "txtSyncCode").GetComponent<Text>();
            btnSyncCode = GameObjectFinder.FindByName(this.gameObject, "btnSyncCode").GetComponent<Button>();

            SliderR = GameObjectFinder.FindByName(this.gameObject, "SliderR").GetComponent<Slider>();
            SliderG = GameObjectFinder.FindByName(this.gameObject, "SliderG").GetComponent<Slider>();
            SliderB = GameObjectFinder.FindByName(this.gameObject, "SliderB").GetComponent<Slider>();

            ColorPicker = GameObjectFinder.FindByName(this.gameObject, "ColorPicker").GetComponent<Image>();
        }

        void Start()
        {
            InitUI();


            btnSwitch.onClick.AddListener(() => SwitchLight());
            btnSyncCode.onClick.AddListener(() => CopyToSyncCode());
            btnLessPower.onClick.AddListener(() => SetIntensity(-0.25f));
            btnMorePower.onClick.AddListener(() => SetIntensity(0.25f));
            btnOpenSetting.onClick.AddListener(() => OpenSetting(true));
            btnHome.onClick.AddListener(() => OpenSetting(false));

            SliderR.onValueChanged.AddListener(delegate { SetMainColor(); });
            SliderR.value = Color.r;
            SliderG.onValueChanged.AddListener(delegate { SetMainColor(); });
            SliderG.value = Color.g;
            SliderB.onValueChanged.AddListener(delegate { SetMainColor(); });
            SliderB.value = Color.b;

            ColorPicker.color = Color;

            // Hide Settings
            SettingsDisplay.SetActive(false);

            // Set Text
            SyncCode = QPatch.Database.SwitchItemList.Where(w => w.Id == _Id).FirstOrDefault().SyncCode;
            txtSyncCode.text = $"Sync Code: {SyncCode}";
            btnSwitchImage.sprite = IsEnable ? btnOn : btnOff;
            txtIntensity.text = Intensity.ToString();
        }

        private void CopyToSyncCode()
        {
            GUIUtility.systemCopyBuffer = QPatch.Database.SwitchItemList.Where(w => w.Id == _Id).FirstOrDefault().SyncCode.ToString();
        }

        private void OpenSetting(bool value)
        {
            SettingsDisplay.SetActive(value);
            MainDisplay.SetActive(!value);
        }

        private void SwitchLight()
        {
            IsEnable = !IsEnable;

            txtIntensity.text = Intensity.ToString();
            btnSwitchImage.sprite = IsEnable ? btnOn : btnOff;
            // Send Event
            OnStatusChanged?.Invoke(SyncCode, IsEnable, Color, Intensity);
        }

        private void SetMainColor()
        {
            Color = new Color(SliderR.value, SliderG.value, SliderB.value, 1.0f);
            ColorPicker.color = Color;
            // Send Event
            OnStatusChanged?.Invoke(SyncCode, IsEnable, Color, Intensity);
        }

        private void SetIntensity(float value)
        {
            if (Intensity <= MinIntensity && Intensity >= MaxIntensity) return;
            Intensity += value;

            txtIntensity.text = Intensity.ToString();
            // Send Event
            OnStatusChanged?.Invoke(SyncCode, IsEnable, Color, Intensity);
        }

        // Saving
        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            AD3D_Common.Helper.Log("Saving LightSwitch", true);
            QPatch.Database.SwitchItemList.Where(w => w.Id == _Id).FirstOrDefault().SetSyncCode(SyncCode);
            QPatch.Database.SwitchItemList.Where(w => w.Id == _Id).FirstOrDefault().SetEnable(IsEnable);
            QPatch.Database.SwitchItemList.Where(w => w.Id == _Id).FirstOrDefault().SetIntensity(Intensity);
            QPatch.Database.SwitchItemList.Where(w => w.Id == _Id).FirstOrDefault().SetColor(Color);
            QPatch.Database.Save();
        }

        // Loading
        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            // Init Db
            _Id = gameObject.GetComponent<PrefabIdentifier>()?.Id ?? gameObject.GetComponentInChildren<PrefabIdentifier>().Id;
            if (QPatch.Database.SwitchItemList.Count(w => w.Id == _Id) == 0)
            {
                AD3D_Common.Helper.Log("Loading New LightSwitch", true);
                var newSwitch = new DataItem(_Id, SwitchItemType.Switch);
                QPatch.Database.SwitchItemList.Add(newSwitch);
                //QPatch.Database.Save();
                //QPatch.Database.Load();
            }

            SyncCode = QPatch.Database.SwitchItemList.Where(w => w.Id == _Id).FirstOrDefault().SyncCode;
            IsEnable = QPatch.Database.SwitchItemList.Where(w => w.Id == _Id).FirstOrDefault().IsEnable;
            Intensity = QPatch.Database.SwitchItemList.Where(w => w.Id == _Id).FirstOrDefault().Intensity;
            Color = QPatch.Database.SwitchItemList.Where(w => w.Id == _Id).FirstOrDefault().Color;
        }
    }
}
