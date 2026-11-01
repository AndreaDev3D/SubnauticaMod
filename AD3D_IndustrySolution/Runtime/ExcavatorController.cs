using AD3D_Common.Utils;
using AD3D_IndustrySolution.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AD3D_IndustrySolution.Runtime
{
    public class DrillEntry
    {
        public TechType TechType;
        public float Weight;

        public DrillEntry(TechType techType, float weight)
        {
            TechType = techType;
            Weight = weight;
        }
    }

    public class ExcavatorController : MonoBehaviour, IHandTarget
    {
        public enum State { Off, Building, NoStorage, NoLubricant, NoPower, StorageFull, Drilling }

        // Tuning: set per prefab in Plugin.cs
        public float CycleSeconds = 60f;
        public float PowerPerSecond = 0.25f; // at full power; with less, the cycle advances proportionally slower
        public float MinPower = 0.01f;       // below this the machine stops
        public float LubricantPerCycle = 0.02f;
        // What it drills comes from DrillTables, by biome

        // The drill starts each cycle partly down and reaches full depth when the cycle completes
        public float StartExtension = 0.2f;

        public bool IsEnabled = true;
        public State CurrentState { get; private set; } = State.Building;
        public float PowerRatio { get; private set; } // share of PowerPerSecond received on the last tick

        private float _progress;
        private bool _isDepositing;

        private Constructable _constructable;
        private PowerRelay _powerRelay;
        private StorageContainer _storage;
        private LubricantController _lubricant;
        private ExcavatorAnimator _animator;

        // Whichever connected relay has the most power: this relay (sources linked into it), the end of the chain it
        // links on to (a base or transmitter chain), or a base it's parented to
        private PowerRelay PowerNetwork
        {
            get
            {
                if (_powerRelay == null)
                    return null;

                var best = _powerRelay;
                foreach (var candidate in new[] { _powerRelay.GetEndpoint(), PowerSource.FindRelay(transform) })
                {
                    if (candidate != null && candidate.GetPower() > best.GetPower())
                        best = candidate;
                }
                return best;
            }
        }

        private float _nextPowerLog;

        private void LogPowerDiagnostics()
        {
            if (Time.time < _nextPowerLog)
                return;
            _nextPowerLog = Time.time + 10f;

            string Describe(PowerRelay r) => r == null ? "none" : $"{r.name} ({r.GetType().Name}, power {r.GetPower():0.#}/{r.GetMaxPower():0.#}, powered {r.IsPowered()})";

            if (_powerRelay == null)
            {
                Plugin.Logger.LogWarning("Excavator power: no PowerRelay on the machine");
                return;
            }

            var inbound = _powerRelay.inboundPowerSources;
            var inboundText = inbound == null || inbound.Count == 0
                ? "none"
                : string.Join(", ", inbound.Select(s => $"{(s as Component)?.name ?? s.GetType().Name} ({s.GetPower():0.#})"));

            Plugin.Logger.LogWarning($"Excavator power: relay {Describe(_powerRelay)} | outbound {Describe(_powerRelay.outboundRelay)} | " +
                $"endpoint {Describe(_powerRelay.GetEndpoint())} | base {Describe(PowerSource.FindRelay(transform))} | inbound {inboundText}");
        }

        private void Start()
        {
            _constructable = GetComponent<Constructable>();
            _powerRelay = GetComponent<PowerRelay>();
            _storage = GetComponentsInChildren<StorageContainer>(true).FirstOrDefault(s => !(s is LubricantController));
            _lubricant = GetComponentInChildren<LubricantController>(true);
            _animator = GetComponent<ExcavatorAnimator>();

            InvokeRepeating(nameof(Tick), 1f, 1f);
        }

        private void Tick()
        {
            CurrentState = EvaluateState();
            if (CurrentState == State.NoPower)
                LogPowerDiagnostics();
            if (CurrentState != State.Drilling)
            {
                PowerRatio = 0f;
                UpdateAnimator();
                return;
            }

            // Take what the network can give, up to the full rate, and advance by the share received
            var network = PowerNetwork;
            network.ConsumeEnergy(Mathf.Min(PowerPerSecond, network.GetPower()), out var consumed);
            PowerRatio = Mathf.Clamp01(consumed / PowerPerSecond);
            _progress += PowerRatio;
            UpdateAnimator();

            if (_progress >= CycleSeconds && !_isDepositing)
            {
                _progress = 0f;
                _lubricant.Consume(LubricantPerCycle);
                StartCoroutine(DepositResource());
            }
        }

        private State EvaluateState()
        {
            if (_constructable != null && !_constructable.constructed) return State.Building;
            if (!IsEnabled) return State.Off;
            if (_storage == null || _lubricant == null) return State.NoStorage;
            if (!_lubricant.HasLubricant(LubricantPerCycle)) return State.NoLubricant;
            if (PowerNetwork == null || PowerNetwork.GetPower() < MinPower) return State.NoPower;
            if (!_storage.container.HasRoomFor(1, 1)) return State.StorageFull;
            return State.Drilling;
        }

        private IEnumerator DepositResource()
        {
            _isDepositing = true;

            var biome = GetBiome();
            var techType = RollResource(DrillTables.Get(biome));
            Plugin.Logger.LogInfo($"Excavator drilled {techType} in biome '{biome}'");

            if (techType != TechType.None)
            {
                var task = CraftData.GetPrefabForTechTypeAsync(techType, false);
                yield return task;

                var prefab = task.GetResult();
                if (prefab != null)
                {
                    var pickupable = Instantiate(prefab).GetComponent<Pickupable>();
                    pickupable.Pickup(false);
                    _storage.container.AddItem(pickupable);
                }
            }

            _isDepositing = false;
        }

        // Weighted pick; a TechType.None entry in the table means "nothing found"
        private static TechType RollResource(List<DrillEntry> table)
        {
            var total = table.Sum(e => e.Weight);
            if (total <= 0f)
            {
                Plugin.Logger.LogWarning("Excavator: drill table is empty, nothing to drill");
                return TechType.None;
            }

            var roll = UnityEngine.Random.value * total;
            foreach (var entry in table)
            {
                roll -= entry.Weight;
                if (roll <= 0f)
                    return entry.TechType;
            }
            return table[table.Count - 1].TechType;
        }

        // Logged for now, to build the per-biome drill tables later
        private string GetBiome() => LargeWorld.main != null ? LargeWorld.main.GetBiome(transform.position) : "unknown";

        private void UpdateAnimator()
        {
            if (_animator == null)
                return;

            _animator.IsDrilling = CurrentState == State.Drilling;
            _animator.PowerRatio = PowerRatio;
            _animator.TargetExtension = Mathf.Lerp(StartExtension, 1f, Mathf.Clamp01(_progress / CycleSeconds));
        }

        public void OnHandHover(GUIHand hand)
        {
            if (CurrentState == State.Building)
                return;

            var status = CurrentState switch
            {
                State.Off => "Off",
                State.NoStorage => "Storage or Lubricant point missing in prefab",
                State.NoLubricant => "Needs lubricant",
                State.NoPower => "No power",
                State.StorageFull => "Storage full",
                _ => $"Drilling {Mathf.FloorToInt(_progress)}/{Mathf.RoundToInt(CycleSeconds)}s" +
                     (PowerRatio < 0.99f ? $" (low power {PowerRatio:P0})" : string.Empty),
            };

            var text = $"Mech Excavator: {status}";
            if (_lubricant != null)
                text += $"\nLubricant: {_lubricant.LubricantAmount:P0}";
            text += PowerNetwork != null ? $"\nPower: {Mathf.RoundToInt(PowerNetwork.GetPower())}" : "\nPower: no relay";
            text += $"\n(click to turn {(IsEnabled ? "off" : "on")})";

            HandReticle.main.SetText(HandReticle.TextType.Hand, text, false);
            HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
            HandReticle.main.SetIcon(HandReticle.IconType.Hand);
        }

        public void OnHandClick(GUIHand hand)
        {
            if (CurrentState == State.Building)
                return;

            IsEnabled = !IsEnabled;
            CurrentState = EvaluateState();
            UpdateAnimator();
        }
    }
}
