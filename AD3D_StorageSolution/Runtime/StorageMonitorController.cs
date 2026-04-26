using Nautilus.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

#if SN
using static CraftData;
#endif

namespace AD3D_StorageSolution.Runtime
{
    public class ItemAndCount
    {
        public TechType TechType;
        public int Count;
    }

    public class StorageMonitorController : MonoBehaviour
    {
        public GameObject ItemButtonPrefab;
        public Transform InnerPanel;
        public RectTransform InnerPanelRect;

        public List<ItemAndCount> GlobalItemList = new List<ItemAndCount>();

        public int GridX = 3;
        public int GridY = 3;

        private Vector3 stepVector;
        private float panelwidth;
        private Coroutine _scrollCoroutine;
        public float scrollDuration = 0.25f;

        public void Init(GameObject itemButtonPrefab)
        {
            ItemButtonPrefab = itemButtonPrefab;
            InnerPanel = this.gameObject.transform.SearchChild("InnerPanel");
            InnerPanelRect = InnerPanel.GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            StorageController.OnStorageChanged += RefreshList;
        }

        private void OnDisable()
        {
            StorageController.OnStorageChanged -= RefreshList;
        }

        private void Start()
        {
            foreach (var rb in GetComponentsInChildren<Rigidbody>(true))
                Destroy(rb);

            panelwidth = InnerPanelRect.rect.width;
            stepVector = new Vector3(panelwidth / GridX, 0);
            
            var refreshButton = this.gameObject.transform.SearchChild("btnRefresh");
            refreshButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                RefreshList();
            });

            var btnLeft = this.gameObject.transform.SearchChild("btnLeft")?.GetComponent<Button>();
            btnLeft.onClick.AddListener(NextPage);

            var btnRight = this.gameObject.transform.SearchChild("btnRight")?.GetComponent<Button>();
            btnRight.onClick.AddListener(PreviousPage);

            RefreshList();
        }

        private void PreviousPage()
        {
            if (_scrollCoroutine != null) return;

            int totalColumns = Mathf.CeilToInt((float)GlobalItemList.Count / GridY);
            int maxShifts = Mathf.Max(0, totalColumns - GridX);
            float minX = -maxShifts * stepVector.x;

            if (InnerPanelRect.localPosition.x <= minX + 0.01f) return;

            var targetPosition = InnerPanelRect.localPosition - stepVector;
            _scrollCoroutine = StartCoroutine(SmoothScroll(targetPosition));
        }

        private void NextPage()
        {
            if (_scrollCoroutine != null) return;
            if (InnerPanelRect.localPosition.x >= -0.01f) return;
            var targetPosition = InnerPanelRect.localPosition + stepVector;
            _scrollCoroutine = StartCoroutine(SmoothScroll(targetPosition));
        }

        private System.Collections.IEnumerator SmoothScroll(Vector3 targetPosition)
        {
            float elapsedTime = 0f;
            Vector3 startingPosition = InnerPanelRect.localPosition;

            while (elapsedTime < scrollDuration)
            {
                InnerPanelRect.localPosition = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / scrollDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            InnerPanelRect.localPosition = targetPosition;
            _scrollCoroutine = null;
        }

        public void RefreshList()
        {
            GlobalItemList.Clear();
            var itemCounts = new Dictionary<TechType, int>();
            var storageControllers = FindObjectsOfType<StorageController>();
            
            int totalItems = 0;
            int validStorages = 0;

            foreach (var controller in storageControllers)
            {
                if (controller.container == null) continue;
                
                validStorages++;
                var types = controller.container.GetItemTypes();
                if (types != null)
                {
                    foreach (var techType in types)
                    {
                        int count = controller.container.GetCount(techType);
                        totalItems += count;
                        if (itemCounts.ContainsKey(techType))
                        {
                            itemCounts[techType] += count;
                        }
                        else
                        {
                            itemCounts[techType] = count;
                        }
                    }
                }
            }

            Plugin.Logger.LogInfo($"[StorageMonitorController] RefreshList Complete - Found {validStorages} valid StorageControllers (out of {storageControllers.Length} total components) containing {totalItems} total items.");

            foreach (var kvp in itemCounts)
            {
                GlobalItemList.Add(new ItemAndCount { TechType = kvp.Key, Count = kvp.Value });
            }

            if (InnerPanel != null)
            {
                foreach (Transform child in InnerPanel)
                {
                    Destroy(child.gameObject);
                }

                foreach (var item in GlobalItemList)
                {
                    var buttonGo = Instantiate(ItemButtonPrefab, InnerPanel, false);
                    
                    var rectTransform = buttonGo.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.localScale = Vector3.one;
                        rectTransform.localRotation = Quaternion.identity;
                        rectTransform.localPosition = Vector3.zero;
                    }

                    var buttonController = buttonGo.GetComponent<StorageMonitorButtonController>();
                    if (buttonController == null)
                    {
                        buttonController = buttonGo.AddComponent<StorageMonitorButtonController>();
                    }

                    buttonController.Initialize(item, this);
                }

                int totalColumns = Mathf.CeilToInt((float)GlobalItemList.Count / GridY);
                int maxShifts = Mathf.Max(0, totalColumns - GridX);
                float minX = -maxShifts * stepVector.x;

                if (InnerPanelRect.localPosition.x < minX)
                {
                    InnerPanelRect.localPosition = new Vector3(minX, 0, 0);
                }
            }
        }

        public void OnItemButtonClicked(ItemAndCount item, Text textComponent)
        {
            if (item.Count <= 0) return;

            var storageControllers = FindObjectsOfType<StorageController>();
            foreach (var controller in storageControllers)
            {
                if (controller.container == null) continue;

                if (controller.container.Contains(item.TechType))
                {
                    if (controller.container.DestroyItem(item.TechType))
                    {
                        CraftData.AddToInventory(item.TechType);
                        item.Count--;
                        
                        if (textComponent != null)
                        {
                            textComponent.text = item.Count.ToString();
                        }
                        break;
                    }
                }
            }
        }
    }
}
