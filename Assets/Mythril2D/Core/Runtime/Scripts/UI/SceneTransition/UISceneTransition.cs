﻿using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class UISceneTransition : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string m_fadeInAnimationParameter;
        [SerializeField] private string m_fadeOutAnimationParameter;
        [SerializeField] private string m_skipFadeOutAnimationParameter;

        [Header("References")]
        [SerializeField] private Animator m_animator;

        private bool m_hasFadeInAnimation = false;
        private bool m_hasFadeOutAnimation = false;
        private bool m_hasSkipFadeOutAnimation = false;

        private TeleportLoadingDelegationParams m_teleportLoadingDelegationParams = null;

        private void Awake()
        {
            Debug.Assert(m_animator, ErrorMessages.InspectorMissingComponentReference<Animator>());

            m_hasFadeInAnimation = AnimationUtils.HasParameter(m_animator, m_fadeInAnimationParameter);
            m_hasFadeOutAnimation = AnimationUtils.HasParameter(m_animator, m_fadeOutAnimationParameter);
            m_hasSkipFadeOutAnimation = AnimationUtils.HasParameter(m_animator, m_skipFadeOutAnimationParameter);
        }

        private void OnEnable()
        {
            GameManager.NotificationSystem.mapTransitionDelegationRequested.AddListener(OnTeleportTransitionDelegationRequested);
        }

        private void OnDisable()
        {
            GameManager.NotificationSystem.mapTransitionDelegationRequested.RemoveListener(OnTeleportTransitionDelegationRequested);
        }

        private void OnTeleportTransitionDelegationRequested(TeleportLoadingDelegationParams delegationParams)
        {
            m_teleportLoadingDelegationParams = delegationParams;

            if (delegationParams.unloadDelegate != null)
            {
                TryPlayFadeOutTransition();
            }
            else
            {
                TryShowBlackScreen();

                m_teleportLoadingDelegationParams.loadDelegate(() =>
                {
                    TryPlayFadeInTransition();
                });
            }
        }

        // Invoked by the StateMessageDispatcher attached to the FadeOut animation in the animation controller 
        private void OnFadeOutCompleted()
        {
            m_teleportLoadingDelegationParams.unloadDelegate(() =>
            {
                m_teleportLoadingDelegationParams.loadDelegate(() =>
                {
                    TryPlayFadeInTransition();
                });
            });
        }

        // Invoked by the StateMessageDispatcher attached to the FadeIn animation in the animation controller 
        private void OnFadeInCompleted()
        {
            m_teleportLoadingDelegationParams.completionDelegate();
        }

        public bool TryPlayFadeInTransition()
        {
            if (m_hasFadeInAnimation)
            {
                m_animator.SetTrigger(m_fadeInAnimationParameter);
                return true;
            }

            return false;
        }

        public bool TryPlayFadeOutTransition()
        {
            if (m_hasFadeOutAnimation)
            {
                m_animator.SetTrigger(m_fadeOutAnimationParameter);
                return true;
            }

            return false;
        }

        public bool TryShowBlackScreen()
        {
            if (m_hasSkipFadeOutAnimation)
            {
                m_animator.SetTrigger(m_skipFadeOutAnimationParameter);
                return true;
            }

            return false;
        }
    }
}
