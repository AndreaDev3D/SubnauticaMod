using AD3D_Common.Utils;
using AD3D_EnergySolution.Runtime;
using Nautilus.Extensions;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static ICSharpCode.SharpZipLib.Zip.Compression.DeflaterHuffman;

namespace AD3D_EnergySolution.BO.Runtime
{
    public class DeepEngineController : GenericPowerController
    {
        public AudioClip AudioClip;
        public AudioSource AudioSource;

        public Text lblStatus;
        public Text lblBattery;
        public Text lblEmission;
        public Text lblDepth;
        public Text lblLubricant;

        public bool MakesNoise;
        public AudioClip Engine_SFX;

        private void Awake()
        {
            // Init UI
            lblStatus = gameObject.FindComponentByName<Text>("lblStatus");
            lblBattery = gameObject.FindComponentByName<Text>("lblBattery");
            lblEmission = gameObject.FindComponentByName<Text>("lblEmission");
            lblDepth = gameObject.FindComponentByName<Text>("lblDepth");
            lblLubricant = gameObject.FindComponentByName<Text>("lblLubricant");
        }

        public override void Start()
        {
            SetupAudio();
            lblDepth.text = $"{Mathf.RoundToInt(Mathf.Abs(this.gameObject.transform.position.y)).ToString()} m";

            base.Start();
        }
        
        public override void EmitEnergy()
        {
            base.EmitEnergy();

            try
            {
                if (Constructable.constructed)
                {
                    var power = Mathf.RoundToInt(powerSource.GetPower());
                    var powerMax = Mathf.RoundToInt(powerSource.GetMaxPower());
                    lblBattery.text = $"Power {power}/{powerMax} Kw";

                    gameObject.GetComponent<DeepEngineAnimations>().IsEnabled = IsEnabled && HasLubricant;
                    UpdateUI();
                }
            }
            catch (Exception ex)
            {
               Plugin.Logger.LogError($"Error Emitting {ex.Message}");
            }
        }

        private void UpdateUI()
        {
            if (IsEnabled && powerSource != null)
            {
                var power = Mathf.RoundToInt(powerSource.GetPower());
                var powerMax = Mathf.RoundToInt(powerSource.GetMaxPower());
                var scalar = HasLubricant ? GetRechargeScalar() : 0f;
                lblStatus.text = HasLubricant ? "<color=green>POWERED</color>" : "<color=orange>NO LUBRICANT</color>";
                lblBattery.text = $"Power {power}/{powerMax} Kw";
                lblEmission.text = $"{Math.Round(CurrentEmitRate * scalar, 2)} w/sec ({scalar:P0})";
            }
            else
            {
                lblStatus.text = $"<color=yellow>STANDBY</color>";
                lblBattery.text = $"Power 0/0  Kw";
                lblEmission.text = $"0.0 w/sec";
            }

            if (lubricantStorageController != null)
                lblLubricant.text = HasLubricant ? $"{lubricantStorageController.LubricantAmount:P}" : "<color=orange>EMPTY</color>";
        }

        void SetupAudio()
        {
            if (!MakesNoise)
                return;
            AudioClip = Engine_SFX;
            AudioSource = this.gameObject.AddComponent<AudioSource>();
            AudioSource.clip = AudioClip;
            AudioSource.loop = true;
            AudioSource.maxDistance = 15f;
            AudioSource.spatialBlend = 1f;

            if (!AudioSource.isPlaying)
                AudioSource.Play();
        }

        //public override void OnHandHover(GUIHand hand)
        //{
        //    if (Constructable.constructed && powerSource != null)
        //    {
        //        if (transform.position.y <= 0f)
        //        {
        //            var emittedRate = CurrentEmitRate;
        //            var power = Mathf.RoundToInt(powerSource.GetPower());
        //            var powerMax = Mathf.RoundToInt(powerSource.GetMaxPower());
        //            // TODO
        //            //HandReticle.main.SetText($"Deep Engine: Current {power}/{powerMax}", $"Producing {Math.Round(CurrentEmitRate, 2)} w/sec", false, false);
        //            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, $"Deep Engine: Current {power}/{powerMax} \nProducing {Math.Round(CurrentEmitRate, 2)} w/sec");
        //            HandReticle.main.SetIcon(HandReticle.IconType.Info, 1.25f);
        //        }
        //        else
        //        {
        //            // TODO
        //            //HandReticle.main.SetText(("Deep Engine: ERROR", "Notice: The engine need to be submerged, please relocate", false, false);
        //            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, $"Deep Engine: ERROR \nNotice: The engine need to be submerged, please relocate at lower depth");
        //            HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
        //        }
        //    }
        //}

        //public override void OnHandClick(GUIHand hand)
        //{
        //    StartNStop();
        //}

        protected override void OnEnabledChanged()
        {
            gameObject.GetComponent<DeepEngineAnimations>().IsEnabled = IsEnabled && HasLubricant;
            UpdateUI();
        }
    }

}