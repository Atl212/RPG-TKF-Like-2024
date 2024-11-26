using Gyvr.Mythril2D;
using UnityEditor;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CustomEditor(typeof(LootTable))]
    public class LootTableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LootTable lootTable = (LootTable)target;

            EditorGUILayout.LabelField("Loot Table Configuration", EditorStyles.boldLabel);

            Debug.Log(lootTable.entries.Length);

            // ����Ȩ�طֲ�����ͼ
            if (lootTable.entries != null && lootTable.entries.Length > 0)
            {
                float totalWeight = 0f;
                foreach (var entry in lootTable.entries)
                {
                    totalWeight += entry.weight;
                }

                foreach (var entry in lootTable.entries)
                {
                    float percentage = (totalWeight > 0) ? entry.weight / totalWeight : 0;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(entry.item ? entry.item.name : "Unnamed Item");
                    Rect rect = EditorGUILayout.GetControlRect(false, 20);
                    EditorGUI.ProgressBar(rect, percentage, $"{percentage * 100:F1}%");
                    EditorGUILayout.EndHorizontal();
                }
            }

            // ��ʾ�ͱ༭��Ŀ����
            if (lootTable.entries != null)
            {
                for (int i = 0; i < lootTable.entries.Length; i++)
                {
                    var entry = lootTable.entries[i];
                    EditorGUILayout.BeginVertical("box");
                    entry.item = (Item)EditorGUILayout.ObjectField("Item", entry.item, typeof(Item), false);
                    entry.maxQuantity = EditorGUILayout.IntField("Max Quantity", entry.maxQuantity);
                    entry.weight = EditorGUILayout.FloatField("Weight", entry.weight);

                    // ɾ����ť
                    if (GUILayout.Button($"Remove Entity {i + 1}"))
                    {
                        // ���������Ƴ�
                        ArrayUtility.RemoveAt(ref lootTable.entries, i);
                        break; // ������ǰѭ�������� IndexOutOfRangeException
                    }

                    EditorGUILayout.EndVertical();
                }
            }

            // ����µ���Ŀ��ť
            if (GUILayout.Button("Add New Loot Entry"))
            {
                if (lootTable.entries == null)
                {
                    lootTable.entries = new LootTable.LootEntryData[0];
                }
                ArrayUtility.Add(ref lootTable.entries, new LootTable.LootEntryData());
            }

            // ����µ���Ŀ��ť
            if (GUILayout.Button("Add Selected Items"))
            {
                Debug.Log($"Selected Objects Count: {Selection.objects.Length}");

                foreach (Object selected in Selection.objects)
                {
                    Debug.Log($"Selected Object: {selected}, Type: {selected.GetType()}");

                    // ���Դ�ѡ�еĶ����л�ȡ Entity ���
                    Item entityToAdd = GetItemComponent(selected);

                    if (entityToAdd != null)
                    {
                        Debug.Log($"Adding Item: {entityToAdd.name}");

                        LootTable.LootEntryData newEntry = new LootTable.LootEntryData
                        {
                            item = entityToAdd,
                            maxQuantity = 1,
                            weight = 1f
                        };
                        ArrayUtility.Add(ref lootTable.entries, newEntry);
                    }
                    else
                    {
                        Debug.Log($"Could not find Entity component on {selected.name}");
                    }
                }
            }

            // ��Ǯ����
            lootTable.money = EditorGUILayout.IntField("Max Money", lootTable.money);

            // �����޸�
            if (GUI.changed)
            {
                EditorUtility.SetDirty(lootTable);
            }
        }


        private Item GetItemComponent(Object obj)
        {
            // ����� GameObject�����л�ȡ���
            if (obj is GameObject gameObject)
            {
                // ���û���ҵ� Entity ��������Ի�ȡ Item ���
                Item item = gameObject.GetComponent<Item>();
                if (item != null)
                {
                    return item;
                }
            }
            // ����� Component��ֱ��ת��
            else if (obj is ScriptableObject scriptableObject)
    {
        Item item = scriptableObject as Item;
        if (item != null)
        {
            return item;
        }
    }

            return null;
        }
    }
}
