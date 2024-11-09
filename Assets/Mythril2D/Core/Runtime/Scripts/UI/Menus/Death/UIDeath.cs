using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIDeath : MonoBehaviour, IUIMenu
    {
        [Header("References")]
        [SerializeField] private Button m_revivalButton = null;
        [SerializeField] private Button m_quitButton = null;

        public void Init()
        {
        }

        public void Show(params object[] args)
        {
            gameObject.SetActive(true);
        }

        //public bool CanPop() => false;
        // �������治Ӧ���ܹ����� ������������Ϊ false �޷�pop
        public bool CanPop(){
            return false;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void EnableInteractions(bool enable)
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup)
            {
                canvasGroup.interactable = enable;
            }
        }

        public GameObject FindSomethingToSelect()
        {
            return m_revivalButton.gameObject;
            //return m_quitButton.gameObject;
        }

        public void RevivePlayer()
        {
            GameManager.UIManagerSystem.UIMenu.PopDeathMenu();

            GameManager.DayNightSystem.OnDisableDayNightSystem();

            // ��ʱ������� GameStateSystem ��������ģ����������������״̬���ܻ�һֱ���� UI ��
            // ������û�취�������������Ϊ��
            GameManager.GameStateSystem.AddLayer(EGameState.Gameplay);

            GameManager.Player.TryPlayRevivalAnimation();
        }

        public void GoToMainMenu()
        {
            SceneManager.LoadScene(GameManager.Config.mainMenuSceneName);
        }
    }
}
