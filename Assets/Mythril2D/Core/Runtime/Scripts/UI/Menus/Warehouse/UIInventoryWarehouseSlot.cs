using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIInventoryWarehouseSlot : MonoBehaviour, IItemSlotHandler, IPointerEnterHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Image m_image = null;
        [SerializeField] private TextMeshProUGUI m_quantity = null;
        [SerializeField] private Button m_button = null;

        public Button button => m_button;

        private Item m_item = null;               // ������Ʒ��Ϣ
        private ItemInstance m_itemInstance = null; // ������Ʒʵ����Ϣ
        private bool m_selected = false;
        private bool isPointerDown = false;
        private float pointerDownTimer = 0f;
        private const float longPressThreshold = 2f; // ����ʱ����ֵ

        private void Update()
        {
            if (isPointerDown)
            {
                pointerDownTimer += Time.deltaTime;
                if (pointerDownTimer >= longPressThreshold)
                {
                    OnLongPress();
                    ResetLongPress();
                }
            }
        }

        private void Start()
        {
            GameManager.InputSystem.ui.submit.started += OnSubmitDown;
            GameManager.InputSystem.ui.submit.canceled += OnSubmitUp;
        }

        private void OnDestroy()
        {
            GameManager.InputSystem.ui.submit.started -= OnSubmitDown;
            GameManager.InputSystem.ui.submit.canceled -= OnSubmitUp;
        }

        public void OnSubmitDown(InputAction.CallbackContext context)
        {
            isPointerDown = true;
            pointerDownTimer = 0f;
        }

        public void OnSubmitUp(InputAction.CallbackContext context)
        {
            ResetLongPress();
        }

        private void OnLongPress()
        {
            if (m_item != null)
            {
                Debug.Log("Long press detected, item discarded.");
                SendMessageUpwards("OnItemDiscarded", m_itemInstance, SendMessageOptions.RequireReceiver);
                Clear(); // �����Ʒ��
            }
        }

        private void ResetLongPress()
        {
            isPointerDown = false;
            pointerDownTimer = 0f;
        }

        public void Clear() => SetItem(null, 0);

        public Item GetItem()
        {
            return m_item;
        }

        public ItemInstance GetItemInstance()
        {
            return m_itemInstance;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_button.Select();
        }

        public void OnSelect(BaseEventData eventData)
        {
            m_selected = true;
            GameManager.NotificationSystem.itemDetailsOpened.Invoke(m_item);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            m_selected = false;
            GameManager.NotificationSystem.itemDetailsClosed.Invoke();
        }

        public void SetItem(ItemInstance itemInstance)
        {
            if (itemInstance != null)
            {
                SetItem(itemInstance.GetItem(), itemInstance.quantity);
                m_itemInstance = itemInstance;
            }
            else
            {
                Clear();
            }
        }

        public void SetItem(Item item, int quantity)
        {
            if (item != null)
            {
                m_item = item;

                // ��� m_itemInstance Ϊ�գ����½�һ��ʵ��
                if (m_itemInstance == null)
                {
                    m_itemInstance = new ItemInstance(item, quantity);
                }
                else
                {
                    // �� m_itemInstance ��ֵ
                    m_itemInstance.itemReference = GameManager.Database.CreateReference(item);
                    m_itemInstance.quantity = quantity;
                }

                // ����Ƕѵ���Ʒ����ʾ����������ֻ��ʾ��Ʒͼ��
                m_quantity.text = item.IsStackable ? quantity.ToString() : string.Empty;

                m_image.enabled = true;
                m_image.sprite = item.Icon;
            }
            else
            {
                // ���û����Ʒ����ղ�λ
                m_image.enabled = false;
                m_quantity.text = string.Empty;
                m_item = null;
            }

            // �����λ��ѡ�У���ʾ��Ʒ����
            if (m_selected)
            {
                GameManager.NotificationSystem.itemDetailsOpened.Invoke(m_item);
            }

        }

        private void Awake()
        {
            m_button.onClick.AddListener(OnSlotClicked);
        }

        private void OnSlotClicked()
        {
            if (m_item != null)
            {
                SendMessageUpwards("OnWarehouseItemClicked", m_item, SendMessageOptions.RequireReceiver);
            }
        }
    }
}
