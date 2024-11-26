using Gyvr.Mythril2D;
using UnityEditor;
using UnityEngine;
using System.Collections;

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
            if (GUILayout.Button("Add Selected Entities"))
            {
                Debug.Log($"Selected Objects Count: {Selection.objects.Length}");

                foreach (Object selected in Selection.objects)
                {
                    Debug.Log($"Selected Object: {selected}, Type: {selected.GetType()}");

                    // ���Դ�ѡ�еĶ����л�ȡ Entity ���
                    Entity entityToAdd = GetEntityComponent(selected);

                    if (entityToAdd != null)
                    {
                        Debug.Log($"Adding Entity: {entityToAdd.name}");

                        EntityTable.EntityData newEntry = new EntityTable.EntityData
                        {
                            entity = entityToAdd,
                            weight = 1f
                        };
                        ArrayUtility.Add(ref entityTable.entries, newEntry);
                    }
                    else
                    {
                        Debug.Log($"Could not find Entity component on {selected.name}");
                    }
                }
            }

            // �����޸�
            if (GUI.changed)
            {
                EditorUtility.SetDirty(entityTable);
            }
        }

        private Entity GetEntityComponent(Object obj)
        {
            // ����� GameObject�����л�ȡ���
            if (obj is GameObject gameObject)
            {
                // ���Ի�ȡ���м̳��� Entity �����
                Component[] components = gameObject.GetComponents(typeof(Entity));
                if (components.Length > 0)
                {
                    return components[0] as Entity;
                }
            }
            // ����� Component��ֱ��ת��
            else if (obj is Component component)
            {
                return component as Entity;
            }

            return null;
        }

    }
}