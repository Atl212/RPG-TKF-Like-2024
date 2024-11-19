using FunkyCode;
using System.Collections;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class Warehouse : OtherEntity
    {
        [Header("References")]
        [SerializeField] private Animator m_warehouAnimator = null;

        [Header("Warehouse Settings")]
        [SerializeField] private string m_gameFlagID = "warehouse_00";
        [SerializeField] private string m_openedAnimationParameter = "opened";
        [SerializeField] private string m_contentRevealAnimationParameter = "reveal";
        [SerializeField] private float m_contentRevealIconCycleDuration = 1.0f;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_openingSound;

        private bool m_hasOpeningAnimation = false;
        private bool m_hasRevealAnimation = false;
        private bool m_opened = false;

        protected void Awake()
        {
            Debug.Assert(m_warehouAnimator, ErrorMessages.InspectorMissingComponentReference<Animator>());

            if (m_warehouAnimator)
            {
                m_hasOpeningAnimation = AnimationUtils.HasParameter(m_warehouAnimator, m_openedAnimationParameter);
            }

        }

        protected override void Start()
        {
            base.Start();

            GameManager.NotificationSystem.UIWarehouseClosed.AddListener(TryClose);
        }

        public bool TryPlayOpeningAnimation(bool open)
        {
            if (m_warehouAnimator && m_hasOpeningAnimation)
            {
                m_warehouAnimator.SetBool(m_openedAnimationParameter, open);
                return true;
            }

            return false;
        }

        public bool TryPlayClosingAnimation(bool open)
        {
            // ����һ������ ֻ�����Ƿ����� ������boolΪfalse����
            if (m_warehouAnimator && m_hasOpeningAnimation)
            {
                m_warehouAnimator.SetBool(m_openedAnimationParameter, !open);
                return true;
            }

            return false;
        }

        private IEnumerator InvokeWarehouseRequestedAfterAnimation()
        {
            if (m_warehouAnimator != null)
            {
                // �ȴ���������ʱ��
                float animationDuration = m_warehouAnimator.GetCurrentAnimatorStateInfo(0).length;
                yield return new WaitForSeconds(animationDuration);
            }

            // ����������ϣ����òֿ�����
            GameManager.NotificationSystem.warehouseRequested.Invoke();
        }


        public bool TryOpen()
        {
            if (m_opened == false)
            {
                GameManager.Player.DisableActions(EActionFlags.All);

                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_openingSound);

                TryPlayOpeningAnimation(true);

                // ����Э�̣��ڶ������������÷���
                StartCoroutine(InvokeWarehouseRequestedAfterAnimation());

                return m_opened = true;
            }

            return false;
        }

        public void TryClose()
        {
            if (m_opened == true)
            {
                GameManager.Player.EnableActions(EActionFlags.All);

                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_openingSound);

                TryPlayClosingAnimation(true);

                m_opened = false;
            }
        }
    }
}
