using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Gyvr.Mythril2D
{
    public static class DatabaseRegistryExtensions
    {
        public static bool HasGUID(this DatabaseRegistry registry, string guid)
        {
            // ��� GUID �Ƿ����
            return registry.entries.ContainsKey(guid);
        }

        public static bool HasGUIDConversion(this DatabaseRegistry registry, string guid)
        {
            // ��� GUID ת��ӳ���Ƿ����
            return registry.GUIDConversionMap.ContainsKey(guid);
        }

        public static void Set(this DatabaseRegistry registry, DatabaseEntry[] entries)
        {
            // ��ʼ��ʱ��ʹ�� GUID ��Ϊ������Ŀ��Ϊֵ
            registry.Initialize(entries.ToDictionary(entry => entry.GetAssetGUID(), entry => entry));
            registry.ForceSave();
        }

        public static void Register(this DatabaseRegistry registry, DatabaseEntry entry)
        {
            // ʹ����Ŀ�� GUID ע��
            string guid = entry.GetAssetGUID();

            // ��ȡ��Ŀ����������
            string typeName = entry.GetType().FullName;

            // ����Ƿ��Ѵ��ڸ� GUID
            if (!registry.entries.ContainsKey(guid))
            {
                // �� GUID ӳ�䵽��������
                registry.entries[guid] = typeName;
                registry.ForceSave();
            }
        }


        public static void Unregister(this DatabaseRegistry registry, DatabaseEntry entry)
        {
            // ʹ�� GUID ɾ����Ŀ
            string guid = entry.GetAssetGUID();
            registry.RemoveAt(guid);
        }

        public static void RemoveAt(this DatabaseRegistry registry, string guid)
        {
            // ɾ��ָ�� GUID ����Ŀ
            if (registry.HasGUID(guid))
            {
                registry.entries.Remove(guid);
                registry.ForceSave();
            }
        }

        public static void RemoveMissingReferences(this DatabaseRegistry registry)
        {
            List<string> keysToRemove = new List<string>();

            foreach (var entry in registry.entries)
            {
                if (entry.Value == null)
                {
                    keysToRemove.Add(entry.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                registry.entries.Remove(key);
            }

            registry.ForceSave();
        }

        public static void Clear(this DatabaseRegistry registry)
        {
            // ���������Ŀ
            registry.entries.Clear();
            registry.ForceSave();
        }

        public static void RemoveConversion(this DatabaseRegistry registry, string from)
        {
            // �Ƴ� GUID ת��ӳ��
            registry.GUIDConversionMap.Remove(from);
            registry.ForceSave();
        }

        public static void SetConversion(this DatabaseRegistry registry, string from, string to)
        {
            // ���� GUID ת��ӳ��
            registry.GUIDConversionMap[from] = to;
            registry.ForceSave();
        }

        private static void ForceSave(this DatabaseRegistry registry)
        {
            // ǿ�Ʊ������
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssetIfDirty(registry);
        }
    }
}
