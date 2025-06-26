using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PagesManager : MonoBehaviour
{
    [Header("Pages Setup")]
    public List<Transform> pages;

    [Header("Additional panels")]
    public List<Transform> panels;

    protected Transform currentOpenPage;

    public void OpenPage(Transform pageToOpen)
    {
        foreach (var page in pages)
        {
            if (page == null) continue;
            page.gameObject.SetActive(page == pageToOpen);
        }
        currentOpenPage = pageToOpen;
    }

    public void OpenPageByIndex(int index)
    {
        if (index < 0 || index >= pages.Count)
        {
            Debug.LogWarning("Invalid page index: " + index);
            return;
        }

        OpenPage(pages[index]);
    }

    public void OpenPageByName(string name)
    {
        foreach (var page in pages)
        {
            if (page != null && page.name == name)
            {
                OpenPage(page);
                return;
            }
        }

        CloseAllPages();
        Debug.LogWarning("Page with name " + name + " not found!");
    }

    public void CloseAllPages()
    {
        foreach (var page in pages)
        {
            if (page == null) continue;
            page.gameObject.SetActive(false);
        }
    }

    public void OpenPanelByName(string name)
    {
        foreach (var panel in panels)
        {
            if (panel != null && panel.name == name)
            {
                panel.gameObject.SetActive(true);
            }
        }
    }

    public void ClosePanelByName(string name)
    {
        foreach (var panel in panels)
        {
            if (panel != null && panel.name == name)
            {
                panel.gameObject.SetActive(false);
            }
        }
    }

    public void CloseAllPanels()
    {
        foreach (var panel in panels)
        {
            if (panel == null) continue;
            panel.gameObject.SetActive(false);
        }
    }

    public void CloseAllPagesDelayed(float seconds)
    {
        StartCoroutine(WaitAndCloseAllPages(seconds));
    }

    IEnumerator WaitAndCloseAllPages(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        CloseAllPages();
    }
}