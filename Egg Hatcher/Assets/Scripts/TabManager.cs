using UnityEngine;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
    [System.Serializable]
    public class TabConfig
    {
        public string tabName;
        public GameObject panelObject;
        public Button tabButton;
    }

    [Header("Tab Setup Configuration")]
    public TabConfig[] allTabs;

    private int lastActiveTabIndex = 0;

    void Start()
    {
        for (int i = 0; i < allTabs.Length; i++)
        {
            int tabIndex = i;
            if (allTabs[i].tabButton != null)
            {
                allTabs[i].tabButton.onClick.RemoveAllListeners();
                allTabs[i].tabButton.onClick.AddListener(() => SwitchToTab(tabIndex));
            }
        }
        SwitchToTab(0);
    }

    public void HideAllPanels()
    {
        foreach (var tab in allTabs)
        {
            if (tab.panelObject != null)
            {
                tab.panelObject.SetActive(false);
            }
        }
    }

    public void RestoreLastActivePanel()
    {
        SwitchToTab(lastActiveTabIndex);
    }



    // Your provided method
    public void SwitchToTab(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= allTabs.Length) return;

        lastActiveTabIndex = targetIndex;

        EggHatcher hatcher = Object.FindFirstObjectByType<EggHatcher>();
        SettingsManager settings = Object.FindFirstObjectByType<SettingsManager>();

        for (int i = 0; i < allTabs.Length; i++)
        {
            bool shouldBeActive = (i == targetIndex);

            if (allTabs[i].panelObject != null)
            {
                // FIXED: This forces the panel containing the button to turn back on cleanly!
                allTabs[i].panelObject.SetActive(shouldBeActive);
            }

            if (allTabs[i].tabButton != null)
            {
                allTabs[i].tabButton.interactable = !shouldBeActive;
            }
        }

        if (hatcher != null && hatcher.eggProgressBar != null)
        {
            hatcher.eggProgressBar.gameObject.SetActive(targetIndex == 0);
        }

        if (hatcher != null && hatcher.eggController != null)
        {
            hatcher.eggController.gameObject.SetActive(targetIndex == 0);
        }

        if (settings != null && settings.openJournalButton != null)
        {
            settings.openJournalButton.gameObject.SetActive(targetIndex == 0);
        }
    }
}