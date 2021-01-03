using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AD3D_LightSolutionMod.BO.Base
{
    public class DataItem
    {
        public string Id { get; set; }
        public string ItemType { get; set; }
        public bool IsEnable { get; internal set; }
        public int SyncCode { get; internal set; }
        [JsonIgnore]
        public Color Color => new Color(R, G, B, 1.0f);
        public float R { get; internal set; } = 1;
        public float G { get; internal set; } = 1;
        public float B { get; internal set; } = 1;
        public float Intensity { get; internal set; }
        public virtual DateTime LastUpdate { get; internal set; }

        [JsonConstructor]
        public DataItem(string id, SwitchItemType type)
        {
            Id = id;
            ItemType = type.ToString("G");
            SyncCode = type== SwitchItemType.Switch ? GetSyncCode() : 0;
            Update();
        }

        public void SetEnable(bool isEnable)
        {
            IsEnable = isEnable;
            Update();
        }

        public void SetSyncCode(int syncCode)
        {
            SyncCode = syncCode;
            Update();
        }

        public void SetColor(Color color)
        {
            R = color.r;
            G = color.g;
            B = color.b;
            Update();
        }

        public void SetIntensity(float currentIntensity)
        {
            Intensity = currentIntensity;
            Update();
        }

        private void Update()
        {
            LastUpdate = DateTime.Now;
        }

        //public Color GetColor()
        //{
        //    return new Color(R, G, B, 1.0f);
        //}

        private int GetSyncCode()
        {
            var newSyncCode = UnityEngine.Random.Range(1000, 9999);
            foreach (var item in QPatch.Database.SwitchItemList)
            {
                if (item.SyncCode == newSyncCode)
                {
                    newSyncCode = UnityEngine.Random.Range(1000, 9999);
                }
            }
            return newSyncCode;
        }
    }

    public enum SwitchItemType
    {
        Switch, Source
    }
}
