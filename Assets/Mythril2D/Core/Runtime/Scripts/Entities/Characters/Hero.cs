using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Events;
using System;
using FunkyCode;

namespace Gyvr.Mythril2D
{
    public class Hero : Character<HeroSheet>
    {
        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_levelUpSound;

        [Header("Hero")]
        [SerializeField] private bool m_restoreHealthOnLevelUp = true;
        [SerializeField] private bool m_restoreManaOnLevelUp = true;

        [Header("Particle")]
        [SerializeField] private ParticleSystem m_dashParticleSystem = null;
        [SerializeField] private ParticleSystem m_runParticleSystem = null;
        public ParticleSystem dashParticleSystem => m_dashParticleSystem;
        public ParticleSystem runParticleSystem => m_runParticleSystem;

        [Header("Stamina Paramenters")]
        // ÿ�����Ķ�������
        [SerializeField] private float staminaMultiplier = 7;
        // �����ظ�ǰ���ȴ�ʱ��
        [SerializeField] private float timeBeforeStaminaRengeStarts = 1.5f;
        // ÿ�ε�λ�����ظ���
        [SerializeField] private float staminaValueIncrement = 2;
        // ��λ�����ظ����ʱ��
        [SerializeField] private float staminaTimeIncrement = 0.1f;
        // ��ǰ����
        public float currentStamina => m_currentStats.Stamina;
        public float maxStamina => m_sheet.GetMaxStamina();

        public bool isNowCanRun = false;
        public bool isExecutingAction = false;
        public bool isRunning = false;
        private bool useStamina = true;
        private Coroutine regeneratingStamina;
        public UnityEvent<float> currentStaminaChanged => m_currentStats.staminaChanged;
        public UnityEvent<float> maxStaminaChanged => m_maxStats.staminaChanged;

        [Header("Loot")]
        [SerializeField] private AudioClipResolver m_lootingSound;
        [SerializeField] private AudioClipResolver m_lootedSound;

        public bool isLooting => m_isLooting;
        private bool m_isLooting = false;
        private Entity m_lootingObject = null;
        public float lootingTime => m_lootingTime;
        public float lootingRequiredtTime => m_lootingRequiredtTime;
        private float m_lootingTime = 0f;
        private float m_lootingRequiredtTime = 2f;

        [Header("Evacuation")]
        [SerializeField] private AudioClipResolver m_evacuatingSound;
        [SerializeField] private AudioClipResolver m_evacuatedSound;

        private float m_evacuatingTime = 0f;
        private bool m_isEvacuating = false;
        private float m_evacuatingRequiredtTime = 3f;
        public float evacuatingTime => m_evacuatingTime;
        public bool isEvacuating => m_isEvacuating;
        public float evacuatingRequiredtTime => m_evacuatingRequiredtTime;

        [Header("Eyesight")]
        [SerializeField] private Light2D m_heroLight = null;

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
        public float missingCurrentStamina => m_missingCurrentStamina;
        public int usedPoints => m_usedPoints;

        public const int MaxEquipedAbilityCount = 4;

        private Stats m_customStats = new Stats();
        private Stats m_missingCurrentStats = new Stats();
        private float m_missingCurrentStamina = 0;
        private int m_usedPoints = 0;
        private int m_experience = 0;
        private SerializableDictionary<EEquipmentType, Equipment> m_equipments = new SerializableDictionary<EEquipmentType, Equipment>();
        private HashSet<AbilitySheet> m_bonusAbilities = new HashSet<AbilitySheet>();

        private AbilitySheet[] m_equippedAbilities = new AbilitySheet[MaxEquipedAbilityCount];

        // �����õĻ�����������sheet���浼���Ӧ�� abilitysheet
        //ScriptableObject.ctor is not allowed to be called from a MonoBehaviour constructor (or instance field initializer), call it in Awake or Start instead. 
        //private DashAbilitySheet m_dashAbility = ScriptableObject.CreateInstance<DashAbilitySheet>();
        private DashAbilitySheet m_dashAbility = null;

        private UnityEvent<AbilitySheet[]> m_equippedAbilitiesChanged = new UnityEvent<AbilitySheet[]>();

        public bool isDashFinished = false;

        private void OnDeadAnimationStart()
        {
            //Debug.Log("OnDeadAnimationStart");

            // ���ø�������ﳯ��
            SetLookAtDirection(Vector2.right);
        }

        private void OnDeadAnimationEnd()
        {
            GameManager.TeleportLoadingSystem.RequestTransition(null, null, () => {
                // �ָ�Ѫ�� 
                m_currentStats[EStat.Health] = 1;
                m_currentStats[EStat.Mana] = 1;
                m_currentStats.Stamina = GameManager.Player.maxStamina;

                // �ָ���ײ��
                Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
                Array.ForEach(colliders, (collider) => collider.enabled = true);
            } ,
            () => {
                StartCoroutine(SaveWithDelay());
            }, ETeleportType.Revival);
        }

        private IEnumerator SaveWithDelay()
        {
            yield return new WaitForSeconds(1f); // �ȴ�һ��
            GameManager.SaveSystem.SaveToFile(GameManager.SaveSystem.saveFileName); // ��������
        }


        private void OnRevivalAnimationEnd()
        {
            //Debug.Log("OnRevivalAnimationEnd");

            EnableActions(EActionFlags.All);
        }

        public void OnDashAnimationEnd()
        {
            //Debug.Log("OnDashAnimationEnd");

            if (!dead)
            {
                isDashFinished = true;
            }
        }

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

        // û�̳и������� �޷���ö���
        // private new void Awake(){}
        private new void Awake()
        {
            isPlayer = true;

            m_maxStats.staminaChanged.AddListener(OnStaminaChanged);

            base.Awake();
            //m_currentStats.changed.AddListener(HandleStamina);
        }

        protected override void Start()
        {
            // ����о�Ӧ�ò���Ҫִ��һ�μ��� tryexcute


            // �о��������Ч������Ϊ isloop ���������Ϸʱ��ͻ����� ���������� Awake �йر�
            // ���� Awake ��ʱ��û�½������ﱨ���ˣ� ������ Start ��
            GameManager.Player.runParticleSystem.Stop();

            UpdateStats();
        }

        private void Update()
        {
            if (useStamina == true) HandleStamina();

            if (m_isLooting == true) OnTryLooting();

            if (m_isEvacuating == true) OnTryEvacuating();
        }

        public bool CheckIsPlayerMoving()
        {
            return movementDirection == Vector2.zero;
        }

        private void OnTryLooting()
        {
            if (CheckIsPlayerMoving())
            {
                m_lootingTime += Time.deltaTime;
            }
            else
            {
                CancelLooting();
            }

            if (m_lootingTime > m_lootingRequiredtTime)
            {
                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_lootedSound);

                //m_lootingObject.SendMessageUpwards("OnInteract", GameManager.Player);
                GameManager.NotificationSystem.playerEndInteracte.Invoke(GameManager.Player, m_lootingObject);

                CancelLooting();
            }
        }

        private void OnTryEvacuating()
        {
            if (GameManager.Player.CheckIsPlayerMoving())
            {
                m_evacuatingTime += Time.deltaTime;
            }
            else
            {
                CancelEvacuate();
            }

            if (m_evacuatingTime > m_evacuatingRequiredtTime)
            {
                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_evacuatedSound);

                GameManager.TeleportLoadingSystem.RequestTransition("Pilgrimage_Place", null, null, () => {
                    GameManager.SaveSystem.SaveToFile(GameManager.SaveSystem.saveFileName);
                }, ETeleportType.Normal, "Player_Spawner");

                GameManager.DayNightSystem.OnDisableDayNightSystem();

                CancelEvacuate();
            }
        }

        // �о����������£�Ҫô���ɼ��ܣ��ŵ�����£�Ҫô���ø�����ȥ����������Ϊ��ȡ��/
        // ��Ȼһ����չ�Բ��ᣬ���߲�ͣ��Update�м��Ҳ����
        private void CancelEvacuate()
        {
            //TerminateCasting();

            m_evacuatingTime = 0f;
            m_isEvacuating = false;
            GameManager.NotificationSystem.audioStopPlaybackRequested.Invoke(m_evacuatingSound);
        }

        public void OnStarEvacuate()
        {
            GameManager.Player.SetMovementDirection(Vector2.zero);
            m_isEvacuating = true;
            GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_evacuatingSound);
        }

        public void CancelLooting()
        {
            //TerminateCasting();

            m_lootingTime = 0f;
            m_isLooting = false;
            m_lootingObject = null;
            GameManager.NotificationSystem.audioStopPlaybackRequested.Invoke(m_lootingSound);
        }

        public void OnStartLooting(Entity lootingObject, float targetLootTime)
        {
            m_lootingRequiredtTime = targetLootTime;

            SetMovementDirection(Vector2.zero);

            m_isLooting = true;

            m_lootingObject = lootingObject;

            GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_lootingSound);
        }

        public void OnTryStartLoot(Entity interactionTargett, float targetLootTime)
        {
            if (GameManager.Player.isLooting == true)
            {
                CancelLooting();
            }
            else
            {
                OnStartLooting(interactionTargett, targetLootTime);
            }
        }

        private void OnStaminaChanged(float previous)
        {
            float difference = m_maxStats.stamina - previous;
            float newCurrentStamina = m_currentStats.stamina + difference;
            // Make sure we don't kill the character when updating its maximum stats
            newCurrentStamina = math.max(newCurrentStamina, 1);
            m_currentStats.Set(newCurrentStamina);
        }

        public float GetMaxStamina()
        {
            return maxStamina;
        }

        public float GetStamina()
        {
            return m_currentStats.Stamina;
        }

        public void RecoverStamina(int value)
        {
            float missingStamina = maxStamina - m_currentStats.Stamina;
            //m_sheet.SetStamina(GetStamina() + math.min(value, missingStamina));
            m_currentStats.Stamina += math.min(value, missingStamina);
            GameManager.NotificationSystem.staminaRecovered.Invoke(this, value);
        }

        public void ConsumeStamina(int value)
        {
            //m_sheet.SetStamina(m_currentStats.Stamina - math.min(value, m_currentStats.Stamina));
            m_currentStats.Stamina -= math.min(value, m_currentStats.Stamina);
            GameManager.NotificationSystem.staminaConsumed.Invoke(this, value);

        }

        private void HandleStamina()
        {
            // ���״̬�������ƶ����룬��������
            if (isRunning == true && movementDirection != Vector2.zero)
            {
                // ��������ظ�Э�̿������ж�
                if (regeneratingStamina != null)
                {
                    StopCoroutine(regeneratingStamina);
                    regeneratingStamina = null;
                }

                // �����������Ҳ��ͬһ��bool isExecutingAction������ ��ᵼ�²����ܲ�Ҳ��۾���ֵ
                m_currentStats.Stamina -= staminaMultiplier * Time.deltaTime;

                if (m_currentStats.Stamina < 0) m_currentStats.Stamina = 0;

                // ����ֵ���㣬��ֹʹ�ó��
                if (m_currentStats.Stamina <= 0)
                {
                    isNowCanRun = false;
                    EndPlayRunAnimation();
                }
            }

            // ����ʱ������һ��bug������ֻ�ܼ�� run ��ʱ���״̬ �������繥���ͳ�̵Ȳ����ܼ�⣬���¿���һ��������һ�߻ָ�����
            // ����ֵ��������û�г�̣��������ظ�δ����
            if (m_currentStats.Stamina < maxStamina && isRunning == false && isExecutingAction == false && regeneratingStamina == null)
            {
                //Debug.Log("RegenerateStamina");
                //Debug.Log(isExecutingAction);
                regeneratingStamina = StartCoroutine(RegenerateStamina());
            }
        }

        private IEnumerator RegenerateStamina()
        {
            yield return new WaitForSeconds(timeBeforeStaminaRengeStarts);

            WaitForSeconds timeToWait = new WaitForSeconds(staminaTimeIncrement);

            while (isExecutingAction == false && m_currentStats.Stamina < maxStamina)
            {
                // ����0������ʹ�ó��
                if (m_currentStats.Stamina > 0f) isNowCanRun = true;

                RecoverStamina((int)(staminaValueIncrement));

                if (m_currentStats.Stamina > maxStamina)
                    m_currentStats.Stamina = maxStamina;

                yield return timeToWait;
            }
            // �����ظ���ϣ������ÿ�
            regeneratingStamina = null;
        }

        public void SetPlayerHealthToZero()
        {
            m_currentStats[EStat.Health] = 0;
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

            m_maxStats.Set(totalStats);
            m_maxStats.Set(maxStamina);

            ApplyMissingCurrentStats();
        }

        private void ApplyMissingCurrentStats()
        {
            m_currentStats.Set(m_currentStats.stamina - m_missingCurrentStamina);
            m_currentStats.Set(m_currentStats.stats - m_missingCurrentStats);
            m_missingCurrentStats.Reset();
            m_missingCurrentStamina = 0f;
        }

        private void OnLevelUp(bool silentMode = false)
        {
            ++m_level;

            if (!silentMode)
            {
                if (m_restoreHealthOnLevelUp)
                {
                    Heal(m_maxStats[EStat.Health] - m_currentStats[EStat.Health]);
                }

                if (m_restoreManaOnLevelUp)
                {
                    RecoverMana(m_maxStats[EStat.Mana] - m_currentStats[EStat.Mana]);
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
            m_missingCurrentStamina = block.missingCurrentStamina;

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
            //Debug.Log("Hero Death");

            // Prevents the Hero GameObject from being destroyed, so it can be used in the death screen.
            m_destroyOnDeath = false; 
            base.OnDeath();

            GameManager.InventorySystem.EmptyBag();

            GameManager.InventorySystem.UnEquipAll();

            GameManager.DayNightSystem.OnDisableDayNightSystem();

            GameManager.NotificationSystem.deathScreenRequested.Invoke();
        }
    }
}
