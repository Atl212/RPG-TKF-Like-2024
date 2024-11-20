using UnityEngine;

namespace Gyvr.Mythril2D
{
    // ����һ��ר�ŵ����ͼ̳��� DatabaseEntry
    public class StringDatabaseEntryReference : DatabaseEntry
    {
        // ��������������ض��߼������總����Ϣ��������ǰֻ��Ҫ�ַ���
        public string guid;

        public StringDatabaseEntryReference(string guid)
        {
            this.guid = guid;
        }
    }
}
