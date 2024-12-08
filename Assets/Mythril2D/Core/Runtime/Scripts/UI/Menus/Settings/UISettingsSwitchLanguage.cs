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
        private int currentLocaleIndex = 0; // ��ǰ��������

        private void Awake()
        {
            if (m_button != null)
            {
                m_button.onClick.AddListener(OnClick);
            }
        }
        private void Start()
        {
            // ��ʼ����ǰ��������
            if (LocalizationSettings.SelectedLocale != null)
            {
                var locales = LocalizationSettings.AvailableLocales.Locales;
                currentLocaleIndex = locales.IndexOf(LocalizationSettings.SelectedLocale);
            }
        }

        public void OnClick()
        {
            SwitchLanguage();
        }

        public void SwitchLanguage()
        {
            var locales = LocalizationSettings.AvailableLocales.Locales; // ��ȡ���п��������б�

            if (locales.Count == 0)
            {
                Debug.LogWarning("No available locales found!");
                return;
            }

            // �л�����һ������
            currentLocaleIndex = (currentLocaleIndex + 1) % locales.Count; // ѭ��������������
            LocalizationSettings.SelectedLocale = locales[currentLocaleIndex]; // ��������
        }
    }
}
