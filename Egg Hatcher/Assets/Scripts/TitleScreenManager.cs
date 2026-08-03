using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EggClickerGame
{
    public class TitleScreenManager : MonoBehaviour
    {
        [Header("Main Title Screen Panels")]
        public GameObject titleScreenPanel;
        public GameObject gameUIElementsPanel;

        [Header("Title Buttons")]
        public Button startButton;
        public Button settingsButton;
        public Button infoCreditsButton;
        public Button exitButton;

        [Header("Title Settings Sub-Menu")]
        public GameObject titleSettingsPanel;
        public Button titleSettingsCloseButton;

        [Header("Credits Popup Window Layout")]
        public GameObject creditsPopupPanel;
        public Button creditsCloseButton;
        public TMP_Text creditsText;

        void Awake()
        {
            if (titleScreenPanel != null) titleScreenPanel.SetActive(true);
            if (gameUIElementsPanel != null) gameUIElementsPanel.SetActive(false);
            if (creditsPopupPanel != null) creditsPopupPanel.SetActive(false);
            if (titleSettingsPanel != null) titleSettingsPanel.SetActive(false);
        }

        void Start()
        {
            if (startButton != null) startButton.onClick.AddListener(StartGame);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenTitleSettings);
            if (titleSettingsCloseButton != null) titleSettingsCloseButton.onClick.AddListener(CloseTitleSettings);
            if (infoCreditsButton != null) infoCreditsButton.onClick.AddListener(OpenCreditsPopup);
            if (exitButton != null) exitButton.onClick.AddListener(ExitGameApplication);
            if (creditsCloseButton != null) creditsCloseButton.onClick.AddListener(CloseCreditsPopup);

            if (creditsText != null)
            {
                creditsText.text = "<b><color=yellow>--- GAME CREDITS ---</color></b>\n\n" +
                                   "<b>Lead Programmer / Designer:</b>\nChrispy and PebZap\n\n" +
                                   "<b>Special Thanks:</b>\nRyan (Ryan Mode Integration)\n\n" +
                                   "Thank you for playing our Egg Hatchers!";
            }
        }

        public void StartGame()
        {
            Debug.Log("[TITLE SYSTEM] Transitioning to core gameplay panels...");

            if (titleScreenPanel != null) titleScreenPanel.SetActive(false);
            if (gameUIElementsPanel != null) gameUIElementsPanel.SetActive(true);

            EggHatcher hatcher = Object.FindFirstObjectByType<EggHatcher>();
            if (hatcher != null)
            {
                if (hatcher.eggProgressBar != null)
                {
                    hatcher.eggProgressBar.gameObject.SetActive(true);
                }
                if (hatcher.eggController != null)
                {
                    hatcher.eggController.gameObject.SetActive(true);
                }

                // FIXED: Forcefully restore visibility to the navigation bar and settings buttons 
                // when entering play mode so they never get stuck hidden in long-term memory!
                if (hatcher.settingsManager != null)
                {
                    if (hatcher.settingsManager.homeTabButton != null)
                    {
                        hatcher.settingsManager.homeTabButton.gameObject.SetActive(true);
                    }
                    if (hatcher.settingsManager.upgradeTabButton != null)
                    {
                        hatcher.settingsManager.upgradeTabButton.gameObject.SetActive(true);
                    }
                    if (hatcher.settingsManager.openSettingsButton != null)
                    {
                        hatcher.settingsManager.openSettingsButton.gameObject.SetActive(true);
                    }
                    if (hatcher.settingsManager.openJournalButton != null)
                    {
                        hatcher.settingsManager.openJournalButton.gameObject.SetActive(true);
                    }

                    // Cleanly force the TabManager to initialize and display Tab 0 (Home Page)
                    if (hatcher.settingsManager.tabManager != null)
                    {
                        hatcher.settingsManager.tabManager.SwitchToTab(0);
                    }
                }

                // Execute save manager initialization loader
                if (hatcher.saveManager != null)
                {
                    hatcher.saveManager.LoadGame(hatcher);
                }
            }
        }



        // FIXED: Allows external scripts to forcefully disable active gameplay and snap back to titles
        public void ShowTitleMenuFromGameplay()
        {
            if (gameUIElementsPanel != null) gameUIElementsPanel.SetActive(false);
            if (titleScreenPanel != null) titleScreenPanel.SetActive(true);

            // Forcefully hide your active tooltip background if it's currently floating
            if (UpgradeTooltipManager.Instance != null)
            {
                UpgradeTooltipManager.Instance.HideTooltip();
            }
        }

        public void OpenTitleSettings()
        {
            if (titleSettingsPanel != null) titleSettingsPanel.SetActive(true);
        }

        public void CloseTitleSettings()
        {
            if (titleSettingsPanel != null) titleSettingsPanel.SetActive(false);
        }

        public void OpenCreditsPopup()
        {
            if (creditsPopupPanel != null) creditsPopupPanel.SetActive(true);
        }

        public void CloseCreditsPopup()
        {
            if (creditsPopupPanel != null) creditsPopupPanel.SetActive(false);
        }

        public void ExitGameApplication()
        {
            Debug.Log("[TITLE SYSTEM] Closing game runtime instance.");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }
    }
}
