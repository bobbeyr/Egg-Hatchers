using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;

public class SettingsManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public Toggle ryanModeToggle;
    public TMP_Text totalTapsText;
    public Button resetButton;
    public Button openSettingsButton;

    public GameObject homePanel;
    public GameObject upgradePanel;

    public GameObject homeTab;
    public GameObject upgradeTab;

    public GameObject resetPopupPanel; // Confirmation popup
    public Button confirmResetButton;  // "Yes"
    public Button cancelResetButton;   // "No"

    public AudioSource backgroundMusic;
    public AudioClip normalMusic;
    public AudioClip ryanMusic;

    private int totalTaps = 0;
    private bool ryanMode = false;

    private GameObject previousPanel;

    void Start()
    {
        totalTaps = PlayerPrefs.GetInt("TotalTaps", 0);
        ryanMode = PlayerPrefs.GetInt("RyanMode", 0) == 1;
        ryanModeToggle.isOn = ryanMode;

        UpdateTotalTapsText();
        UpdateMusic();

        if (openSettingsButton != null)
            openSettingsButton.onClick.AddListener(OpenSettings);
        if (ryanModeToggle != null)
            ryanModeToggle.onValueChanged.AddListener(OnRyanModeChanged);
        if (resetButton != null)
            resetButton.onClick.AddListener(ShowResetConfirmation);

        if (confirmResetButton != null)
            confirmResetButton.onClick.AddListener(PerformReset);
        if (cancelResetButton != null)
            cancelResetButton.onClick.AddListener(CloseResetPopup);

        if (resetPopupPanel != null)
            resetPopupPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        previousPanel = null;

        if (openSettingsButton != null)
            openSettingsButton.gameObject.SetActive(false);
        if (homeTab != null)
            homeTab.SetActive(false);
        if (upgradeTab != null)
            upgradeTab.SetActive(false);

        if (homePanel != null && homePanel.activeSelf)
            previousPanel = homePanel;
        if (upgradePanel != null && upgradePanel.activeSelf)
            previousPanel = upgradePanel;

        if (previousPanel != null)
            previousPanel.SetActive(false);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        if (openSettingsButton != null)
            openSettingsButton.gameObject.SetActive(true);
        if (homeTab != null)
            homeTab.SetActive(true);
        if (upgradeTab != null)
            upgradeTab.SetActive(true);
        if (previousPanel != null)
            previousPanel.SetActive(true);
    }

    private void OnRyanModeChanged(bool isOn)
    {
        ryanMode = isOn;
        PlayerPrefs.SetInt("RyanMode", isOn ? 1 : 0);
        UpdateMusic();
    }

    private void UpdateMusic()
    {
        if (ryanMode && ryanMusic != null)
            backgroundMusic.clip = ryanMusic;
        else
            backgroundMusic.clip = normalMusic;

        backgroundMusic.Play();
    }

    public void AddTaps(int count)
    {
        totalTaps += count;
        PlayerPrefs.SetInt("TotalTaps", totalTaps);
        UpdateTotalTapsText();
    }

    private void UpdateTotalTapsText()
    {
        if (totalTapsText != null)
            totalTapsText.text = "Total Taps: " + totalTaps;
    }

    // Show confirmation popup when reset is pressed
    public void ShowResetConfirmation()
    {
        if (resetPopupPanel != null)
            resetPopupPanel.SetActive(true);
    }

    // Perform the reset
    // In your `PerformReset()` method, update to delete the save file:

    public void PerformReset()
    {
        Debug.Log("Resetting game...");
        PlayerPrefs.DeleteKey("TotalTaps");
        PlayerPrefs.DeleteKey("EggHatched");
        PlayerPrefs.DeleteKey("AutoHatchCount");
        PlayerPrefs.Save();

        // Delete the save file explicitly
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
        if (resetPopupPanel != null)
            resetPopupPanel.SetActive(false);
    }
}