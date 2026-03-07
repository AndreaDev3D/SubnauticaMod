using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AD3D_StorageSolution.Runtime
{
    public class StorageMonitorButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private ItemAndCount _item;
        private Text _textComponent;
        private StorageMonitorController _parentController;
        private string _hoverText;
        private bool _isHovered;

        public void Initialize(ItemAndCount item, StorageMonitorController parentController)
        {
            _item = item;
            _parentController = parentController;
            _hoverText = Language.main.Get(item.TechType);

            var image = transform.Find("Image")?.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = SpriteManager.Get(item.TechType);
            }

            _textComponent = transform.Find("Background/Text")?.GetComponent<Text>();
            if (_textComponent != null)
            {
                _textComponent.text = item.Count.ToString();
            }

            var button = GetComponent<Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickCallback);
        }

        private void OnClickCallback()
        {
            _parentController.OnItemButtonClicked(_item, _textComponent);
        }

        private void OnDisable()
        {
            _isHovered = false; // Reset hover state when disabled
        }

        private void Update()
        {
            if (_isHovered)
            {
                if (!this.enabled) return;

                HandReticle.main.SetText(HandReticle.TextType.Hand, $"{_hoverText}", true, GameInput.Button.LeftHand);
                HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, true);
                HandReticle.main.SetIcon(HandReticle.IconType.Hand);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
        }
    }
}
