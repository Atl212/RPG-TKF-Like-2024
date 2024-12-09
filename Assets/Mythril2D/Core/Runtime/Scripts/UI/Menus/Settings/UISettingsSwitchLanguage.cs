using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

namespace Gyvr.Mythril2D
{
    public class UISettingsSwitchLanguage : MonoBehaviour
    {
        [SerializeField] private Button m_button = null;

        private Coroutine imageAlphaCoroutine;
        private Coroutine colorCoroutine;

        public Button button => m_button;

        private void Awake()
        {
            if (m_button != null)
            {
                m_button.onClick.AddListener(OnClick);
            }
        }
        public void OnClick()
        {
            SwitchLanguage();
        }

        public void SwitchLanguage()
        {
            // �л�����һ������
            GameManager.LocalizationSystem.currentLocaleIndex = (GameManager.LocalizationSystem.currentLocaleIndex + 1) % GameManager.LocalizationSystem.locales.Count; // ѭ��������������
            LocalizationSettings.SelectedLocale = GameManager.LocalizationSystem.locales[GameManager.LocalizationSystem.currentLocaleIndex]; // ��������
        }
    }
}
