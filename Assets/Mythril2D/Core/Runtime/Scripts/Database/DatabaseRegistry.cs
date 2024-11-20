using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D + nameof(DatabaseRegistry))]
    public class DatabaseRegistry : ScriptableObject
    {
        public bool autoAddNewDatabaseEntries => m_autoAddNewDatabaseEntries;
        public bool autoRemoveDatabaseEntries => m_autoRemoveDatabaseEntries;
        //public SerializableDictionary<string, DatabaseEntry> entries => m_entries;
        public SerializableDictionary<string, string> entries => m_entries;  // GUID ������ӳ��
        public SerializableDictionary<string, string> GUIDConversionMap => m_GUIDConversionMap;  // GUID ת��ӳ��

        [Header("Automation Settings")]
        [SerializeField] private bool m_autoAddNewDatabaseEntries = true;
        [SerializeField] private bool m_autoRemoveDatabaseEntries = true;

        [Header("Database Content")]
        [SerializeField] private SerializableDictionary<string, string> m_entries = null; // �洢 GUID ����Ʒ���͵�ӳ��
        [SerializeField] private SerializableDictionary<string, string> m_GUIDConversionMap = null; // �洢 GUID ת��ӳ��

        // ʹ�� GUID ������Ʒʵ��
        public Item LoadItemByGUID(string guid)
        {
            if (m_entries.ContainsKey(guid))
            {
                // ��ȡ��Ӧ GUID ����Ʒ����
                string itemType = m_entries[guid];

                // ���� itemType ��������Ʒ�� ScriptableObject ����ʵ��
                return LoadItemOfType(itemType);
            }

            return null;
        }

        // ������Ʒʵ���ĸ�������
        private Item LoadItemOfType(string itemType)
        {
            // ���� itemType ���������ض�Ӧ���͵���Ʒʵ��
            // �������ͨ���������������ʽ���ض�Ӧ���͵� Item ����
            // �������ﷵ�ص���ͨ����Դ���ص� Item ʵ��
            return Resources.Load<Item>(itemType);
        }

        private T LoadItemOfType<T>(string itemType) where T : DatabaseEntry
        {
            // ���������ַ���������Ʒʵ��
            return Resources.Load<T>(itemType);  // ������Ը���ʵ����Ŀ��Ҫ�滻���ط���
        }


        // �������Ʒ�����ݿ�
        public void AddItem(string guid, string itemType)
        {
            if (!m_entries.ContainsKey(guid))
            {
                m_entries.Add(guid, itemType);
            }
        }

        // �Ƴ����ݿ��е���Ʒ
        public void RemoveItem(string guid)
        {
            if (m_entries.ContainsKey(guid))
            {
                m_entries.Remove(guid);
            }
        }
        public DatabaseEntryReference<T> CreateReference<T>(T entry) where T : DatabaseEntry
        {
            // ���� GUID ����ʱ�����Ǹ�����Ŀ��ȡ GUID�������� DatabaseEntryReference
            string guid = DatabaseEntryToGUID(entry);
            return new DatabaseEntryReference<T>(guid);
        }

        public T LoadFromReference<T>(DatabaseEntryReference<T> reference) where T : DatabaseEntry
        {
            // �������õ� GUID ������Ӧ����Ʒ�������ݿ���Ŀ��
            return GUIDToDatabaseEntry<T>(reference.guid);
        }

        public T GUIDToDatabaseEntry<T>(string guid) where T : DatabaseEntry
        {
            HashSet<string> visited = new HashSet<string>();

            // ���⻷�����ã�ת�� GUID �������
            while (m_GUIDConversionMap.ContainsKey(guid))
            {
                guid = m_GUIDConversionMap[guid];
                if (visited.Contains(guid))
                {
                    Debug.LogError($"Circular reference detected in DatabaseRegistry: {guid}");
                    return null;
                }
                visited.Add(guid);
            }

            // ͨ�� GUID ������Ʒ�����ݿ���Ŀ
            if (m_entries.ContainsKey(guid))
            {
                // ���� GUID ��ȡ��Ŀ���Ͳ�����ʵ��
                string itemType = m_entries[guid];
                return LoadItemOfType<T>(itemType);  // ���ض�Ӧ���͵���Ŀ
            }

            return null;
        }

        public string DatabaseEntryToGUID<T>(T instance) where T : DatabaseEntry
        {
            // ��ȡʵ��������������Ϊ�ַ���
            string instanceTypeName = instance.GetType().FullName;

            // ����ʵ���������Ʋ��Ҷ�Ӧ�� GUID
            string guid = m_entries.FirstOrDefault(entry => entry.Value == instanceTypeName).Key;

            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError($"Database entry {instance} does not exist in the registry.");
            }

            return guid;
        }


        public void Initialize(Dictionary<string, DatabaseEntry> data)
        {
            // ͨ�� GUID ӳ���ʼ�����ݿ���Ŀ
            m_entries = new SerializableDictionary<string, string>();

            foreach (var entry in data)
            {
                string guid = entry.Key;  // GUID Ϊ�ֵ�ļ�
                string itemType = entry.Value.GetType().FullName;  // ��ȡ��Ŀ�������ַ���
                m_entries[guid] = itemType;  // ���� GUID -> ���� ӳ��
            }
        }
    }
}
