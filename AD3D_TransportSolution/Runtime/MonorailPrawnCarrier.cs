using AD3D_Common.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AD3D_TransportSolution.Runtime
{
    // A car that carries a Prawn Suit. Its deck is a drawer: a button on either side slides it out of that open side
    // (the intermediate runners follow at half the travel), clear of the frame, so a Prawn can walk or jump onto it.
    // Get out of the Prawn on the deck and it locks on, frozen in place like the moonpool holds it, standing across
    // the track; slide the deck back in and it rides with the train. Get back in and it lets go. A carried Prawn
    // recharges slowly. Which Prawn rides on which carrier is saved. The train won't move while the deck is out.
    public class MonorailPrawnCarrier : MonorailWagon, IConstructable
    {
        // per second, as a fraction of the Prawn's capacity (about 3 minutes from empty to full)
        public const float ChargeRate = 0.005f;
        public const float DeckSpeed = 0.6f;     // m/s
        private const float DefaultTravel = 2.7f;
        private const float SettleSeconds = 0.5f;
        // where a parked Prawn is taken from, above the deck: along the car, up, across
        private static readonly Vector3 Zone = new Vector3(3.2f, 3.2f, 2.6f);
        // a saved Prawn that hasn't loaded after this long with the player nearby is gone
        private const float MissingSeconds = 20f;
        private const float NearPlayer = 60f;

        private Transform _point, _deck;
        private Transform[] _runners = new Transform[0];
        private Vector3 _deckRest;
        private Vector3[] _runnerRest = new Vector3[0];
        private Vector3 _across;                 // root space, +1 side
        private float _travel = DefaultTravel, _deckPos, _deckTarget;
        private Light _lamp;
        private Exosuit _prawn;
        private Rigidbody _body;
        private EnergyInterface _energy;
        private LargeWorldEntity _cell;
        private Vector3 _localPos, _fromPos;
        private Quaternion _localRot, _fromRot;
        private float _settle = 1f, _nextScan, _nextCell, _missing;
        private bool _logged;

        // every Prawn riding on a carrier: it keeps its parked crouch (ExosuitPatcher), which the game would drop
        // because a frozen Prawn never touches the ground, and always drops under water
        public static readonly HashSet<Exosuit> Riding = new HashSet<Exosuit>();

        public bool Carrying => _prawn != null;
        public bool HasDeck => _deck != null;
        public bool DeckIn => Mathf.Approximately(_deckTarget, 0f);
        private bool DeckMoving => !Mathf.Approximately(_deckPos, _deckTarget);

        public override bool Stowed => Mathf.Approximately(_deckPos, 0f) && DeckIn;
        public override string StowMessage => "Monorail: slide the Prawn carrier's deck in first";

        public static Shape MeasureCarrier(GameObject root) => MeasureShape(root, "CarrierCouplerRear");

        protected override Shape Measure() => MeasureCarrier(gameObject);

        protected override string Label => "Prawn carrier";

        protected override string Status => _prawn != null ? CarriedStatus
            : Track == null ? "Not on a rail"
            : HasDeck ? "Use the deck buttons on the posts to slide the deck out"
            : "Walk a Prawn Suit onto the deck and get out to lock it on";

        public string DeckStatus => _prawn != null ? CarriedStatus
            : DeckIn ? "Then walk a Prawn Suit onto it and get out to lock it on"
            : "Walk a Prawn Suit onto the deck and get out to lock it on";

        private string CarriedStatus
        {
            get
            {
                if (_energy == null)
                    return "Carrying a Prawn Suit. Get in to unload it";
                _energy.GetValues(out var charge, out var capacity);
                var pct = capacity > 0f ? Mathf.RoundToInt(100f * charge / capacity) : 0;
                return $"Carrying a Prawn Suit, {pct}% charged. Get in to unload it";
            }
        }

        private string Id => SaveIds.Of(this);
        private string IdOrNew => SaveIds.Of(this, allowNew: true);

        // the deck top: the Prawn's feet, and the frame its pose is kept in
        private Transform Anchor => _point != null ? _point : transform;

        protected override void Awake()
        {
            base.Awake();
            _across = Vector3.Cross(Vector3.up, LocalForward).normalized;
            _point = gameObject.FindByName("PrawnPoint")?.transform;
            if (_point == null)
                Plugin.Logger.LogWarning("Monorail_PrawnCarrier has no PrawnPoint: the Prawn stands on the hang point");

            // the sliding deck (older bundles: a fixed deck)
            _deck = gameObject.FindByName("MR_PrawnCarrierDeck")?.transform;
            if (_deck != null)
            {
                _deckRest = _deck.localPosition;
                _runners = GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("MR_PrawnCarrierRunner")).ToArray();
                _runnerRest = _runners.Select(t => t.localPosition).ToArray();
                var stop = gameObject.FindByName("DeckStop")?.transform;
                if (stop != null && _point != null)
                    _travel = Vector3.Distance(stop.position, _point.position);
                MoveDeckColliders();
                foreach (var name in new[] { "DeckButtonMinusY", "DeckButtonPlusY" })
                    AddDeckButton(name);
            }

            var lamp = gameObject.FindByName("ChargeLamp")?.transform;
            if (lamp != null)
            {
                _lamp = lamp.gameObject.AddComponent<Light>();
                _lamp.type = LightType.Point;
                _lamp.color = new Color(0.3f, 0.9f, 1f);
                _lamp.range = 2f;
                _lamp.intensity = 1.2f;
                _lamp.shadows = LightShadows.None;
                _lamp.enabled = false;
            }
        }

        // The prefab's lowest box is the fixed deck: it goes, and the sliding deck gets a box of its own
        private void MoveDeckColliders()
        {
            var boxes = gameObject.FindByName("Collider")?.GetComponents<BoxCollider>();
            if (boxes != null && boxes.Length > 0)
                boxes.OrderBy(b => transform.InverseTransformPoint(b.transform.TransformPoint(b.center)).y).First().enabled = false;
            var mesh = _deck.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh == null)
                return;
            var box = _deck.gameObject.AddComponent<BoxCollider>();
            box.center = mesh.bounds.center;
            box.size = mesh.bounds.size;
        }

        // A 0.25 m click area on the named empty; it slides the deck out on that empty's side
        private void AddDeckButton(string empty)
        {
            var at = gameObject.FindByName(empty)?.transform;
            if (at == null)
                return;
            var button = new GameObject(empty + "Trigger");
            button.transform.SetParent(at, false);
            button.AddComponent<BoxCollider>().size = Vector3.one * 0.25f / Mathf.Max(1e-4f, button.transform.lossyScale.x);
            var b = button.AddComponent<MonorailCarrierButton>();
            b.Carrier = this;
            b.Side = Vector3.Dot(transform.InverseTransformPoint(at.position), _across) >= 0f ? 1 : -1;
        }

        // Out on the given side when in, back in when out (or on its way out). Returns a message, or null.
        public string ToggleDeck(int side)
        {
            if (_deck == null)
                return "This carrier is from an older model and has no sliding deck";
            if (DeckIn && HitsPylon(side))
                return "The deck would hit the pylon on that side: move the train along first";
            _deckTarget = DeckIn ? side * _travel : 0f;
            return null;
        }

        // Whether the deck slid out to side (with room for a Prawn on it) would run into a pylon's column or arm
        private bool HitsPylon(int side)
        {
            var mesh = _deck.GetComponent<MeshFilter>()?.sharedMesh;
            var size = mesh != null ? Vector3.Scale(mesh.bounds.size, _deck.lossyScale) : new Vector3(2.9f, 0.12f, 2.5f);
            var forward = transform.TransformDirection(LocalForward);
            var frame = Quaternion.LookRotation(forward, Vector3.up);    // x across, y up, z along
            // the import may turn the mesh's axes: the longest side runs along the car, the thinnest is the thickness
            var sides = new[] { Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z) }.OrderBy(v => v).ToArray();
            var along = sides[2];
            var across = sides[1];
            var half = new Vector3(across, Zone.y, along) * 0.5f;
            var centre = Anchor.position + transform.TransformVector(_across * side * _travel) + Vector3.up * half.y;
            return Physics.OverlapBox(centre, half, frame, ~0, QueryTriggerInteraction.Ignore)
                .Any(c => c.GetComponentInParent<MonorailPylon>() != null);
        }

        private void PlaceDeck()
        {
            var shift = transform.TransformVector(_across * _deckPos);
            _deck.localPosition = _deckRest + _deck.parent.InverseTransformVector(shift);
            for (var i = 0; i < _runners.Length; i++)
                _runners[i].localPosition = _runnerRest[i] + _runners[i].parent.InverseTransformVector(shift * 0.5f);
        }

        protected override void Update()
        {
            base.Update();
            if (!IsBuilt)
                return;
            if (_deck != null && DeckMoving)
            {
                _deckPos = Mathf.MoveTowards(_deckPos, _deckTarget, DeckSpeed * Time.deltaTime);
                PlaceDeck();
            }
            if (_prawn != null || Time.time < _nextScan)
                return;
            _nextScan = Time.time + 0.25f;

            // a Prawn saved on this carrier: wait for it to load, then take it back
            var id = Id;
            if (!string.IsNullOrEmpty(id) && Plugin.ModData.Carried.TryGetValue(id, out var prawnId))
            {
                if (UniqueIdentifier.TryGetIdentifier(prawnId, out var uid) && uid != null)
                {
                    var p = uid.GetComponent<Exosuit>();
                    if (p != null && !p.GetPilotingMode() && !TakenElsewhere(p))
                    {
                        Lock(p, false);
                        return;
                    }
                    Plugin.ModData.Carried.Remove(id);   // someone drove it off
                    return;
                }
                if (Player.main != null && Vector3.Distance(Player.main.transform.position, transform.position) < NearPlayer)
                {
                    _missing += 0.25f;
                    if (_missing > MissingSeconds)
                    {
                        Plugin.Logger.LogInfo($"Monorail: the Prawn Suit saved on carrier {id} never loaded, forgetting it");
                        Plugin.ModData.Carried.Remove(id);
                    }
                }
                return;
            }

            if (Track != null && !DeckMoving)
                Scan();
        }

        // A parked Prawn standing on the deck
        private void Scan()
        {
            var forward = transform.TransformDirection(LocalForward);
            var frame = Quaternion.LookRotation(forward, Vector3.up);    // x across, y up, z along
            var half = new Vector3(Zone.z, Zone.y, Zone.x) * 0.5f;
            foreach (var c in Physics.OverlapBox(Anchor.position + Vector3.up * half.y, half, frame, ~0, QueryTriggerInteraction.Ignore))
            {
                var p = c.GetComponentInParent<Exosuit>();
                if (p != null && !p.GetPilotingMode() && !p.docked && !TakenElsewhere(p) && p.GetComponent<Rigidbody>() != null)
                {
                    Lock(p, true);
                    ErrorMessage.AddMessage("Monorail: Prawn Suit locked to the carrier");
                    return;
                }
            }
        }

        private bool TakenElsewhere(Exosuit p) =>
            All.OfType<MonorailPrawnCarrier>().Any(c => c != this && c._prawn == p);

        private void Lock(Exosuit p, bool settle)
        {
            _prawn = p;
            Riding.Add(p);
            _body = p.GetComponent<Rigidbody>();
            _energy = p.GetComponent<EnergyInterface>();
            _cell = p.GetComponent<LargeWorldEntity>();
            _body.isKinematic = true;
            _missing = 0f;

            // standing across the track, facing whichever side is nearer its current heading, feet on the deck
            var forward = transform.TransformDirection(LocalForward);
            var across = Vector3.Cross(Vector3.up, forward).normalized;
            var rot = Quaternion.LookRotation(Vector3.Dot(p.transform.forward, across) >= 0f ? across : -across, Vector3.up);
            var b = LocalBounds(p);
            var pos = Anchor.position + rot * new Vector3(-b.center.x, -b.min.y, -b.center.z);
            _localPos = Anchor.InverseTransformPoint(pos);
            _localRot = Quaternion.Inverse(Anchor.rotation) * rot;
            if (!_logged)
            {
                _logged = true;
                Plugin.Logger.LogInfo($"Monorail: Prawn Suit bounds {b.size.x:0.00} wide, {b.size.y:0.00} tall, {b.size.z:0.00} deep (m)");
            }

            _fromPos = p.transform.position;
            _fromRot = p.transform.rotation;
            _settle = settle ? 0f : 1f;

            var id = IdOrNew;
            var prawnId = SaveIds.Of(p, allowNew: true);
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(prawnId))
                Plugin.ModData.Carried[id] = prawnId;
        }

        // The Prawn's colliders in its own frame
        private static Bounds LocalBounds(Exosuit p)
        {
            var t = p.transform;
            Bounds? result = null;
            foreach (var c in p.GetComponentsInChildren<Collider>())
            {
                if (!c.enabled || c.isTrigger)
                    continue;
                var w = c.bounds;
                for (var i = 0; i < 8; i++)
                {
                    var corner = t.InverseTransformPoint(new Vector3(
                        (i & 1) == 0 ? w.min.x : w.max.x, (i & 2) == 0 ? w.min.y : w.max.y, (i & 4) == 0 ? w.min.z : w.max.z));
                    if (result == null)
                        result = new Bounds(corner, Vector3.zero);
                    else
                    {
                        var r = result.Value;
                        r.Encapsulate(corner);
                        result = r;
                    }
                }
            }
            return result ?? new Bounds(Vector3.zero, Vector3.zero);
        }

        // Lets go: the Prawn falls back under physics and is filed in the streaming cell it is in now
        private void Release(bool forget)
        {
            if (_body != null)
                _body.isKinematic = false;
            if (_prawn != null && LargeWorldStreamer.main != null)
                LargeWorldStreamer.main.cellManager.RegisterEntity(_prawn.gameObject);
            Riding.RemoveWhere(r => r == null || r == _prawn);
            _prawn = null;
            _body = null;
            _energy = null;
            _cell = null;
            if (_lamp != null)
                _lamp.enabled = false;
            var id = Id;
            if (forget && !string.IsNullOrEmpty(id))
                Plugin.ModData.Carried.Remove(id);
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (_prawn == null)
            {
                // unloaded with its cell (the saved link stays and picks it up again)
                if (_body != null || _lamp != null && _lamp.enabled)
                    Release(false);
                return;
            }
            if (_prawn.GetPilotingMode())
            {
                Release(true);
                return;
            }

            // after the train and the deck have moved this frame
            _body.isKinematic = true;
            var pos = Anchor.TransformPoint(_localPos);
            var rot = Anchor.rotation * _localRot;
            if (_settle < 1f)
            {
                _settle = Mathf.MoveTowards(_settle, 1f, Time.deltaTime / SettleSeconds);
                var k = Mathf.SmoothStep(0f, 1f, _settle);
                pos = Vector3.Lerp(_fromPos, pos, k);
                rot = Quaternion.Slerp(_fromRot, rot, k);
            }
            _prawn.transform.SetPositionAndRotation(pos, rot);

            // carried across the world: keep it filed in the streaming cell it is in, or it unloads with the one it left
            if (_cell != null && Time.time >= _nextCell && LargeWorldStreamer.main != null)
            {
                _nextCell = Time.time + 1f;
                _cell.UpdateCell(LargeWorldStreamer.main);
            }

            var charging = false;
            if (_energy != null)
            {
                _energy.GetValues(out var charge, out var capacity);
                if (charge < capacity)
                {
                    // TODO: draw this from the nearest power source (a base or power relay in range) instead of free
                    _energy.AddEnergy(Mathf.Min(capacity - charge, capacity * ChargeRate * Time.deltaTime));
                    charging = true;
                }
            }
            if (_lamp != null)
            {
                _lamp.enabled = true;
                _lamp.intensity = charging ? 0.6f + 0.6f * Mathf.PingPong(Time.time, 1f) : 1.2f;
            }
        }

        // Deconstruction started: let the Prawn go (the game disables this component at the same moment)
        public void OnConstructedChanged(bool constructed)
        {
            if (!constructed && _prawn != null)
                Release(true);
        }

        public bool IsDeconstructionObstacle() => false;

        public bool CanDeconstruct(out string reason)
        {
            reason = _prawn != null ? "A Prawn Suit is locked on: get in and walk it off first" : null;
            return _prawn == null;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_prawn != null)
                Release(false);
        }
    }
}
