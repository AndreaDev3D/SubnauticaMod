using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AD3D_EnergySolution.Runtime
{
    public class LubricantStorageController : StorageContainer, IProtoEventListener
    {
        public string Id 
        {
            get
            {
                var identifier = gameObject.GetComponent<PrefabIdentifier>() ?? gameObject.GetComponentInParent<PrefabIdentifier>();
                return identifier?.Id;
            }
        }

        public float LubricantAmount = 0f;
        private float _maxLubricantAmount = 1f;
        public Transform LubricantAmountObj;

        private bool _NeedsCleanUp = false;

        void Start()
        {
            this.container.isAllowedToAdd += new global::IsAllowedToAdd(this.IsAllowedToAdd);
            this.container.isAllowedToRemove += new global::IsAllowedToRemove(this.IsAllowedToRemove);
            this.container.onAddItem += AddLubricant;

            LubricantAmountObj = transform.Find("LubricantAmount");

            if (!string.IsNullOrEmpty(Id) && Plugin.ModData.LubricantLevels != null && Plugin.ModData.LubricantLevels.TryGetValue(Id, out float savedAmount))
            {
                LubricantAmount = savedAmount;
            }

            if (LubricantAmountObj != null && LubricantAmount >= 0)
            {
                SetLubricantAmount(0); // Pass 0 to just update the local scale visually
                Plugin.Logger.LogError($"LubricantAmount OnStart: {LubricantAmount} for {Id}");
            }
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose) => pickupable.GetTechType() == TechType.Lubricant;

        private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
        {
            if (LubricantAmount >= 0.25f)
            {
                SetLubricantAmount(-0.25f);
                return true;
            }
            return false;
        }


        private void AddLubricant(InventoryItem inventoryItem)
        {
            SetLubricantAmount(0.25f);
            _NeedsCleanUp = true;
        }

        public override void OnClose()
        {
            base.OnClose();
            if (_NeedsCleanUp)
            {
                container.Clear(true); 
                _NeedsCleanUp = false;
            }
        }

        public float SetLubricantAmount(float amount)
        {
            LubricantAmount += amount;

            if (LubricantAmount > _maxLubricantAmount)
                LubricantAmount = _maxLubricantAmount;

            if (LubricantAmount < 0)
            {
                LubricantAmount = 0;
                container.Clear();
            }


            LubricantAmountObj.transform.localScale = new Vector3(1, 1, LubricantAmount);

            // Save to GameData
            if (!string.IsNullOrEmpty(Id))
            {
                if (Plugin.ModData.LubricantLevels == null)
                    Plugin.ModData.LubricantLevels = new Dictionary<string, float>();
                
                Plugin.ModData.LubricantLevels[Id] = LubricantAmount;
            }

            return LubricantAmount;
        }

        // Saving
        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (string.IsNullOrEmpty(Id)) return;

            if (Plugin.ModData.LubricantLevels == null)
            {
                Plugin.ModData.LubricantLevels = new Dictionary<string, float>();
            }

            Plugin.ModData.LubricantLevels[Id] = LubricantAmount;
            Plugin.ModData.Save();
        }

        // Loading
        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            Plugin.Logger.LogError($"OnDeserialize Called");

            if (string.IsNullOrEmpty(Id))
            {
                Plugin.Logger.LogError($"OnDeserialize: {Id} missing");
                return;
            }
            

            if (Plugin.ModData.LubricantLevels != null && 
                Plugin.ModData.LubricantLevels.TryGetValue(Id, out float savedAmount))
            {
                LubricantAmount = savedAmount;
                Plugin.Logger.LogError($"LubricantAmount: {LubricantAmount} for {Id}");
            }
        }
    }
}
