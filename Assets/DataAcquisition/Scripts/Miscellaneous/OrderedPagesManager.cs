using UnityEngine;
using System.Collections.Generic;

public class OrderedPagesManager : MonoBehaviour
{
    [Header("Pages Setup")]
    public List<Transform> pages; // List of page Transforms

    protected int currentPageIndex = -1;

    protected virtual void Start()
    {
        ShowPage(0); // Optionally show the first page on start
    }

    public void ShowPage(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= pages.Count)
        {
            Debug.LogWarning("Page index out of range: " + pageIndex);
            return;
        }

        for (int i = 0; i < pages.Count; i++)
        {
            bool shouldBeActive = (i == pageIndex);
            if (pages[i] != null)
                pages[i].gameObject.SetActive(shouldBeActive);
        }

        currentPageIndex = pageIndex;
    }

    public void NextPage()
    {
        int nextPage = (currentPageIndex + 1) % pages.Count;
        ShowPage(nextPage);
    }

    public void PreviousPage()
    {
        int prevPage = (currentPageIndex - 1 + pages.Count) % pages.Count;
        ShowPage(prevPage);
    }
}
