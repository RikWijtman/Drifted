using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class BookManager : MonoBehaviour
{
    private bool bookOpen = false;
    public GameObject bookPanel;
    public TextMeshProUGUI pageNrText;
    public Button nextPageButton;
    public Button previousPageButton;

    private int currentPage = 1;

    public void ToggleInventory()
    {
        bookOpen = !bookOpen;
        bookPanel.SetActive(bookOpen);
    }

    public void NextPage()
    {
        currentPage++;
        UpdatePageDisplay();
    }

    public void PreviousPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            UpdatePageDisplay();
        }
    }

    private void UpdatePageDisplay()
    {
        pageNrText.text = currentPage.ToString();
    }
}