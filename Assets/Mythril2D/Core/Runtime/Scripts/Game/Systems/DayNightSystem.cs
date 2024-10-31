﻿using FunkyCode;
using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class DayNightSystem : AGameSystem
    {
        [SerializeField] private LightingManager2D m_lightingManager = null;
        [SerializeField] private float m_maxBrightness = 0.5f;
        [SerializeField] private float m_maxRemainTime = 180f;
        [SerializeField] private float m_maxEmergencyTime = 30f;
        [SerializeField] private UITimeReminder m_timeReminder = null;
        private float m_currentTime = 180f;

        public LightingManager2D lightingManager => m_lightingManager;
        public float maxRemainTime => m_maxRemainTime;
        public float maxEmergencyTime => m_maxEmergencyTime;
        public float currentTime => m_currentTime;
        private bool isOnEnableSystem = false;

        private void Update()
        {
            if (isOnEnableSystem == true) {
                UpdateDayNightSystem();
            }
        }

        private void UpdateDayNightSystem()
        {
            if (m_currentTime > 0f)
            {
                m_currentTime -= Time.deltaTime;

                float newBrightness = m_lightingManager.profile.DarknessColor.r;

                //  maxWhite = m_maxBrightness -> black = 0
                newBrightness = (float)(m_maxBrightness * (m_currentTime / m_maxRemainTime));

                // 同样rgb等于设置灰度
                m_lightingManager.profile.DarknessColor = new Color(newBrightness, newBrightness, newBrightness, 1);
            }

            else if (m_currentTime < 0f)
            {
                if (isOnEnableSystem == true)
                {
                    OnDisableDayNightSystem();

                    m_currentTime = 0f;

                    m_lightingManager.profile.DarknessColor = Color.black;
                }
            }
        }

        public void OnEnableDayNightSystem()
        {
            m_currentTime = m_maxRemainTime;

            isOnEnableSystem = true;

            m_timeReminder.gameObject.SetActive(true);
        }

        public void OnDisableDayNightSystem()
        {
            m_currentTime = m_maxRemainTime;

            isOnEnableSystem = false;

            m_timeReminder.gameObject.SetActive(false);
        }
    }
}
