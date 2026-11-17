using AD3D_Common.Utils;
using AD3D_EnergySolution.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AD3D_EnergySolution.Runtime
{
    // Burns fish and plants from its hopper. Each item gives FuelEfficiency x its Bioreactor charge, burned at
    // CurrentEmitRate per tick. It only burns above water, with lubricant, and while its power storage has room.
    public class BiomassBurnerController : GenericPowerController
    {
        public float FuelEfficiency = 0.5f;

        private StorageContainer _hopper;
        private float _fuel;         // energy left in the item that is burning
        private bool _burning;
        private float _flame;        // 0..1, eased towards _burning

        private Transform _lid;
        private Quaternion _lidClosed;
        private float _lidSign = 1f;
        private float _lidAngle;
        private const float LidOpenAngle = 65f;

        private Material _fireMaterial;
        private Light _fireLight;
        private float _noiseSeed;

        public bool IsAboveWater => transform.position.y >= SurfaceOnlyPatcher.MinHeight;

        private bool IsStorageFull => powerSource.GetPower() >= powerSource.GetMaxPower() - 0.01f;

        public override void Start()
        {
            base.Start();

            _hopper = gameObject.FindByName("Storage")?.GetComponent<StorageContainer>();
            if (_hopper != null)
                _hopper.container.isAllowedToAdd += IsFuel;

            var id = Id;
            if (!string.IsNullOrEmpty(id) && Plugin.ModData.BurnerFuel != null && Plugin.ModData.BurnerFuel.TryGetValue(id, out var fuel))
                _fuel = fuel;

            SetupLid();
            SetupFire();
        }

        private static bool IsFuel(Pickupable pickupable, bool verbose)
        {
            var allowed = pickupable != null && BaseBioReactor.GetCharge(pickupable.GetTechType()) > 0f;
            if (!allowed && verbose)
                ErrorMessage.AddMessage("Only fish and plants burn");
            return allowed;
        }

        public override void EmitEnergy()
        {
            try
            {
                UpdateLubricantWarning();
                _burning = false;

                if (!IsEnabled || !Constructable.constructed || !HasLubricant || !IsAboveWater || IsStorageFull)
                    return;

                if (_fuel <= 0f && !TakeFuel())
                    return;

                var room = powerSource.GetMaxPower() - powerSource.GetPower();
                var amount = Mathf.Min(CurrentEmitRate, _fuel, room);
                powerRelay.ModifyPower(amount, out float added);
                if (added <= 0f)
                    return;

                _fuel = Mathf.Max(0f, _fuel - added);
                _burning = true;
                lubricantStorageController?.SetLubricantAmount(-0.0001f);
                SaveFuel();
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error burning {gameObject.name} {ex.Message}");
            }
        }

        // Moves the next burnable item from the hopper into the fire
        private bool TakeFuel()
        {
            if (_hopper == null)
                return false;

            var item = _hopper.container.FirstOrDefault(i => BaseBioReactor.GetCharge(i.item.GetTechType()) > 0f);
            if (item == null)
                return false;

            var pickupable = item.item;
            _fuel += BaseBioReactor.GetCharge(pickupable.GetTechType()) * FuelEfficiency;
            _hopper.container.RemoveItem(pickupable, true);
            Destroy(pickupable.gameObject);
            return true;
        }

        private void SaveFuel()
        {
            var id = Id;
            if (string.IsNullOrEmpty(id))
                return;
            Plugin.ModData.BurnerFuel ??= new Dictionary<string, float>();
            Plugin.ModData.BurnerFuel[id] = _fuel;
        }

        private float HopperFuel() =>
            _hopper == null ? 0f : _hopper.container.Sum(i => Mathf.Max(0f, BaseBioReactor.GetCharge(i.item.GetTechType()))) * FuelEfficiency;

        public override void OnHandHover(GUIHand hand)
        {
            if (!Constructable.constructed)
                return;

            string text;
            if (!IsEnabled) text = "Power Off";
            else if (!IsAboveWater) text = "NEEDS AIR: under water";
            else if (!HasLubricant) text = "OUT OF LUBRICANT";
            else if (_burning) text = $"Burning: {FormatTime((_fuel + HopperFuel()) * CurrentEmitIntervalSec / CurrentEmitRate)} of fuel";
            else if (_fuel <= 0f && HopperFuel() <= 0f) text = "Hopper empty: load fish or plants";
            else if (IsStorageFull) text = "Power storage full";
            else text = "Igniting...";

            text += $"\n Charge: {Mathf.RoundToInt(powerSource.GetPower())}/{Mathf.RoundToInt(powerSource.GetMaxPower())} kW";
            if (lubricantStorageController != null)
                text += $"\nLubricant: {lubricantStorageController.LubricantAmount:P}";

            HandReticle.main.SetText(HandReticle.TextType.Hand, text, false);
            HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
            HandReticle.main.SetIcon(HandReticle.IconType.Hand);
        }

        private static string FormatTime(float seconds)
        {
            var total = Mathf.CeilToInt(seconds);
            return total >= 60 ? $"{total / 60} min {total % 60:00} s" : $"{total} s";
        }

        private void SetupLid()
        {
            _lid = gameObject.FindByName("BB_HopperLid")?.transform;
            if (_lid == null)
                return;

            // The lid's pivot is its back hinge and the hinge runs along its local X: open the way that lifts it
            _lidClosed = _lid.localRotation;
            var mesh = _lid.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh != null)
            {
                var closedY = _lid.TransformPoint(mesh.bounds.center).y;
                _lid.localRotation = _lidClosed * Quaternion.AngleAxis(LidOpenAngle, Vector3.right);
                _lidSign = _lid.TransformPoint(mesh.bounds.center).y >= closedY ? 1f : -1f;
                _lid.localRotation = _lidClosed;
            }
        }

        private void SetupFire()
        {
            _noiseSeed = UnityEngine.Random.value * 100f;

            var fire = gameObject.FindByName("BB_Fire")?.GetComponent<Renderer>();
            if (fire != null)
            {
                _fireMaterial = fire.material;
                _fireMaterial.EnableKeyword("MARMO_EMISSION");
            }

            var point = gameObject.FindByName("FirePoint")?.transform ?? transform;
            var lightObj = new GameObject("FireLight");
            lightObj.transform.SetParent(point, false);
            _fireLight = lightObj.AddComponent<Light>();
            _fireLight.type = LightType.Point;
            _fireLight.color = new Color(1f, 0.55f, 0.2f);
            _fireLight.range = 4f;
            _fireLight.intensity = 0f;
            _fireLight.shadows = LightShadows.None;
            _fireLight.enabled = false;
        }

        void Update()
        {
            _flame = Mathf.MoveTowards(_flame, _burning ? 1f : 0f, Time.deltaTime * 0.5f);
            var flicker = 0.75f + 0.5f * Mathf.PerlinNoise(Time.time * 4f, _noiseSeed);

            if (_fireMaterial != null)
            {
                var glow = 0.1f + 2.4f * _flame * flicker;
                _fireMaterial.SetFloat(ShaderPropertyID._GlowStrength, glow);
                _fireMaterial.SetFloat(ShaderPropertyID._GlowStrengthNight, glow);
            }
            if (_fireLight != null)
            {
                _fireLight.intensity = 1.4f * _flame * flicker;
                _fireLight.enabled = _flame > 0.01f;
            }

            if (_lid != null)
            {
                var target = _hopper != null && _hopper.GetOpen() ? LidOpenAngle : 0f;
                _lidAngle = Mathf.MoveTowards(_lidAngle, target, Time.deltaTime * 180f);
                _lid.localRotation = _lidClosed * Quaternion.AngleAxis(_lidSign * _lidAngle, Vector3.right);
            }
        }
    }
}
