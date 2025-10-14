using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public List<Image> toolbarSlots; // Verwijzingen naar de vakjes in de UI
    public Sprite emptySlotSprite; // Sprite die een leeg vakje vertegenwoordigt

    private List<Sprite> itemIcons = new List<Sprite>(); // Huidige iconen in de inventory

    private void Start()
    {
        // Initialiseer de vakjes als leeg
        ClearInventoryUI();
    }

    // Voeg een item toe aan de inventory
    public void AddItem(Sprite itemSprite)
    {
        for (int i = 0; i < toolbarSlots.Count; i++)
        {
            if (itemIcons.Count <= i || itemIcons[i] == null)
            {
                // Update het vakje met het nieuwe item
                toolbarSlots[i].sprite = itemSprite;

                // Sla het item op
                if (itemIcons.Count <= i)
                {
                    itemIcons.Add(itemSprite);
                }
                else
                {
                    itemIcons[i] = itemSprite;
                }

                return;
            }
        }

        Debug.LogWarning("Inventory is full!");
    }

    // Verwijder een item van een bepaalde index
    public void RemoveItem(int index)
    {
        if (index < 0 || index >= toolbarSlots.Count)
        {
            Debug.LogWarning("Invalid inventory index!");
            return;
        }

        // Verwijder het item uit de lijst
        if (itemIcons.Count > index)
        {
            itemIcons[index] = null;
        }

        // Zet de sprite terug naar leeg
        toolbarSlots[index].sprite = emptySlotSprite;
    }

    // Leeg de inventory
    public void ClearInventoryUI()
    {
        foreach (Image slot in toolbarSlots)
        {
            slot.sprite = emptySlotSprite;
        }

        itemIcons.Clear();
    }
}
