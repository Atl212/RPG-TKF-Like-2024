using TMPro;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class UIInventory : MonoBehaviour, IUIMenu
    {
        [SerializeField] private UIInventoryEquipment m_equipment = null;
        [SerializeField] private UIInventoryBag m_bag = null;
        [SerializeField] private UIInventoryStats m_stats = null;
        [SerializeField] private TextMeshProUGUI m_backpackMoney = null;

        public UIInventoryBag bag => m_bag;

        public void Init()
        {
            m_bag.UpdateSlots();
        }

        public void Show(params object[] args)
        {
            UpdateUI();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void EnableInteractions(bool enable)
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup)
            {
                canvasGroup.interactable = enable;
            }
        }

        public GameObject FindSomethingToSelect()
        {
            UINavigationCursorTarget bagNavigationTarget = m_bag.FindNavigationTarget();

            if (bagNavigationTarget && bagNavigationTarget.gameObject.activeInHierarchy)
            {
                return bagNavigationTarget.gameObject;
            }
            else
            {
                UINavigationCursorTarget equipmentNavigationTarget = m_equipment.FindNavigationTarget();

                if (equipmentNavigationTarget && equipmentNavigationTarget.isActiveAndEnabled)
                {
                    return equipmentNavigationTarget.gameObject;
                }
            }

            return null;
        }

        // Message called by children using SendMessageUpward when the bag or equipment changed
        private void UpdateUI()
        {
            m_bag.UpdateSlots();
            m_equipment.UpdateSlots();
            m_stats.UpdateUI();
            m_backpackMoney.text = StringFormatter.Format("{0}", GameManager.InventorySystem.backpackMoney.ToString());
        }

        private void OnItemClicked(Item item, EItemLocation location)
        {
            item.Use(GameManager.Player, location);
            UpdateUI();
        }

        private void OnBagItemClicked(Item item) => OnItemClicked(item, EItemLocation.Bag);
        private void OnEquipmentItemClicked(Item item) => OnItemClicked(item, EItemLocation.Equipment);
    }
}
