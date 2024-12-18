using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public struct MonsterSpawn
    {
        public GameObject prefab;
        public int rate;
    }

    public abstract class AMonsterSpawner : MonoBehaviour
    {
        [Header("General Settings")]
        //[SerializeField] private MonsterSpawn[] m_monsters = null;
        [SerializeField] private bool isUseGroup = false;
        [SerializeField] private GameObject monsterPrefabs = null;
        [SerializeField] private int rate = 100;
        [SerializeField][Range(Stats.MinLevel, Stats.MaxLevel)] private int m_minLevel = Stats.MinLevel;
        [SerializeField][Range(Stats.MinLevel, Stats.MaxLevel)] private int m_maxLevel = Stats.MaxLevel;

        [Header("Spawn Settings")]
        [SerializeField] private float m_spawnCooldown = 5.0f;
        [SerializeField] private int m_monstersToPrespawn = 4;
        [SerializeField] private int m_maxSimulatenousMonsterCount = 4;

        [Header("Spawn Limitations")]
        [SerializeField] private bool m_limitMonsterCount = false;
        [SerializeField][Min(1)] private int m_maxMonsterCount = 1;

        private HashSet<Monster> m_spawnedMonsters = new HashSet<Monster>();

        // Private Members
        private float m_spawnTimer = 0.0f;
        private bool m_valid = false;

        private int m_totalSpawnedMonsterCount = 0;

        // Used for the first update to prespawn monsters
        private bool m_isFirstUpdate = true;

        protected abstract Vector2 FindSpawnLocation();

        private void Prespawn()
        {
            for (int i = 0; i < m_monstersToPrespawn; ++i)
            {
                TrySpawn();
            }
        }

        private void Update()
        {
            if (m_isFirstUpdate)
            {
                Prespawn();
                m_isFirstUpdate = false;
            }

            if (m_valid && m_spawnedMonsters.Count < m_maxSimulatenousMonsterCount)
            {
                m_spawnTimer += Time.deltaTime;

                if (m_spawnTimer > m_spawnCooldown)
                {
                    m_spawnTimer = 0.0f;
                    TrySpawn();
                }
            }
        }

        private GameObject FindMonsterToSpawn()
        {
            int randomNumber = UnityEngine.Random.Range(0, 100);

            if (randomNumber <= rate)
            {
                return monsterPrefabs;
            }
            else
            {
                return null;
            }
        }

        private bool CanSpawn()
        {
            return !m_limitMonsterCount || m_totalSpawnedMonsterCount < m_maxMonsterCount;
        }

        private void TrySpawn()
        {
            if (CanSpawn())
            {
                Spawn();
            }
        }

        private void Spawn()
        {
            Vector2 position = FindSpawnLocation();
            GameObject monster = FindMonsterToSpawn();

            if (monster != null)
            {
                GameObject instance = Instantiate(monster, position, Quaternion.identity, transform);
                instance.transform.parent = null;

                if (isUseGroup == true)
                {
                    MonsterGroup monsterGroup = instance.GetComponent<MonsterGroup>();

                    foreach (Monster child in monsterGroup.monsters)
                    {
                        SetMonster(child);
                    }
                }
                else
                {
                    Monster monsterComponent = instance.GetComponent<Monster>();
                    ++m_totalSpawnedMonsterCount;

                    SetMonster(monsterComponent);
                }
            }
            else
            {
                Debug.LogError("Couldn't find a monster to spawn, please check your spawn rates and make sure their sum is 100");
            }
        }

        private void SetMonster(Monster monster)
        {
            if (monster)
            {
                // 设置子对象怪物等级
                monster.SetLevel(UnityEngine.Random.Range(m_minLevel, m_maxLevel));

                // 添加销毁监听
                // 如果这里增加对销毁的监听 为什么不需要考虑上面怪物数量的 -- ？
                monster.destroyed.AddListener(() => m_spawnedMonsters.Remove(monster));
                m_spawnedMonsters.Add(monster);
            }
            else
            {
                Debug.LogError("No Monster component found on the monster prefab");
            }
        }
    }
}
