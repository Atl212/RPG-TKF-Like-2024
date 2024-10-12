using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gyvr.Mythril2D
{
    public class Hero : Character<HeroSheet>
    {
        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_levelUpSound;

        [Header("Hero")]
        [SerializeField] private bool m_restoreHealthOnLevelUp = true;
        [SerializeField] private bool m_restoreManaOnLevelUp = true;

        public int experience => m_experience;
        public int nextLevelExperience => GetTotalExpRequirement(m_level + 1);
        public int availablePoints => m_sheet.pointsPerLevel * (m_level - Stats.MinLevel) - m_usedPoints;
        public SerializableDictionary<EEquipmentType, Equipment> equipments => m_equipments;
        public AbilitySheet[] equippedAbilities => m_equippedAbilities;
        public DashAbilitySheet dashAbility => m_dashAbility;
        public HashSet<AbilitySheet> bonusAbilities => m_bonusAbilities;
        public UnityEvent<AbilitySheet[]> equippedAbilitiesChanged => m_equippedAbilitiesChanged;
        public Stats customStats => m_customStats;
        public Stats missingCurrentStats => m_missingCurrentStats;
        public int usedPoints => m_usedPoints;

        public const int MaxEquipedAbilityCount = 4;

        private Stats m_customStats = new Stats();
        private Stats m_missingCurrentStats = new Stats();
        private int m_usedPoints = 0;
        private int m_experience = 0;
        private SerializableDictionary<EEquipmentType, Equipment> m_equipments = new SerializableDictionary<EEquipmentType, Equipment>();
        private HashSet<AbilitySheet> m_bonusAbilities = new HashSet<AbilitySheet>();

        private AbilitySheet[] m_equippedAbilities = new AbilitySheet[MaxEquipedAbilityCount];

        // 这里用的话，得在人物sheet下面导入对应的 abilitysheet
        //ScriptableObject.ctor is not allowed to be called from a MonoBehaviour constructor (or instance field initializer), call it in Awake or Start instead. 
        //private DashAbilitySheet m_dashAbility = ScriptableObject.CreateInstance<DashAbilitySheet>();
        private DashAbilitySheet m_dashAbility = null;

        private UnityEvent<AbilitySheet[]> m_equippedAbilitiesChanged = new UnityEvent<AbilitySheet[]>();

        public int GetTotalExpRequirement(int level)
        {
            int total = 0;

            for (int i = 1; i < level; i++)
            {
                total += m_sheet.experience[i];
            }

            return total;
        }

        public void AddExperience(int experience, bool silentMode = false)
        {
            Debug.Assert(experience > 0, "Cannot add a negative amount of experience.");

            GameManager.NotificationSystem.experienceGained.Invoke(experience);

            m_experience += experience;

            while (m_experience >= GetTotalExpRequirement(m_level + 1))
            {
                OnLevelUp(silentMode);
            }
        }

        public void AddCustomStats(Stats customStats)
        {
            m_customStats += customStats;
            UpdateStats();
        }

        public void LogUsedPoints(int points)
        {
            m_usedPoints += points;
        }

        private void Start()
        {
            UpdateStats();
        }

        private void ApplyMissingCurrentStats()
        {
            m_currentStats.Set(m_currentStats.stats - m_missingCurrentStats);
            m_missingCurrentStats.Reset();
        }

        public Equipment Equip(Equipment equipment)
        {
            Equipment previousEquipment = Unequip(equipment.type);
            m_equipments[equipment.type] = equipment;
            GameManager.NotificationSystem.itemEquipped.Invoke(equipment);
            UpdateStats();
            return previousEquipment;
        }

        public Equipment Unequip(EEquipmentType type)
        {
            m_equipments.TryGetValue(type, out Equipment toUnequip);

            if (toUnequip)
            {
                m_equipments.Remove(type);
                GameManager.NotificationSystem.itemUnequipped.Invoke(toUnequip);
                UpdateStats();
            }

            return toUnequip;
        }

        public void AddBonusAbility(AbilitySheet abilitySheet)
        {
            if (!m_bonusAbilities.Contains(abilitySheet))
            {
                m_bonusAbilities.Add(abilitySheet);
                AddAbility(abilitySheet);
            }
        }

        public void RemoveBonusAbility(AbilitySheet abilitySheet)
        {
            if (!m_bonusAbilities.Contains(abilitySheet))
            {
                Debug.LogAssertion("Cannot remove an ability that hasn't been added in the first place.");
            }
            else
            {
                m_bonusAbilities.Remove(abilitySheet);
                RemoveAbility(abilitySheet);
            }
        }

        private bool AddDashAbility(DashAbilitySheet abilitySheet)
        {
            if (base.AddAbility(abilitySheet))
            {
                if (!IsAbilityEquiped(abilitySheet))
                {
                    for (int i = 0; i < m_equippedAbilities.Length; ++i)
                    {
                        m_dashAbility = abilitySheet;
                    }
                }

                GameManager.NotificationSystem.abilityAdded.Invoke(abilitySheet);
                return true;
            }

            return false;
        }

        protected override bool AddAbility(AbilitySheet abilitySheet)
        {
            if (base.AddAbility(abilitySheet))
            {
                if (!IsAbilityEquiped(abilitySheet))
                {
                    for (int i = 0; i < m_equippedAbilities.Length; ++i)
                    {
                        if (m_equippedAbilities[i] == null)
                        {
                            Equip(abilitySheet, i);
                        }
                    }
                }

                GameManager.NotificationSystem.abilityAdded.Invoke(abilitySheet);
                return true;
            }

            return false;
        }

        protected override bool RemoveAbility(AbilitySheet abilitySheet)
        {
            if (base.RemoveAbility(abilitySheet))
            {
                for (int i = 0; i < m_equippedAbilities.Length; ++i)
                {
                    if (m_equippedAbilities[i] == abilitySheet)
                    {
                        Unequip(i);
                    }
                }

                GameManager.NotificationSystem.abilityRemoved.Invoke(abilitySheet);
                return true;
            }

            return false;
        }

        public void Equip(AbilitySheet ability, int index)
        {
            m_equippedAbilities[index] = ability;
            m_equippedAbilitiesChanged.Invoke(m_equippedAbilities);
        }

        public void Unequip(int index)
        {
            m_equippedAbilities[index] = null;
            m_equippedAbilitiesChanged.Invoke(m_equippedAbilities);
        }

        public bool IsAbilityEquiped(AbilitySheet abilitySheet)
        {
            foreach (AbilitySheet ability in m_equippedAbilities)
            {
                if (ability == abilitySheet) return true;
            }

            return false;
        }

        private Stats CalculateEquipmentStats()
        {
            Stats equipmentStats = new Stats();

            foreach (Equipment piece in m_equipments.Values)
            {
                if (piece)
                {
                    equipmentStats += piece.bonusStats;
                }
            }

            return equipmentStats;
        }

        private void UpdateStats()
        {
            Stats equipmentStats = CalculateEquipmentStats();
            Stats totalStats = m_sheet.baseStats + m_customStats + equipmentStats;

            m_stats.Set(totalStats);

            ApplyMissingCurrentStats();
        }

        private void OnLevelUp(bool silentMode = false)
        {
            ++m_level;

            if (!silentMode)
            {
                if (m_restoreHealthOnLevelUp)
                {
                    Heal(m_stats[EStat.Health] - m_currentStats[EStat.Health]);
                }

                if (m_restoreManaOnLevelUp)
                {
                    RecoverMana(m_stats[EStat.Mana] - m_currentStats[EStat.Mana]);
                }

                GameManager.NotificationSystem.levelUp.Invoke(m_level);
                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_levelUpSound);
            }

            foreach (AbilitySheet ability in m_sheet.GetAbilitiesUnlockedAtLevel(m_level))
            {
                AddAbility(ability);
            }
        }

        public void Initialize(PlayerDataBlock block)
        {
            m_usedPoints = block.usedPoints;

            if (block.experience > 0)
            {
                AddExperience(block.experience, true);
            }

            AddDashAbility(m_sheet.GetDashAbility());

            foreach (var ability in block.bonusAbilities)
            {
                AddBonusAbility(GameManager.Database.LoadFromReference(ability));
            }

            m_equipments = new SerializableDictionary<EEquipmentType, Equipment>();

            foreach (var piece in block.equipments)
            {
                Equipment instance = GameManager.Database.LoadFromReference(piece);
                m_equipments[instance.type] = instance;
            }

            m_customStats = block.customStats;

            // Copy missing current stats so block data doesn't get altered
            m_missingCurrentStats = new Stats(block.missingCurrentStats);

            // Clear equipped abilities
            for (int i = 0; i < m_equippedAbilities.Length; ++i)
            {
                m_equippedAbilities[i] = i < block.equippedAbilities.Length ? GameManager.Database.LoadFromReference(block.equippedAbilities[i]) : null;
            }

            m_equippedAbilitiesChanged.Invoke(m_equippedAbilities);

            transform.position = block.position;
        }

        protected override void OnDeath()
        {
            // Prevents the Hero GameObject from being destroyed, so it can be used in the death screen.
            m_destroyOnDeath = false; 
            base.OnDeath();
            GameManager.NotificationSystem.deathScreenRequested.Invoke();
        }
    }
}
