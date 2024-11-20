using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public struct WarehouseDataBlock
    {
        public int money;
        public SerializableDictionary<DatabaseEntryReference<StringDatabaseEntryReference>, int> items;
    }

    public class WarehouseSystem : AGameSystem, IDataBlockHandler<WarehouseDataBlock>
    {
        public int warehouseMoney => m_warehouseMoney;
        public Dictionary<string, int> warehouseItems => m_warehouseItems;


        private int m_warehouseMoney = 0;
        private Dictionary<string, int> m_warehouseItems = new Dictionary<string, int>();

        public bool isOpenning = false;

        public int GetItemCount(Item item)
        {
            // �����Ʒ�ɶѵ���ͨ���ѵ�����������
            if (item.isStackable)
            {
                if (warehouseItems.TryGetValue(item.uniqueID, out int count))
                {
                    return count;
                }
            }
            else
            {
                // ���ڲ��ɶѵ���Ʒ��������Ʒʵ��������
                return warehouseItems
                    .Where(kvp => kvp.Key == item.uniqueID)
                    .Count();
            }

            return 0;
        }

        public void AddMoney(int value)
        {
            if (value > 0)
            {
                m_warehouseMoney += value;
                //GameManager.NotificationSystem.moneyAdded.Invoke(value);
            }
        }

        public void RemoveMoney(int value)
        {
            if (value > 0)
            {
                m_warehouseMoney = math.max(warehouseMoney - value, 0);
                //GameManager.NotificationSystem.moneyRemoved.Invoke(value);
            }
        }

        public bool TryRemoveMoney(int value)
        {
            if (HasSufficientFunds(value))
            {
                RemoveMoney(value);
                return true;
            }

            return false;
        }

        public bool HasSufficientFunds(int value)
        {
            return value <= warehouseMoney;
        }

        public bool HasItemInWarehouse(Item item, int quantity = 1)
        {
            if (item.isStackable)
            {
                return warehouseItems.ContainsKey(item.uniqueID) && warehouseItems[item.uniqueID] >= quantity;
            }
            else
            {
                // ���ڲ��ɶѵ���Ʒ������Ƿ����㹻�Ĳ�ͬʵ��
                return warehouseItems
                    .Where(kvp => kvp.Key == item.uniqueID)
                    .Count() >= quantity;
            }
        }

        public void AddToWarehouse(Item item, int quantity = 1, bool forceNoEvent = false)
        {
            if (item.isStackable)
            {
                // �ɶѵ���Ʒ����������
                if (!warehouseItems.ContainsKey(item.uniqueID))
                {
                    warehouseItems.Add(item.uniqueID, quantity);
                }
                else
                {
                    warehouseItems[item.uniqueID] += quantity;
                }
            }
            else
            {
                // ���ɶѵ���Ʒ��������ʵ��������uniqueID���
                for (int i = 0; i < quantity; i++)
                {
                    // ����Ψһ������Ϊ�����һ���µ� uniqueID
                    Item newItem = CreateUniqueItem(item);
                    warehouseItems.Add(newItem.uniqueID, 1); // ÿ������������Ĭ���� 1
                }
            }

            // ֪ͨ��Ʒ����¼�
            if (!forceNoEvent)
            {
                if (!GameManager.WarehouseSystem.isOpenning)
                {
                    GameManager.NotificationSystem.itemAdded.Invoke(item, quantity);
                }
            }
        }

        private Item CreateUniqueItem(Item baseItem)
        {
            // ���ݻ�����Ʒ����һ�������������˴���Ҫʵ��ʵ���߼����������������û��ǣ�
            Item newItem = ScriptableObject.Instantiate(baseItem);
            newItem.name = $"{baseItem.name}_{System.Guid.NewGuid()}"; // ʹ��Ψһ GUID ��ʶ
            return newItem;
        }

        public bool RemoveFromWarehouse(Item item, int quantity = 1, bool forceNoEvent = false)
        {
            bool success = false;

            if (item.isStackable)
            {
                // �ɶѵ���Ʒ����������
                if (warehouseItems.ContainsKey(item.uniqueID))
                {
                    if (quantity >= warehouseItems[item.uniqueID])
                    {
                        warehouseItems.Remove(item.uniqueID);
                    }
                    else
                    {
                        warehouseItems[item.uniqueID] -= quantity;
                    }

                    success = true;
                }
            }
            else
            {
                // ���ɶѵ���Ʒ����ʵ�����ɾ��
                int removed = 0;
                var itemsToRemove = warehouseItems
                    .Where(kvp => kvp.Key == item.uniqueID)
                    .Take(quantity)
                    .ToList();

                foreach (var kvp in itemsToRemove)
                {
                    warehouseItems.Remove(kvp.Key);
                    removed++;
                }

                success = removed == quantity;
            }

            // ֪ͨ��Ʒ�Ƴ��¼�
            if (!forceNoEvent && success)
            {
                if (!GameManager.WarehouseSystem.isOpenning)
                {
                    GameManager.NotificationSystem.itemRemoved.Invoke(item, quantity);
                }
            }

            return success;

        }

        public void LoadDataBlock(WarehouseDataBlock block)
        {
            m_warehouseMoney = block.money;
            m_warehouseItems = block.items.ToDictionary(
                kvp => kvp.Key.guid,  // ���� StringDatabaseEntryReference �� uniqueID �ֶ�
                kvp => kvp.Value);
        }

        public WarehouseDataBlock CreateDataBlock()
        {
            return new WarehouseDataBlock
            {
                money = m_warehouseMoney,
                items = new SerializableDictionary<DatabaseEntryReference<StringDatabaseEntryReference>, int>(
                    m_warehouseItems.ToDictionary(
                        kvp =>
                        {
                            // ͨ�� guid ��ȡ StringDatabaseEntryReference ʵ��
                            var entry = GameManager.Database.LoadFromReference<StringDatabaseEntryReference>(kvp.Key);
                            return GameManager.Database.CreateReference(entry);  // ʹ�� CreateReference ������ DatabaseEntryReference
                        },
                        kvp => kvp.Value
                    )
                )
            };
        }
    }
}
