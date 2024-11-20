using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIInventoryEquipmentSlot : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private EEquipmentType m_equipmentType = EEquipmentType.Head;
        [SerializeField] private Image m_placeholder = null;
        [SerializeField] private Image m_content = null;
        [SerializeField] private Button m_button = null;

        public EEquipmentType equipmentType => m_equipmentType;

        private string m_equipmentGUID = null; // ʹ�� GUID ���� Equipment ʵ��
        //private Equipment m_equipment = null;
        private bool m_selected = false;

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_button.Select();
        }

        public void OnSelect(BaseEventData eventData)
        {
            m_selected = true;
            // ��� m_equipmentGUID ��Ϊ�գ�ͨ�� GUID ��ȡ Equipment ʵ��
            if (!string.IsNullOrEmpty(m_equipmentGUID))
            {
                Equipment equipment = GameManager.Database.LoadItemByGUID(m_equipmentGUID) as Equipment;

                // ȷ��װ������������ȷ������
                if (equipment != null && equipment.type == m_equipmentType)
                {
                    string itemGUID = GameManager.Database.DatabaseEntryToGUID(equipment); // ��ȡ��Ӧ�� GUID
                                                                                           // ���� itemDetailsOpened �¼������� GUID
                    GameManager.NotificationSystem.itemDetailsOpened.Invoke(itemGUID);
                }
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            m_selected = false;
            GameManager.NotificationSystem.itemDetailsClosed.Invoke();
        }

        public void SetEquipment(Equipment equipment)
        {
            if (equipment != null)
            {
                m_equipmentGUID = GameManager.Database.DatabaseEntryToGUID(equipment); // �洢��Ʒ�� GUID

                Debug.Assert(equipment.type == m_equipmentType, "Equipment type mismatch");

                m_placeholder.enabled = false;
                m_content.enabled = true;
                m_content.sprite = equipment.icon;
            }
            else
            {
                m_equipmentGUID = null; // ��� GUID

                m_placeholder.enabled = true;
                m_content.enabled = false;
                m_content.sprite = null;
            }

            if (m_selected && !string.IsNullOrEmpty(m_equipmentGUID))
            {
                Equipment newEquipment = GameManager.Database.LoadItemByGUID(m_equipmentGUID) as Equipment;
                if (newEquipment != null)
                {
                    GameManager.NotificationSystem.itemDetailsOpened.Invoke(m_equipmentGUID); // ���� GUID
                }
            }
        }

        private void Awake()
        {
            m_button.onClick.AddListener(OnSlotClicked);
        }

        private void OnSlotClicked()
        {
            if (!string.IsNullOrEmpty(m_equipmentGUID))
            {
                // ͨ�� GUID ��ȡ Equipment ʵ��
                Equipment equipment = GameManager.Database.LoadItemByGUID(m_equipmentGUID) as Equipment;
                if (equipment != null)
                {
                    SendMessageUpwards("OnEquipmentItemClicked", equipment, SendMessageOptions.RequireReceiver);
                }
            }
        }
    }
}
