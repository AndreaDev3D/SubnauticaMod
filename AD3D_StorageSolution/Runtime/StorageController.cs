using AD3D_Common.Utils;
using UnityEngine.UI;
using UnityEngine;

namespace AD3D_StorageSolution.Runtime
{
    public class StorageController : StorageContainer, IProtoEventListener
    {
        public static event System.Action OnStorageChanged;

        private Image Icon;
        private bool _isInited;
        private PrefabIdentifier _prefabIdentifier;

        public TechType Filter { get; private set; } = TechType.None;

        public void Start()
        {
            foreach (var rb in GetComponentsInChildren<Rigidbody>(true))
                Destroy(rb);

            Icon = gameObject.FindComponentByName<Image>("Icon");
            _prefabIdentifier = GetComponentInParent<PrefabIdentifier>();

            if (_prefabIdentifier != null && Plugin.ModData.StorageFilters.TryGetValue(_prefabIdentifier.Id, out var savedFilter))
            {
                Filter = savedFilter;
            }
        }

        public void SetFilter(TechType techType)
        {
            Filter = techType;
            if (_prefabIdentifier != null)
            {
                Plugin.ModData.StorageFilters[_prefabIdentifier.Id] = techType;
            }
            SetIcon();
            OnStorageChanged?.Invoke();
        }

        public void Update()
        {
            if (!_isInited)
            {
                if (container != null)
                {
                    _isInited = true;
                    container.Sort();
                    SetIcon();

                    // Refresh StorageMonitorController list
                    container.onAddItem += (item) => 
                    {
                        if (Filter == TechType.None)
                        {
                            SetFilter(item.item.GetTechType());
                        }
                        OnStorageChanged?.Invoke();
                    };
                    container.onRemoveItem += (item) => OnStorageChanged?.Invoke();

                    if(Filter == TechType.None)
                    {
                        var firstItem = container.itemsMap[0, 0];
                        if (firstItem != null)
                        {
                             SetFilter(firstItem.techType);
                        }
                    }
                }

            }
        }

        public override void OnClose()
        {
            base.OnClose();
            SetIcon();
        }

        public void SetIcon()
        {
            if(Icon == null)
            {

                Plugin.Logger.LogError($"SetIcon Icon is null");
            }
            if (container == null)
            {

                Plugin.Logger.LogError($"container Icon is null");
            }
            if (container == null)
            {

                Plugin.Logger.LogError($"container.itemsMap is null");
            }

            if (Icon != null && (container != null || container.itemsMap != null))
            {
                var firstItem = container.itemsMap[0, 0];
                if (firstItem != null)
                {
                    Icon.sprite = SpriteManager.Get(firstItem.techType);
                    Icon.enabled = true;
                }
                else if (Filter != TechType.None)
                {
                    Icon.sprite = SpriteManager.Get(Filter);
                    Icon.enabled = true;
                }
                else
                {
                    Icon.enabled = false;
                }
            }
            else
            {
                Plugin.Logger.LogError($"SetIcon something is broken");
            }
        }

        public new void OnHandHover(GUIHand hand)
        {
#if SN
            if (!this.enabled)
                return;
#elif BZ
                if (!this.enabled || this.disableUseability)
                return;
#endif

            Constructable component = this.gameObject.GetComponent<Constructable>();
            if ((bool)(UnityEngine.Object)component && !component.constructed)
                return;

            var firstItem = container.itemsMap[0, 0];
            var extraText = "";
            var techType = TechType.None;

            if (firstItem != null)
            {
                techType = firstItem.techType;
            }
            else if (Filter != TechType.None)
            {
                techType = Filter;
            }

            if (techType != TechType.None)
            {
                extraText = $"\n {container.GetCount(techType)}/{container.count}";
            }

            HandReticle.main.SetText(HandReticle.TextType.Hand, $"{this.hoverText}{extraText}", true, GameInput.Button.LeftHand);
            HandReticle.main.SetText(HandReticle.TextType.HandSubscript, this.IsEmpty() ? "Empty" : string.Empty, true);

            var heldItem = Inventory.main.GetHeld();
            if (heldItem != null)
            {
                var heldTechType = heldItem.inventoryItem.item.GetTechType();
                if (heldTechType != Filter)
                {
                    HandReticle.main.SetText(HandReticle.TextType.HandSubscript, $"Filter by: {Language.main.Get(heldTechType)}", true, GameInput.Button.RightHand);
                }
                else
                {
                    HandReticle.main.SetText(HandReticle.TextType.HandSubscript, $"Clear Filter", true, GameInput.Button.RightHand);
                }
            }

            HandReticle.main.SetIcon(HandReticle.IconType.Hand);
        }

        public new void OnHandClick(GUIHand hand)
        {
            if (GameInput.GetButtonDown(GameInput.Button.RightHand))
            {
                 var heldItem = Inventory.main.GetHeld();
                 if (heldItem != null)
                 {
                     var heldTechType = heldItem.inventoryItem.item.GetTechType();
                     if (heldTechType == Filter)
                     {
                         SetFilter(TechType.None);
                         Plugin.Logger.LogInfo($"[StorageController] Filter cleared for {_prefabIdentifier?.Id}");
                     }
                     else
                     {
                         SetFilter(heldTechType);
                         Plugin.Logger.LogInfo($"[StorageController] Filter set to {heldTechType} for {_prefabIdentifier?.Id}");
                     }
                 }
                 return;
            }

            // Default behavior is inherited via StorageContainer but let's be explicit if needed
            // base.OnHandClick(hand); 
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (_prefabIdentifier == null || string.IsNullOrEmpty(_prefabIdentifier.Id)) return;

            Plugin.ModData.StorageFilters[_prefabIdentifier.Id] = Filter;
            Plugin.ModData.Save();
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            if (_prefabIdentifier == null || string.IsNullOrEmpty(_prefabIdentifier.Id)) return;

            if (Plugin.ModData.StorageFilters.TryGetValue(_prefabIdentifier.Id, out var savedFilter))
            {
                Filter = savedFilter;
                SetIcon();
            }
        }
    }
}
