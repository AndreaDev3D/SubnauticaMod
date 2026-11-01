using System.Linq;
using UnityEngine;

namespace AD3D_IndustrySolution.Runtime
{
    // Same mechanism as the Deep Engine (AD3D_EnergySolution): each Lubricant dropped in adds
    // LubricantPerItem to the level, and the items are consumed when the player closes the slot.
    public class LubricantController : StorageContainer
    {
        public float LubricantPerItem = 0.25f;
        public float MaxLubricant = 1f;

        public float LubricantAmount { get; private set; }

        private bool _isConsuming;

        private string Id => GetComponentInParent<PrefabIdentifier>()?.Id;

        void Start()
        {
            container.isAllowedToAdd += IsAllowedToAdd;
            container.onAddItem += _ => SetLubricantAmount(LubricantAmount + LubricantPerItem);
            container.onRemoveItem += _ =>
            {
                // Taking an item back out before closing refunds it
                if (!_isConsuming)
                    SetLubricantAmount(LubricantAmount - LubricantPerItem);
            };

            if (!string.IsNullOrEmpty(Id) && Plugin.ModData.LubricantLevels.TryGetValue(Id, out var saved))
                LubricantAmount = saved;
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose) =>
            pickupable.GetTechType() == TechType.Lubricant && LubricantAmount + LubricantPerItem <= MaxLubricant + 0.001f;

        public override void OnClose()
        {
            base.OnClose();

            _isConsuming = true;
            foreach (var item in container.ToList())
            {
                container.RemoveItem(item.item, true);
                Destroy(item.item.gameObject);
            }
            _isConsuming = false;
        }

        public bool HasLubricant(float amount) => LubricantAmount >= amount;

        public void Consume(float amount) => SetLubricantAmount(LubricantAmount - amount);

        private void SetLubricantAmount(float amount)
        {
            LubricantAmount = Mathf.Clamp(amount, 0f, MaxLubricant);

            if (!string.IsNullOrEmpty(Id))
                Plugin.ModData.LubricantLevels[Id] = LubricantAmount;
        }
    }
}
