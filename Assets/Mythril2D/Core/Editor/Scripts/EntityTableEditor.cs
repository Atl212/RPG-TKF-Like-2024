using Gyvr.Mythril2D;
using UnityEditor;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CustomEditor(typeof(EntityTable))]
    public class EntityTableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EntityTable entityTable = (EntityTable)target;

            EditorGUILayout.LabelField("Entity Table Configuration", EditorStyles.boldLabel);

            // ����Ȩ�طֲ�����ͼ
            if (entityTable.entries != null && entityTable.entries.Length > 0)
            {
                float totalWeight = 0f;
                foreach (var entry in entityTable.entries)
                {
                    totalWeight += entry.weight;
                }

                foreach (var entry in entityTable.entries)
                {
                    float percentage = (totalWeight > 0) ? entry.weight / totalWeight : 0;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(entry.entity ? entry.entity.name : "Unnamed Entity");
                    Rect rect = EditorGUILayout.GetControlRect(false, 20);
                    EditorGUI.ProgressBar(rect, percentage, $"{percentage * 100:F1}%");
                    EditorGUILayout.EndHorizontal();
                }
            }

            // ��ʾ�ͱ༭��Ŀ����
            if (entityTable.entries != null)
            {
                for (int i = 0; i < entityTable.entries.Length; i++)
                {
                    var entry = entityTable.entries[i];
                    EditorGUILayout.BeginVertical("box");
                    entry.entity = (Entity)EditorGUILayout.ObjectField("Entity", entry.entity, typeof(Entity), false);
                    entry.weight = EditorGUILayout.FloatField("Weight", entry.weight);

                    // ɾ����ť
                    if (GUILayout.Button($"Remove Entity {i + 1}"))
                    {
                        // ���������Ƴ�
                        ArrayUtility.RemoveAt(ref entityTable.entries, i);
                        break; // ������ǰѭ�������� IndexOutOfRangeException
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            // ����µ���Ŀ��ť
            if (GUILayout.Button("Add New Entity"))
            {
                if (entityTable.entries == null)
                {
                    entityTable.entries = new EntityTable.EntityData[0];
                }
                ArrayUtility.Add(ref entityTable.entries, new EntityTable.EntityData());
            }

            // �����޸�
            if (GUI.changed)
            {
                EditorUtility.SetDirty(entityTable);
            }
        }
    }
}