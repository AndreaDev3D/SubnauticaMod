using AD3D_Common.Utils;
using UnityEngine;
using System.Collections.Generic;

namespace AD3D_StorageSolution.Runtime
{
    public class ResourceUploaderController : StorageContainer
    {
        public void Start()
        {
            foreach (var rb in GetComponentsInChildren<Rigidbody>(true))
                Destroy(rb);
        }

        public override void OnClose()
        {
            base.OnClose();

            if (container == null || container.count == 0) return;

            var storageControllers = FindObjectsOfType<StorageController>();
            if (storageControllers == null || storageControllers.Length == 0) return;

            // Group storages by TechType filter for faster lookup
            var filterMap = new Dictionary<TechType, List<StorageController>>();
            foreach (var sc in storageControllers)
            {
                if (sc.Filter == TechType.None || sc.container == null) continue;
                if (!filterMap.ContainsKey(sc.Filter)) filterMap[sc.Filter] = new List<StorageController>();
                filterMap[sc.Filter].Add(sc);
            }

            // Get all items currently in the uploader
            var itemsToTransfer = new List<InventoryItem>();
            foreach (var item in container)
            {
                itemsToTransfer.Add(item);
            }

            foreach (var inventoryItem in itemsToTransfer)
            {
                var techType = inventoryItem.item.GetTechType();
                if (filterMap.TryGetValue(techType, out var targets))
                {
                    foreach (var target in targets)
                    {
                        if (target.container.HasRoomFor(inventoryItem.item))
                        {
                            // Move the item
                            if (container.RemoveItem(inventoryItem.item))
                            {
                                if (target.container.AddItem(inventoryItem.item) != null)
                                {
                                    Plugin.Logger.LogInfo($"[ResourceUploader] Transferred {techType} to filtered storage.");
                                    break; // Successfully moved, continue to next item in uploader
                                }
                                else
                                {
                                    // Fallback: if adding failed for some reason, put it back in uploader
                                    container.AddItem(inventoryItem.item);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
