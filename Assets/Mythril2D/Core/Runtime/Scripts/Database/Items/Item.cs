using UnityEngine;

namespace Gyvr.Mythril2D
{
    public enum EItemLocation
    {
        Bag,
        Warehouse,
        Equipment
    }

    public enum EItemCategory
    {
        Consumable,
        Resource,
        Gear,
        Key
    }

    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Items + nameof(Item))]
    public class Item : DatabaseEntry, INameable
    {
        [Header("General")]
        [SerializeField] private string m_uniqueID = string.Empty; // 唯一标识符
        [SerializeField] private EItemCategory m_category = 0;
        [SerializeField] private Sprite m_icon = null;
        [SerializeField] private string m_displayName = string.Empty;
        [SerializeField] private string m_description = string.Empty;
        [SerializeField] private int m_price = 50;
        [SerializeField] private bool m_isStackable = false; // 默认设置为不可堆叠


        public virtual void Use(CharacterBase target, EItemLocation location)
        {
            GameManager.DialogueSystem.Main.PlayNow("This item has no effect");
        }

        public string uniqueID => m_uniqueID;
        public EItemCategory category => m_category;
        public Sprite icon => m_icon;
        public string displayName => DisplayNameUtils.GetNameOrDefault(this, m_displayName);
        public string description => StringFormatter.Format(m_description);
        public int price => m_price;
        public bool isStackable => m_isStackable; // 改为通过公共访问器暴露

        // 检测并生成唯一标识符
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(m_uniqueID))
            {
                m_uniqueID = System.Guid.NewGuid().ToString(); // 生成 GUID
                Debug.Log($"Generated unique ID for item '{m_displayName}': {m_uniqueID}");
            }
        }
    }
}
