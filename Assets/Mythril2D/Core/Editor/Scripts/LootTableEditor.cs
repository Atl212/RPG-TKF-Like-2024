using Gyvr.Mythril2D;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LootTable))]
public class LootTableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LootTable lootTable = (LootTable)target;

        EditorGUILayout.LabelField("Loot Table Configuration", EditorStyles.boldLabel);

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
            foreach (var entry in lootTable.entries)
            {
                EditorGUILayout.BeginVertical("box");
                entry.item = (Item)EditorGUILayout.ObjectField("Item", entry.item, typeof(Item), false);
                entry.maxQuantity = EditorGUILayout.IntField("Max Quantity", entry.maxQuantity);
                entry.weight = EditorGUILayout.FloatField("Weight", entry.weight);
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

        // ��Ǯ����
        lootTable.money = EditorGUILayout.IntField("Max Money", lootTable.money);

        // �����޸�
        if (GUI.changed)
        {
            EditorUtility.SetDirty(lootTable);
        }
    }
}
