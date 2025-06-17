using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject inventoryPanel;
    public Image mainItemSlot1;
    public Image mainItemSlot2;
    public Transform inventoryGridParent; // Parent voor je inventory icons
    public GameObject inventoryIconPrefab; // Prefab met een Image component

    [Header("Toolbar icons (alleen visuals)")]
    public List<Sprite> itemIcons;

    [Header("Echte inventory items")]
    public List<Item> items = new List<Item>();
    public List<FishData> fishes = new List<FishData>();

    // Checks voor Oeck's items
    public FishingMinigameManager fishingManager;
    public bool hasHorn = false;

    private bool inventoryOpen = false;

    [Header("Inventory Grid Slots")]
    public List<Image> inventoryGridSlots; 

    [Header("Inventory Upgrade")]
    public int inventoryLevel = 1; // Start op level 1
    public int maxInventoryLevel = 5;
    public int slotsPerLevel = 2;
    public int baseSlots = 8;

    [Header("Inventory Frames")]
    public Image inventoryBackground; // Sleep hier je achtergrond-image in de inspector
    public Sprite[] inventoryFrames;  // Sleep hier je 5 frames in de juiste volgorde (level 1 t/m 5)

    [Header("Inventory Select Frames")]
    public Sprite selectionSprite; // Sleep hier sInventorySelect in de inspector
    public List<Image> selectionFrames; // Sleep hier lege Images over je inventoryGridSlots (zelfde volgorde)

    public FeedbackManager feedbackManager;

    private int selectedSlotIndex = -1;

    void Start()
    {
        UpdateInventoryGrid();
        UpdateInventoryFrame();
        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
        // Upgrade inventory met "X"
        if (Input.GetKeyDown(KeyCode.X))
        {
            UpgradeInventory();
        }
    }

    public void ToggleInventory()
    {
        inventoryOpen = !inventoryOpen;
        inventoryPanel.SetActive(inventoryOpen);
        if (inventoryOpen)
        {
            UpdateInventoryGrid();
        }
    }

    public void UpdateInventoryGrid()
    {
        int activeSlots = GetActiveSlotCount();

        List<Sprite> allIcons = new List<Sprite>();
        foreach (var item in items)
            allIcons.Add(item.itemIcon);
        foreach (var fish in fishes)
            allIcons.Add(fish.fishSprite);

        for (int i = 0; i < inventoryGridSlots.Count; i++)
        {
            if (i < activeSlots)
            {
                if (i < allIcons.Count)
                {
                    inventoryGridSlots[i].sprite = allIcons[i];
                    inventoryGridSlots[i].color = Color.white;
                }
                else
                {
                    inventoryGridSlots[i].sprite = null;
                    inventoryGridSlots[i].color = new Color(1,1,1,0.2f); // Leeg, maar zichtbaar
                }
            }
            else
            {
                inventoryGridSlots[i].sprite = null;
                inventoryGridSlots[i].color = new Color(1,1,1,0); // Helemaal onzichtbaar
            }
        }

        UpdateInventoryFrame();
    }

    private void UpdateInventoryFrame()
    {
        int frameIndex = Mathf.Clamp(inventoryLevel - 1, 0, inventoryFrames.Length - 1);
        if (inventoryBackground != null && inventoryFrames.Length > frameIndex)
            inventoryBackground.sprite = inventoryFrames[frameIndex];
    }

    public void SetHasHorn(bool value)
    {
        hasHorn = value;
    }

    public bool AddItem(Item item)
    {
        // Check of inventory vol is
        int totalItems = items.Count + fishes.Count;
        if (totalItems >= GetActiveSlotCount())
        {
            if (feedbackManager != null)
                feedbackManager.ShowFeedback("Inventory is full!", true);
            return false;
        }
        
        if (item.itemName == "Horn")
        {
            hasHorn = true;
        }

        // Voeg altijd toe aan de items-lijst
        items.Add(item);
        UpdateInventoryGrid();
        return true;
    }

    public bool AddFish(FishData fish)
    {
        int totalItems = items.Count + fishes.Count;
        if (totalItems >= GetActiveSlotCount())
        {
            if (feedbackManager != null)
                feedbackManager.ShowFeedback("Inventory is full!", true);
            return false;
        }

        fishes.Add(fish);
        UpdateInventoryGrid();
        return true;
    }

    public int GetActiveSlotCount()
    {
        return baseSlots + (inventoryLevel - 1) * slotsPerLevel;
    }

    public void UpgradeInventory()
    {
        if (inventoryLevel < maxInventoryLevel)
        {
            inventoryLevel++;
            UpdateInventoryGrid();
            UpdateInventoryFrame();
            if (feedbackManager != null)
                feedbackManager.ShowFeedback($"Inventory upgraded! Slots: {GetActiveSlotCount()}", true);
        }
        else
        {
            if (feedbackManager != null)
                feedbackManager.ShowFeedback("Inventory is already max level!", true);
        }
    }

    public void SelectSlot(int index)
    {
        // Deselecteer alles
        for (int i = 0; i < selectionFrames.Count; i++)
        {
            selectionFrames[i].enabled = false;
        }

        // Selecteer het nieuwe slot
        if (index >= 0 && index < selectionFrames.Count)
        {
            selectionFrames[index].sprite = selectionSprite;
            selectionFrames[index].enabled = true;
            selectedSlotIndex = index;
        }
        else
        {
            selectedSlotIndex = -1;
        }
    }
}