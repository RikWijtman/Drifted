using UnityEngine;
using UnityEngine.UI;

public class InventorySlotClick : MonoBehaviour
{
    public int slotIndex;
    public InventoryUIManager inventoryUI;

    public void OnClick()
    {
        Debug.Log("Slot clicked: " + slotIndex);
        inventoryUI.SelectSlot(slotIndex);
    }
}