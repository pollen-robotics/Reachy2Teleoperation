using UnityEngine;
using System.Collections.Generic;

public class OrderedPagesManager : PagesManager
{
    protected virtual void OnEnable()
    {
        OpenPageByIndex(0);
    }

    public void NextPage()
    {
        int nextPage = (currentPageIndex + 1) % pages.Count;
        OpenPageByIndex(nextPage);
    }

    public void PreviousPage()
    {
        int prevPage = (currentPageIndex - 1 + pages.Count) % pages.Count;
        OpenPageByIndex(prevPage);
    }
}
