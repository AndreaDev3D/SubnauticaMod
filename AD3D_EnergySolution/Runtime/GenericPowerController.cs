using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AD3D_EnergySolution.Runtime
{
    public class GenericPowerController: MonoBehaviour, IHandTarget
    {
        public bool IsEnabled = true;

        public PowerSource powerSource;
        [AssertNotNull]
        public PowerRelay powerRelay;

        private Constructable _constructable;
        public Constructable Constructable => _constructable ??= gameObject.GetComponent<Constructable>();

        public int MaxPowerAllowed = 750;
        public float MinDepth = -5f;
        public float MaxDepth = 50f;
        public bool IsSunSusceptible = true;
        public float CurrentEmitRate = 0.25f;
        public float CurrentEmitIntervalSec = 2.0f;
        private float biomeSunlightScale = 1f;

        public LubricantStorageController lubricantStorageController;

        // Linear interp on world-space Y between MinDepth (0%) and MaxDepth (100%).
        // Works for both directions: Wind/Solar use MaxDepth > MinDepth (high = strong);
        // Deep Engine uses MaxDepth < MinDepth (deep = strong).
        private float GetDepthScalar()
        {
            var range = MaxDepth - MinDepth;
            if (Mathf.Approximately(range, 0f))
                return 0f;
            return Mathf.Clamp01((gameObject.transform.position.y - MinDepth) / range);
        }

        private float GetSunScalar() => DayNightCycle.main.GetLocalLightScalar() * this.biomeSunlightScale;

        protected float GetRechargeScalar()
        {
            var depth = GetDepthScalar();
            return IsSunSusceptible ? depth * GetSunScalar() : depth;
        }

        public virtual void Start()
        {
            powerRelay = gameObject.GetComponent<PowerRelay>();
            powerSource = gameObject.GetComponent<PowerSource>();

            powerSource.maxPower = MaxPowerAllowed;

            lubricantStorageController = gameObject.GetAllComponentsInChildren<LubricantStorageController>().FirstOrDefault();

            InvokeRepeating("EmitEnergy", 0, CurrentEmitIntervalSec);
        }

        public virtual void EmitEnergy()
        {
            if (!IsEnabled)
                return;

            try
            {
                if (Constructable.constructed)
                {
                    var rate = CurrentEmitRate * GetRechargeScalar();
                    powerRelay.ModifyPower(rate, out float num);

                    if (lubricantStorageController != null)
                    {
                        var result = lubricantStorageController.SetLubricantAmount(-0.0001f);
                        if (result == 0f)
                        {
                            StartNStop();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error Emitting {gameObject.name} {ex.Message}");
            }
        }

        public virtual void OnHandHover(GUIHand hand)
        {
            if (!this.gameObject.GetComponent<Constructable>().constructed)
                return;

            var text = "";
            if (IsEnabled)
            {
                var recharge = this.GetRechargeScalar();
                var power = Mathf.RoundToInt(this.powerSource.GetPower());
                var maxPower = Mathf.RoundToInt(this.powerSource.GetMaxPower());
                text = $"Efficiency: {recharge:P0} \n Charge: {power}/{maxPower} kW";

                if (lubricantStorageController != null)
                    text += $"\nLubricant: {lubricantStorageController.LubricantAmount:P}";

            }else
            {
                text = "Power Off";
            }

            HandReticle.main.SetText(HandReticle.TextType.Hand, text, false);
            HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
            HandReticle.main.SetIcon(HandReticle.IconType.Hand);
        }

        public virtual void OnHandClick(GUIHand hand)
        {
            StartNStop();
        }

        public virtual void StartNStop()
        {
            IsEnabled = !IsEnabled;
        }
    }
}
