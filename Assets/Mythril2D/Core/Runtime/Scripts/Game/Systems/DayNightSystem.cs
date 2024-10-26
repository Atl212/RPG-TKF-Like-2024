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
        private float m_currentTime = 180f;

        public LightingManager2D lightingManager => m_lightingManager;
        public float maxRemainTime => m_maxRemainTime;
        public float maxEmergencyTime => m_maxEmergencyTime;
        public float currentTime => m_currentTime;

        private void Update()
        {
            m_currentTime -= Time.deltaTime;

            if(m_currentTime > 0f)
            {
                float newBrightness = m_lightingManager.profile.DarknessColor.r;

                //  maxWhite = m_maxBrightness -> black = 0
                newBrightness = (float) (m_maxBrightness * (m_currentTime / m_maxRemainTime));

                m_lightingManager.profile.DarknessColor = new Color(newBrightness, newBrightness, newBrightness, 1);
            }

            else if (Mathf.Approximately(1.0f, m_currentTime / m_maxRemainTime))
            {
                m_lightingManager.profile.DarknessColor = new Color(0, 0, 0, 1);
            }
        }
    }
}