using System.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace Gyvr.Mythril2D
{
    public class DeadBody : OtherEntity
    {
        [Header("Dead Body Settings")]
        [SerializeField] private Loot m_loot;
        [SerializeField] private string m_gameFlagID = "DeadBody_00";
        [SerializeField] private Sprite[] m_deadBodySprites = null;
        [SerializeField] private int m_canLootCount = 5;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_openedSound;
        public LootTable lootTable;   // ���� ScriptableObject ���ݱ�

        private bool m_opened = false;
        private int m_randomMaxLootedCount = 0;
        private int m_nowLootedCount = 0;

        protected override void Start()
        {
            base.Start();

            m_randomMaxLootedCount = Random.Range(0, m_canLootCount);

            AssignRandomSprite();
        }

        private void AssignRandomSprite()
        {
            // ����Ҫ�������Ƿ���Ч
            if (m_spriteRenderer == null || m_spriteRenderer.Length == 0 || m_deadBodySprites == null || m_deadBodySprites.Length == 0)
            {
                Debug.LogWarning("SpriteRenderer�����DeadBodySprites����δ��ȷ���ã�");
                return;
            }

            // ��m_deadBodySprites�����ѡ��һ������
            Sprite randomSprite = m_deadBodySprites[Random.Range(0, m_deadBodySprites.Length)];

            // ��������Ƿ�תx��
            bool flipX = Random.Range(0, 2) == 0; // ����0��1�������Ƿ�ת

            // Ӧ�õ�m_spriteRenderer[0]
            m_spriteRenderer[0].sprite = randomSprite;
            m_spriteRenderer[0].flipX = flipX;
        }

        public override void OnStartInteract(CharacterBase sender, Entity target)
        {
            if (target != this)
            {
                return;
            }

            GameManager.Player.OnTryStartLoot(target, m_lootedTime);
        }

        private LootTable.LootEntryData GetRandomLootEntry()
        {
            float totalWeight = 0f;

            foreach (var entry in lootTable.entries)
            {
                totalWeight += entry.weight;
            }

            float randomValue = Random.Range(0f, totalWeight);

            foreach (var entry in lootTable.entries)
            {
                if (randomValue < entry.weight)
                {
                    return entry;
                }

                randomValue -= entry.weight;
            }

            return null;
        }


        public bool TryLooted()
        {
            if (m_nowLootedCount < m_randomMaxLootedCount) // ����Ƿ���Լ����Ӷ�
            {
                // ���Ŵ�����
                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_openedSound);

                // ��������Ӷ���Ʒ���ǽ�Ǯ
                bool lootItem = Random.Range(0, 2) == 0;

                if (lootItem && lootTable.entries != null && lootTable.entries.Length > 0)
                {
                    // ʹ�û���Ȩ�ص����ѡ�����
                    var randomEntry = GetRandomLootEntry();

                    if (randomEntry != null)
                    {
                        int randomQuantity = Random.Range(1, randomEntry.maxQuantity + 1);
                        GameManager.InventorySystem.AddToBag(randomEntry.item, randomQuantity);
                        Debug.Log($"��һ���� {randomQuantity} �� {randomEntry.item.name}");
                    }

                }
                else if (lootTable.money > 0)
                {
                    int randomMoney = Random.Range(10, lootTable.money + 1);
                    GameManager.InventorySystem.AddMoney(randomMoney);
                    Debug.Log($"��һ���� {randomMoney} ���");
                }

                // �����Ӷ����
                m_nowLootedCount++;

                // ����Ƿ��Ѵﵽ����Ӷ����
                if (m_nowLootedCount >= m_randomMaxLootedCount)
                {
                    this.gameObject.layer = LayerMask.NameToLayer("Default"); // ����Ϊ���ɱ��Ӷ�
                }

                return true; // ��ʾ�����Ӷ�ɹ�
            }

            return false; // ��ʾ�Ѵﵽ����Ӷ�������޷����Ӷ�
        }
    }
}
