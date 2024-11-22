using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIWarehouseBag : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SerializableDictionary<EItemCategory, UIInventoryBagCategory> m_categories = null;
        [SerializeField] private Transform m_slotParent = null; // �������ӵĸ�����
        [SerializeField] private UIInventoryWarehouseSlot m_slotPrefab = null; // ���ӵ�Ԥ����
        private List<UIInventoryWarehouseSlot> m_slots = new List<UIInventoryWarehouseSlot>();

        //private UIInventoryWarehouseSlot[] m_slots = null;
        private EItemCategory m_category = 0;

        private void Start()
        {
            Init();

            //GameManager.NotificationSystem.UICategorySelected.AddListener(OnBagCategorySelected);
        }

        public void Init()
        {
            GenerateSlots(GameManager.WarehouseSystem.warehouseCapacity);
            FillSlots();
        }

        // Always reset to the first category when shown
        private void OnEnable() => SetCategory(0);

        public void UpdateSlots()
        {
            FillSlots(); // ֻ������Ʒ��ʾ�������´�������
        }

        private void ClearSlots()
        {
            foreach (UIInventoryWarehouseSlot slot in m_slots)
            {
                Destroy(slot.gameObject); // ɾ�����ӵ� GameObject
            }

            m_slots.Clear();
        }

        private void GenerateSlots(int capacity)
        {
            for (int i = 0; i < capacity; i++)
            {
                UIInventoryWarehouseSlot slotObject = Instantiate(m_slotPrefab, m_slotParent);
                slotObject.SetItem(null, 0);
                m_slots.Add(slotObject);
            }
        }

        private void FillSlots()
        {
            // ������и���
            foreach (var slot in m_slots)
            {
                slot.Clear();
            }

            int usedSlots = 0;

            // ��ȡ�����е���Ʒʵ��
            List<ItemInstance> items = GameManager.WarehouseSystem.warehouseItems;

            foreach (ItemInstance instance in items)
            {
                if (usedSlots >= m_slots.Count) break; // ��ֹ��λԽ��

                UIInventoryWarehouseSlot slot = m_slots[usedSlots++];
                slot.SetItem(instance.GetItem(), instance.quantity);
            }
        }

        public UIInventoryWarehouseSlot GetFirstSlot()
        {
            return m_slots.Count > 0 ? m_slots[0] : null;
        }

        public UINavigationCursorTarget FindNavigationTarget()
        {
            if (m_slots.Count > 0)
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
