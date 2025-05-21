using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InteractableItem : MonoBehaviour
{
    [Header("General Settings")]
    public string interactionType = "Item";
    public float interactionRange = 2f;
    public float particleRange = 20f;
    public string interactionPrompt = "Press E to interact";

    [Header("Item Specific Settings")]
    public Item itemData;
    public InventoryUI inventoryUI;

    [Header("Effects")]
    public ParticleSystem pickupParticles;

    [Header("Popup Settings")]
    public GameObject popupPrefab;
    public float popupHeightOffset = 2f;
    public float popupFloatSpeed = 1f;
    public float popupFloatAmplitude = 0.2f;

    private Transform player;
    private bool isInteracted = false;
    private bool particlesShowing = false;


    private GameObject currentPopup;
    private Vector3 popupOriginalPosition;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (pickupParticles != null) pickupParticles.Stop();
    }

    private void Update()
    {
        if (isInteracted) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactionRange)
        {
            if (currentPopup == null) CreatePopup();
            AnimatePopup();

            if (Input.GetKeyDown(KeyCode.E)) PickupItem();
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

    private void PickupItem()
    {
        isInteracted = true;

        if (inventoryUI != null && itemData != null)
        {
            inventoryUI.AddItem(itemData.itemIcon);
        }

        Debug.Log($"You picked up {itemData.itemName}!");
        
        if (itemData != null)
        {
            if (itemData.itemName == "Fishing Rod")
            {
                var fishingManager = FindObjectOfType<FishingMinigameManager>();
                fishingManager?.SetFishingRodStatus(true);
            }
            else
            {
                var combatController = FindObjectOfType<CombatController>();
                if (itemData.itemName == "Sword")
                {
                    combatController?.SetSwordStatus(true);
                }
                else if (itemData.itemName == "Shield")
                {
                    combatController?.SetShieldStatus(true);
                }
            }
        }
        

        if (pickupParticles != null) pickupParticles.Stop();
        gameObject.SetActive(false);

        if (currentPopup != null) Destroy(currentPopup);
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
