using System.Collections;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class DeadBody : OtherEntity
    {
        [Header("Dead Body Settings")]
        [SerializeField] private ChestLoot m_loot;
        [SerializeField] private string m_gameFlagID = "DeadBody_00";
        [SerializeField] private Sprite[] m_deadBodySprites = null;
        [SerializeField] private int m_canLootCount = 3;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_openedSound;

        private bool m_opened = false;
        private int m_nowLootedCount = 0;

        protected override void Start()
        {
            base.Start();
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

        public bool TryLooted()
        {
            if (m_nowLootedCount < m_canLootCount) // ����Ƿ���Լ����Ӷ�
            {
                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_openedSound);

                // ��������Ӷ���Ʒ���ǽ�Ǯ
                bool lootItem = Random.Range(0, 2) == 0;

                if (lootItem && m_loot.entries != null && m_loot.entries.Length > 0)
                {
                    // ���ѡ��һ����Ʒ
                    var randomEntry = m_loot.entries[Random.Range(0, m_loot.entries.Length)];

                    // ���������Ŀ����������Χ�ɵ�����
                    int randomQuantity = Random.Range(1, randomEntry.quantity + 1);

                    GameManager.InventorySystem.AddToBag(randomEntry.item, randomQuantity);
                }
                else if (m_loot.money > 0)
                {
                    // ��������Ǯ��������Χ�ɵ�����
                    int randomMoney = Random.Range(10, m_loot.money + 1);
                    GameManager.InventorySystem.AddMoney(randomMoney);
                }

                // �����Ӷ����
                m_nowLootedCount++;

                // ����Ƿ��Ѵﵽ����Ӷ����
                if (m_nowLootedCount >= m_canLootCount)
                {
                    this.gameObject.layer = LayerMask.NameToLayer("Default"); // ����Ϊ���ɱ��Ӷ�
                }

                return true; // ��ʾ�����Ӷ�ɹ�
            }

            return false; // ��ʾ�Ѵﵽ����Ӷ�������޷����Ӷ�
        }
    }
}
