using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AD3D_LightSolution.Base.Enumerators;

namespace AD3D_LightSolution.Base
{
    public class DataItem
    {
        public string Id { get; set; }
        public string ItemType { get; set; }
        public bool IsEnable { get; private set; }
        public bool IsBaseEnabled { get; private set; } = true;
        public int SyncCode { get; private set; }
        public float R { get; private set; } = 1.0f;
        public float G { get; private set; } = 1.0f;
        public float B { get; private set; } = 1.0f;
        public float Intensity { get; private set; }


        private Color _color;
        private bool _colorUpdated = false;
        [JsonIgnore]
        public Color Color
        {
            get
            {
                if (!_colorUpdated)
                {
                    _color = new Color(R, G, B, 1.0f);
                    _colorUpdated = true;
                }
                return _color;
            }
        }

        public DateTime LastUpdate { get; private set; }

        [JsonConstructor]
        public DataItem(string id, string itemType, bool isEnable, string syncCode, float r, float g, float b, float intensity, bool isBaseEnabled = true)
        {
            Id = id;
            ItemType = itemType;
            IsEnable = isEnable;
            IsBaseEnabled = isBaseEnabled;
            SyncCode = Convert.ToInt32(syncCode);
            if(SyncCode == 0 && ItemType == SwitchItemType.Switch.ToString())
            {
                SyncCode = GenerateUniqueSyncCode();
            }
            SetColor(new Color(r, g, b));
            Intensity = intensity;
            Update();
        }

        public DataItem(string id, SwitchItemType type)
        {
            Id = id;
            ItemType = type.ToString("G");
            SyncCode = type == SwitchItemType.Switch ? GenerateUniqueSyncCode() : 0;

            Update();
        }

        public void SetEnable(bool isEnable)
        {
            if (IsEnable != isEnable)
            {
                IsEnable = isEnable;
                Update();
            }
        }

        public void SetBaseEnable(bool isBaseEnabled)
        {
            if (IsBaseEnabled != isBaseEnabled)
            {
                IsBaseEnabled = isBaseEnabled;
                Update();
            }
        }

        public void SetSyncCode(int syncCode)
        {
            if (SyncCode != syncCode)
            {
                SyncCode = syncCode;
                Update();
            }
        }

        public void SetColor(Color color)
        {
            if (R != color.r || G != color.g || B != color.b)
            {
                R = color.r;
                G = color.g;
                B = color.b;
                _colorUpdated = false; // Mark color as needing an update
                Update();
            }
        }

        public void SetIntensity(float currentIntensity)
        {
            if (Intensity != currentIntensity)
            {
                Intensity = currentIntensity;
                Update();
            }
        }

        private void Update()
        {
            LastUpdate = DateTime.Now;
        }

        private int GenerateUniqueSyncCode()
        {
            int newSyncCode;
            do
            {
                newSyncCode = UnityEngine.Random.Range(1000, 9999);
            } while (Plugin.ModData.SwitchItemList.Exists(item => item.SyncCode == newSyncCode));

            return newSyncCode;
        }
    }
}
