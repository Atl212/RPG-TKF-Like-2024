using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIInventoryBag : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SerializableDictionary<EItemCategory, UIInventoryBagCategory> m_categories = null;

        private UIInventoryBagSlot[] m_slots = null;
        private EItemCategory m_category = 0;

        private void Start()
        {
            GameManager.NotificationSystem.UICategorySelected.AddListener(OnBagCategorySelected);
        }

        public void Init()
        {
            m_slots = GetComponentsInChildren<UIInventoryBagSlot>();

            // 没get到他为什么要设置为反过来 我在grid组件里面重新设置为 topleft 然后把这个b代码给注释掉了
            // Because we display slots from bottom right to top left, we need to reverse them here to make sure we fill
            // them from top left to bottom right.
            //Array.Reverse(m_slots);

            foreach (var category in m_categories)
            {
                category.Value.SetCategory(category.Key);
            }
        }

        // Always reset to the first category when shown
        private void OnEnable() => SetCategory(0);

        public void UpdateSlots()
        {
            ClearSlots();
            FillSlots();
        }

        private void ClearSlots()
        {
            foreach (UIInventoryBagSlot slot in m_slots)
            {
                slot.Clear();
            }
        }

        private void FillSlots()
        {
            int usedSlots = 0;

            List<ItemInstance> items = GameManager.InventorySystem.backpackItems;

            foreach (ItemInstance instance in items)
            {
                if (instance.GetItem().category == m_category)
                {
                    UIInventoryBagSlot slot = m_slots[usedSlots++];
                    slot.SetItem(instance.GetItem(), instance.quantity);

                    // 检查槽位是否用完，防止数组越界
                    if (usedSlots >= m_slots.Count())
                    {
                        break;
                    }
                }
            }
        }

        public UIInventoryBagSlot GetFirstSlot()
        {
            return m_slots.Length > 0 ? m_slots[0] : null;
        }

        public UINavigationCursorTarget FindNavigationTarget()
        {
            if (m_slots.Length > 0)
            {
                return m_slots[0].gameObject.GetComponentInChildren<UINavigationCursorTarget>();
            }

            return null;
        }

        public void SetCategory(EItemCategory category)
        {
            // Make sure this category is available in the bag
            if (!m_categories.ContainsKey(category))
            {
                Debug.LogWarning($"Category {category} not found in the bag");
                return;
            }
            
            foreach (var entry in m_categories)
            {
                entry.Value.SetHighlight(false);
            }

            m_category = category;
            m_categories[m_category].SetHighlight(true);

            UpdateSlots();
        }

        private void OnBagCategorySelected(EItemCategory category) => SetCategory(category);
    }
}
