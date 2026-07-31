using UnityEngine;

public class TabManager : MonoBehaviour
{
    public GameObject homePanel;
    public GameObject upgradePanel;

    // Keep track of which panel is currently supposed to be visible
    private bool isHomeActive = true;

    void Start()
    {
        ShowHome();
    }

    public void ShowHome()
    {
        homePanel.SetActive(true);
        upgradePanel.SetActive(false);
        isHomeActive = true;
    }

    public void ShowUpgrades()
    {
        homePanel.SetActive(false);
        upgradePanel.SetActive(true);
        isHomeActive = false;
    }

    // NEW: Tells both panels to hide when settings open
    public void HideAllPanels()
    {
        if (homePanel != null) homePanel.SetActive(false);
        if (upgradePanel != null) upgradePanel.SetActive(false);
    }

    // NEW: Automatically restores whichever panel was active before settings opened
    public void RestoreLastActivePanel()
    {
        if (isHomeActive)
        {
            ShowHome();
        }
        else
        {
            ShowUpgrades();
        }
    }
}
