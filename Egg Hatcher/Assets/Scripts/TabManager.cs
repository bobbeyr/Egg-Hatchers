using UnityEngine;

public class TabManager : MonoBehaviour
{
    public GameObject homePanel;
    public GameObject upgradePanel;

    void Start()
    {
        ShowHome(); // Show home panel at start
    }

    public void ShowHome()
    {
        homePanel.SetActive(true);
        upgradePanel.SetActive(false);
    }

    public void ShowUpgrades()
    {
        homePanel.SetActive(false);
        upgradePanel.SetActive(true);
    }
}