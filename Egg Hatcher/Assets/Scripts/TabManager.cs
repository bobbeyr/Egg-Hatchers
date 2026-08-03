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

    public void SwitchToTab(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= allTabs.Length) return;

        lastActiveTabIndex = targetIndex;

        EggHatcher hatcher = Object.FindFirstObjectByType<EggHatcher>();

        for (int i = 0; i < allTabs.Length; i++)
        {
            bool shouldBeActive = (i == targetIndex);

            if (allTabs[i].panelObject != null)
            {
                allTabs[i].panelObject.SetActive(shouldBeActive);
            }

            if (allTabs[i].tabButton != null)
            {
                allTabs[i].tabButton.interactable = !shouldBeActive;
            }
        }

        // FIXED: Explicitly toggle the progress bar visibility based on the active tab page
        if (hatcher != null && hatcher.eggProgressBar != null)
        {
            hatcher.eggProgressBar.gameObject.SetActive(targetIndex == 0);
        }

        // FIXED: Explicitly toggle the egg image visibility based on the active tab page
        if (hatcher != null && hatcher.eggController != null)
        {
            hatcher.eggController.gameObject.SetActive(targetIndex == 0);
        }
    }
}
