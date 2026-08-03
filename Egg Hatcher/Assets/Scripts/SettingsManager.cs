using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public Toggle ryanModeToggle;
    public TMP_Text totalTapsText;
    public Button resetButton;
    public Button openSettingsButton;
    public Button closeSettingsButton;

    [Header("Journal Integration")]
    public Button openJournalButton;

    [Header("Title Screen Integration")]
    // REQUIRED: Drag your new BackToTitleButton here in the Inspector
    public Button backToTitleButton;

    [Header("Tab System Integration")]
    public TabManager tabManager;
    public GameObject homeTab;
    public GameObject upgradeTab;

    [Header("Tab Buttons")]
    public Button homeTabButton;
    public Button upgradeTabButton;

    [Header("Reset Popup")]
    public GameObject resetPopupPanel;
    public Button confirmResetButton;
    public Button cancelResetButton;

    [Header("Audio")]
    public AudioSource backgroundMusic;
    public AudioClip normalMusic;
    public AudioClip ryanMusic;

    private int totalTaps = 0;
    private bool ryanMode = false;

    void Start()
    {
        totalTaps = PlayerPrefs.GetInt("TotalTaps", 0);
        ryanMode = PlayerPrefs.GetInt("RyanMode", 0) == 1;

        if (ryanModeToggle != null) ryanModeToggle.isOn = ryanMode;
        UpdateTotalTapsText();
        UpdateMusic();

        if (openSettingsButton != null) openSettingsButton.onClick.AddListener(OpenSettings);
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettings);
        if (ryanModeToggle != null) ryanModeToggle.onValueChanged.AddListener(OnRyanModeChanged);

        if (resetButton != null) resetButton.onClick.AddListener(ShowResetConfirmation);
        if (confirmResetButton != null) confirmResetButton.onClick.AddListener(PerformReset);
        if (cancelResetButton != null) cancelResetButton.onClick.AddListener(CloseResetPopup);

        // FIXED: Dynamically listen for the Back to Title Screen button click event
        if (backToTitleButton != null) backToTitleButton.onClick.AddListener(ReturnToTitleScreen);

        if (resetPopupPanel != null) resetPopupPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);

        if (homeTab != null) homeTab.SetActive(false);
        if (upgradeTab != null) upgradeTab.SetActive(false);
        if (tabManager != null) tabManager.HideAllPanels();

        if (homeTabButton != null) homeTabButton.gameObject.SetActive(false);
        if (upgradeTabButton != null) upgradeTabButton.gameObject.SetActive(false);
        if (openSettingsButton != null) openSettingsButton.gameObject.SetActive(false);

        EggHatcher hatcher = Object.FindFirstObjectByType<EggHatcher>();
        if (hatcher != null && hatcher.eggProgressBar != null) hatcher.eggProgressBar.gameObject.SetActive(false);
        if (hatcher != null && hatcher.eggController != null) hatcher.eggController.gameObject.SetActive(false);
        if (openJournalButton != null) openJournalButton.gameObject.SetActive(false);

        UpdateTotalTapsText();
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (openSettingsButton != null) openSettingsButton.gameObject.SetActive(true);
        if (homeTabButton != null) homeTabButton.gameObject.SetActive(true);
        if (upgradeTabButton != null) upgradeTabButton.gameObject.SetActive(true);

        if (homeTabButton != null) homeTabButton.interactable = true;
        if (upgradeTabButton != null) upgradeTabButton.interactable = true;

        if (tabManager != null)
        {
            tabManager.RestoreLastActivePanel();
        }
        else
        {
            if (homeTab != null) homeTab.SetActive(true);
            EggHatcher hatcher = Object.FindFirstObjectByType<EggHatcher>();
            if (hatcher != null && hatcher.eggProgressBar != null) hatcher.eggProgressBar.gameObject.SetActive(true);
            if (hatcher != null && hatcher.eggController != null) hatcher.eggController.gameObject.SetActive(true);
            if (openJournalButton != null) openJournalButton.gameObject.SetActive(true);
        }
    }

    // FIXED: Saves data, shuts off gameplay UI panels, and brings up the main Title Screen layout
    public void ReturnToTitleScreen()
    {
        Debug.Log("[SETTINGS SYSTEM] Returning safely to main menu...");

        // Force an immediate data autosave so players don't lose progress when backing out
        EggHatcher hatcher = Object.FindFirstObjectByType<EggHatcher>();
        if (hatcher != null)
        {
            hatcher.SaveGame();
        }

        // Hide the active settings frame panel
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Tell the Title Screen Manager to pull the main menu back up
        EggClickerGame.TitleScreenManager titleScreen = Object.FindFirstObjectByType<EggClickerGame.TitleScreenManager>();
        if (titleScreen != null)
        {
            titleScreen.ShowTitleMenuFromGameplay();
        }
    }

    private void OnRyanModeChanged(bool isOn)
    {
        ryanMode = isOn;
        PlayerPrefs.SetInt("RyanMode", isOn ? 1 : 0);
        UpdateMusic();
    }

    private void UpdateMusic()
    {
        if (backgroundMusic == null) return;
        if (ryanMode && ryanMusic != null) backgroundMusic.clip = ryanMusic;
        else backgroundMusic.clip = normalMusic;
        backgroundMusic.Play();
    }

    public void UpdateTotalTapsText()
    {
        int manualTaps = PlayerPrefs.GetInt("TotalTaps", 0);
        if (totalTapsText != null) totalTapsText.text = "Total Taps: " + manualTaps;
    }

    public void ShowResetConfirmation()
    {
        if (resetPopupPanel != null) resetPopupPanel.SetActive(true);
    }

    public void PerformReset()
    {
        Debug.Log("[SETTINGS] Resetting game variables selectively...");

        // Core Currency Clearance
        PlayerPrefs.DeleteKey("TotalTaps");
        PlayerPrefs.DeleteKey("EggsCurrency");
        PlayerPrefs.DeleteKey("EggHatched");
        PlayerPrefs.DeleteKey("AutoHatchCount");
        PlayerPrefs.DeleteKey("AutoHatchInterval");

        // Clean sweep across precise, separate shop keys
        PlayerPrefs.DeleteKey("Cost_TapStrength");
        PlayerPrefs.DeleteKey("Level_TapStrength");
        PlayerPrefs.DeleteKey("Cost_AutoHatcher");
        PlayerPrefs.DeleteKey("Level_AutoHatcher");
        PlayerPrefs.DeleteKey("Cost_HatchSpeed");
        PlayerPrefs.DeleteKey("Level_HatchSpeed");

        // Double-check spelling variants just in case
        PlayerPrefs.DeleteKey("Cost_Auto Hatcher");
        PlayerPrefs.DeleteKey("Level_Auto Hatcher");

        // FIXED: Removed PlayerPrefs.DeleteAll() to protect your creature log dictionary registers!
        if (EggClickerGame.CreatureJournalManager.Instance != null)
        {
            EggClickerGame.CreatureJournalManager.Instance.WipeCollection();
        }

        PlayerPrefs.Save();

        string path = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("JSON Save file deleted successfully.");
        }

        Debug.Log("Wipe complete. Reloading environment...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }



    public void CloseResetPopup()
    {
        if (resetPopupPanel != null) resetPopupPanel.SetActive(false);
    }
}
