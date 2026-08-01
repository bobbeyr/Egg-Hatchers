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

        if (ryanModeToggle != null)
            ryanModeToggle.isOn = ryanMode;

        UpdateTotalTapsText();
        UpdateMusic();

        if (openSettingsButton != null) openSettingsButton.onClick.AddListener(OpenSettings);
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettings);
        if (ryanModeToggle != null) ryanModeToggle.onValueChanged.AddListener(OnRyanModeChanged);
        if (resetButton != null) resetButton.onClick.AddListener(ShowResetConfirmation);
        if (confirmResetButton != null) confirmResetButton.onClick.AddListener(PerformReset);
        if (cancelResetButton != null) cancelResetButton.onClick.AddListener(CloseResetPopup);

        if (resetPopupPanel != null) resetPopupPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        if (homeTab != null) homeTab.SetActive(false);
        if (upgradeTab != null) upgradeTab.SetActive(false);
        if (tabManager != null) tabManager.HideAllPanels();
        if (homeTabButton != null) homeTabButton.interactable = false;
        if (upgradeTabButton != null) upgradeTabButton.interactable = false;
        if (openSettingsButton != null) openSettingsButton.gameObject.SetActive(false);

        UpdateTotalTapsText();
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        if (openSettingsButton != null) openSettingsButton.gameObject.SetActive(true);
        if (homeTab != null) homeTab.SetActive(true);
        if (upgradeTab != null) upgradeTab.SetActive(true);
        if (tabManager != null) tabManager.RestoreLastActivePanel();
        if (homeTabButton != null) homeTabButton.interactable = true;
        if (upgradeTabButton != null) upgradeTabButton.interactable = true;
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

        if (ryanMode && ryanMusic != null)
            backgroundMusic.clip = ryanMusic;
        else
            backgroundMusic.clip = normalMusic;

        backgroundMusic.Play();
    }

    public void UpdateTotalTapsText()
    {
        int manualTaps = PlayerPrefs.GetInt("TotalTaps", 0);
        if (totalTapsText != null)
            totalTapsText.text = "Total Taps: " + manualTaps;
    }

    public void ShowResetConfirmation()
    {
        if (resetPopupPanel != null) resetPopupPanel.SetActive(true);
    }

    public void PerformReset()
    {
        Debug.Log("Resetting game...");
        PlayerPrefs.DeleteKey("TotalTaps");
        PlayerPrefs.DeleteKey("EggsCurrency");
        PlayerPrefs.DeleteKey("EggHatched");
        PlayerPrefs.DeleteKey("AutoHatchCount");
        PlayerPrefs.Save();

        string path = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Save file deleted.");
        }

        Debug.Log("PlayerPrefs and save file cleared. Reloading scene...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CloseResetPopup()
    {
        if (resetPopupPanel != null) resetPopupPanel.SetActive(false);
    }
}
