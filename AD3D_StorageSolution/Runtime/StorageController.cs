using AD3D_Common.Utils;
using UnityEngine.UI;

namespace AD3D_StorageSolution.Runtime
{
    public class StorageController : StorageContainer
    {
        public static event System.Action OnStorageChanged;

        private Image Icon;

        private bool _isInited;

        public void Start()
        {
            Icon = gameObject.FindComponentByName<Image>("Icon");
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
                    container.onAddItem += (item) => OnStorageChanged?.Invoke();
                    container.onRemoveItem += (item) => OnStorageChanged?.Invoke();
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
                if (firstItem == null)
                {
                    return;
                }
                Icon.sprite = SpriteManager.Get(firstItem.techType);

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
            if (firstItem == null)
            {
                extraText = $"\n {container.GetCount(firstItem.techType)}/{container.count}";
            }

            HandReticle.main.SetText(HandReticle.TextType.Hand, $"{this.hoverText}{extraText}", true, GameInput.Button.LeftHand);
            HandReticle.main.SetText(HandReticle.TextType.HandSubscript, this.IsEmpty() ? "Empty" : string.Empty, true);
            HandReticle.main.SetIcon(HandReticle.IconType.Hand);
        }
    }
}
