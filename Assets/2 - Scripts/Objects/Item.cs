using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName; // Naam van het item
    public Sprite itemIcon; // Sprite voor het item
    [TextArea(10, 10)]
    public string description; // Beschrijving van het item
}