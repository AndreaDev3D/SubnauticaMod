using AD3D_LightSolutionMod.BO.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AD3D_LightSolutionMod.BO.InGame
{
    public class LightSource : MonoBehaviour, IHandTarget, IProtoEventListener
    {
        // db
        public string _Id;
        //private DataItem dbItem => QPatch.Database.SwitchItemList.Where(w => w.Id == _Id).FirstOrDefault();

        // Ingame
        private Light Light;
        private int SyncCode;
        private bool IsEnable;
        private float Intensity;
        private Color Color;
        private string SyncCodeString;

        void Awake()
        {
            //QPatch.Database.Load();
            LightSwitch.OnStatusChanged += LightSwitch_OnStatusChanged;
        }

        void Start()
        {
            Light = AD3D_Common.GameObjectFinder.FindByName(gameObject, "LightItem").GetComponent<Light>();

            SetLight(SyncCode, IsEnable, Intensity, Color);
        }

        private void LightSwitch_OnStatusChanged(int syncCode, bool isEnable, Color color, float intensity) => SetLight(syncCode, isEnable, intensity, color);

        public void SetLight(int syncCode, bool isEnable, float intensity, Color color)
        {
            AD3D_Common.Helper.Log("Set LightSource", true);
            AD3D_Common.Helper.Log($"Set {SyncCode} = {syncCode}", true);
            if (SyncCode != syncCode) return;

            // Property
            IsEnable = isEnable;
            Intensity = intensity;
            Color = color;
            // Light
            Light.intensity = isEnable ? intensity : 0.0f;
            Light.color = color;

            List<Renderer> Renderers = gameObject.GetComponentsInChildren<Renderer>().ToList();
            foreach (Renderer renderer in Renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (!isEnable) 
                        material.DisableKeyword("MARMO_EMISSION");
                    else
                        material.EnableKeyword("MARMO_EMISSION");

                    material.SetColor(ShaderPropertyID._GlowColor, color);
                    material.SetFloat(ShaderPropertyID._GlowStrength, 2.0f);
                    material.SetFloat(ShaderPropertyID._GlowStrengthNight, 3.0f);
                }
            }
            AD3D_Common.Helper.Log("Set LightSource DONE", true);
        }

        public void OnHandClick(GUIHand hand)
        {
            SyncCodeString = AD3D_Common.Helper.ClipboardHelper.ClipBoard;
            SyncCode = Convert.ToInt32(SyncCodeString);
        }

        public void OnHandHover(GUIHand hand)
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.Backspace))
                {
                    SyncCodeString = string.Empty;
                    SyncCode = 0;
                }

                KeyPadDown(KeyCode.Alpha0);
                KeyPadDown(KeyCode.Alpha1);
                KeyPadDown(KeyCode.Alpha2);
                KeyPadDown(KeyCode.Alpha3);
                KeyPadDown(KeyCode.Alpha4);
                KeyPadDown(KeyCode.Alpha5);
                KeyPadDown(KeyCode.Alpha6);
                KeyPadDown(KeyCode.Alpha7);
                KeyPadDown(KeyCode.Alpha8);
                KeyPadDown(KeyCode.Alpha9);

                KeyPadDown(KeyCode.Keypad0);
                KeyPadDown(KeyCode.Keypad1);
                KeyPadDown(KeyCode.Keypad2);
                KeyPadDown(KeyCode.Keypad3);
                KeyPadDown(KeyCode.Keypad4);
                KeyPadDown(KeyCode.Keypad5);
                KeyPadDown(KeyCode.Keypad6);
                KeyPadDown(KeyCode.Keypad7);
                KeyPadDown(KeyCode.Keypad8);
                KeyPadDown(KeyCode.Keypad9);

                SyncCode = Convert.ToInt32(SyncCodeString);
            }
            catch (Exception)
            {
                SyncCodeString = string.Empty;
                SyncCode = 0;
            }
            finally
            {
                HandReticle.main.SetInteractText($"Sync Code : {SyncCode}", $"", false, false, HandReticle.Hand.None);
                HandReticle.main.SetIcon(HandReticle.IconType.Info, 1.25f);
            }

        }

        public void KeyPadDown(KeyCode keycode)
        {
            var input = "";
            if (Input.GetKeyDown(keycode))
            {
                switch (keycode)
                {
                    case KeyCode.Keypad0:
                    case KeyCode.Alpha0:
                        input = "0";
                        break;
                    case KeyCode.Keypad1:
                    case KeyCode.Alpha1:
                        input = "1";
                        break;
                    case KeyCode.Keypad2:
                    case KeyCode.Alpha2:
                        input = "2";
                        break;
                    case KeyCode.Keypad3:
                    case KeyCode.Alpha3:
                        input = "3";
                        break;
                    case KeyCode.Keypad4:
                    case KeyCode.Alpha4:
                        input = "4";
                        break;
                    case KeyCode.Keypad5:
                    case KeyCode.Alpha5:
                        input = "5";
                        break;
                    case KeyCode.Keypad6:
                    case KeyCode.Alpha6:
                        input = "6";
                        break;
                    case KeyCode.Keypad7:
                    case KeyCode.Alpha7:
                        input = "7";
                        break;
                    case KeyCode.Keypad8:
                    case KeyCode.Alpha8:
                        input = "8";
                        break;
                    case KeyCode.Keypad9:
                    case KeyCode.Alpha9:
                        input = "9";
                        break;
                }
            }
            SyncCodeString = $"{SyncCodeString}{input}";
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            AD3D_Common.Helper.Log("Saving LightSource", true);
            QPatch.Database.SwitchItemList.Where(w => w.Id == _Id).FirstOrDefault().SetSyncCode(SyncCode);
            QPatch.Database.SwitchItemList.Where(w => w.Id == _Id).FirstOrDefault().SetEnable(IsEnable);
            QPatch.Database.SwitchItemList.Where(w => w.Id == _Id).FirstOrDefault().SetIntensity(Intensity);
            QPatch.Database.SwitchItemList.Where(w => w.Id == _Id).FirstOrDefault().SetColor(Color);
            QPatch.Database.Save();
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            // Init Db
            _Id = gameObject.GetComponent<PrefabIdentifier>()?.Id ?? gameObject.GetComponentInChildren<PrefabIdentifier>().Id;
            if (QPatch.Database.SwitchItemList.Count(w => w.Id == _Id) == 0)
            {
                AD3D_Common.Helper.Log("Loading New LightSource", true);
                var newSwitch = new DataItem(_Id, SwitchItemType.Source);
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