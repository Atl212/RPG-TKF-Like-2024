using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIInventoryWarehouseSlot : MonoBehaviour, IItemSlotHandler, IPointerEnterHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Image m_image = null;
        [SerializeField] private TextMeshProUGUI m_quantity = null;
        [SerializeField] private Button m_button = null;

        public Button button => m_button;

        private string m_itemGUID = null; // ʹ�� GUID ����ֱ�ӵ� Item ʵ��
        //private Item m_item = null;
        private bool m_selected = false;

        public void Clear() => SetItem(null, 0);

        public Item GetItem()
        {
            return string.IsNullOrEmpty(m_itemGUID) ? null : GameManager.Database.LoadItemByGUID(m_itemGUID); // ͨ�� GUID ��ȡ Item
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_button.Select();
        }

        public void OnSelect(BaseEventData eventData)
        {
            m_selected = true;

            // ͨ�� GUID ��ȡ Item����������ϸ��Ϣ��ʾ
            if (!string.IsNullOrEmpty(m_itemGUID))
            {
                // ���� GUID �� itemDetailsOpened �¼�
                GameManager.NotificationSystem.itemDetailsOpened.Invoke(m_itemGUID); // ���� GUID ������ Item
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            m_selected = false;
            GameManager.NotificationSystem.itemDetailsClosed.Invoke();
        }

        public void SetItem(Item item, int quantity)
        {
            if (item != null)
            {
                m_itemGUID = GameManager.Database.DatabaseEntryToGUID(item); // �洢 GUID ���� Item ʵ��
                m_quantity.text = quantity.ToString();
                m_image.enabled = true;
                m_image.sprite = item.icon;
            }
            else
            {
                m_image.enabled = false;
                m_quantity.text = string.Empty;
                m_itemGUID = null; // ��� GUID
            }

            if (m_selected && !string.IsNullOrEmpty(m_itemGUID))
            {
                // ���� GUID �� itemDetailsOpened �¼�
                GameManager.NotificationSystem.itemDetailsOpened.Invoke(m_itemGUID); // ���� GUID ������ Item
            }
        }

        private void Awake()
        {
            m_button.onClick.AddListener(OnSlotClicked);
        }

        private void OnSlotClicked()
        {
            if (!string.IsNullOrEmpty(m_itemGUID))
            {
                // ͨ�� GUID ��ȡ��Ʒʵ��
                Item item = GameManager.Database.LoadItemByGUID(m_itemGUID);
                if (item != null)
                {
                    // ������Ʒ����¼���������Ʒʵ��
                    SendMessageUpwards("OnWarehouseItemClicked", item, SendMessageOptions.RequireReceiver);
                }
            }
        }
    }
}
