using UnityEngine;
using System.Collections.Generic;

public class InteractableItem : MonoBehaviour
{
    [Header("General Settings")]
    public string interactionType = "Item";
    public float interactionRange = 2f;
    public float particleRange = 20f;
    public string interactionPrompt = "Press E to interact";

    [Header("Item Specific Settings")]
    public Item itemData;
    public InventoryUIManager inventoryUI;

    [Header("Effects")]
    public ParticleSystem pickupParticles;

    [Header("Popup Settings")]
    public GameObject popupPrefab;
    public float popupHeightOffset = 2f;
    public float popupFloatSpeed = 1f;
    public float popupFloatAmplitude = 0.2f;

    private bool isInteracted = false;
    private bool particlesShowing = false;

    private Transform player;
    private GameObject currentPopup;
    private Vector3 popupOriginalPosition;

    public static List<InteractableItem> allItems = new List<InteractableItem>();

    private void Awake()
    {
        allItems.Add(this);
    }

    private void OnDestroy()
    {
        allItems.Remove(this);
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (pickupParticles != null) pickupParticles.Stop();
    }

    private void Update()
    {
        if (isInteracted) return;

        if (player == null) return;

        InteractableItem closest = null;
        float closestDist = float.MaxValue;

        // Zoek het dichtstbijzijnde item binnen interactie-range
        foreach (var item in allItems)
        {
            float dist = Vector3.Distance(player.position, item.transform.position);
            if (dist < item.interactionRange && dist < closestDist)
            {
                closest = item;
                closestDist = dist;
            }
        }

        // Alleen het dichtstbijzijnde item mag reageren
        /*if (closest == this)
        {
            // Toon prompt (optioneel)
            // Debug.Log(interactionPrompt);

            if (Input.GetKeyDown(KeyCode.E))
            {
                PickupItem();
            }
        }*/

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactionRange)
        {
            if (currentPopup == null) CreatePopup();
            AnimatePopup();
        }
        else
        {
            HidePopup();
        }

        if (distance <= particleRange && !particlesShowing)
        {
            particlesShowing = true;
            if (pickupParticles != null) pickupParticles.Play();
        }
        else if (distance > particleRange && particlesShowing)
        {
            particlesShowing = false;
            if (pickupParticles != null) pickupParticles.Stop();
        }
    }

    public void PickupItem()
    {

        bool pickedUp = false;

        if (inventoryUI != null && itemData != null)
        {
            pickedUp = inventoryUI.AddItem(itemData);

            if (pickedUp) {
                isInteracted = true;
                if (pickedUp && itemData.itemName == "Fishing Rod")
                {
                    var fishingManager = FindObjectOfType<FishingMinigameManager>();
                    fishingManager?.SetFishingRodStatus(true);
                    PlayerPrefs.SetInt("HasFishingRod", 1);
                }
                else if (pickedUp && itemData.itemName == "Horn")
                {
                    inventoryUI.SetHasHorn(true);
                }
                else if (pickedUp)
                {
                    var combatController = FindObjectOfType<CombatController>();
                    if (itemData.itemName == "Magical Sword")
                        combatController?.SetSwordStatus(true);
                    else if (itemData.itemName == "Magical Shield")
                        combatController?.SetShieldStatus(true);
                }
            }
        }

        // Alleen verwijderen als het echt is opgepakt!
        if (pickedUp)
        {
            if (pickupParticles != null) pickupParticles.Stop();
            if (currentPopup != null) Destroy(currentPopup);
            Destroy(gameObject);
        }
    }


    private void CreatePopup()
    {
        if (popupPrefab != null)
        {
            currentPopup = Instantiate(popupPrefab, transform.position + Vector3.up * popupHeightOffset, Quaternion.identity);
            popupOriginalPosition = currentPopup.transform.position;
        }
    }

    private void AnimatePopup()
    {
        if (currentPopup != null)
        {
            float floatOffset = Mathf.Sin(Time.time * popupFloatSpeed) * popupFloatAmplitude;
            currentPopup.transform.position = popupOriginalPosition + Vector3.up * floatOffset;
        }
    }

    private void HidePopup()
    {
        if (currentPopup != null)
        {
            Destroy(currentPopup);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, particleRange);
    }
}
