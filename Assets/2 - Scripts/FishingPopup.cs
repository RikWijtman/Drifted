using UnityEngine;
using System.Collections;

public class FishingSpot : MonoBehaviour
{
    [Header("General Settings")]
    public float interactionRange = 2f; // Afstand voor interactie
    public string interactionPrompt = "Press E to start fishing"; // Prompt die getoond wordt
    public float cooldownTime = 120f; // Tijd in seconden voordat de visplek weer actief wordt

    [Header("Popup Settings")]
    public GameObject popupPrefab; // De prefab van de popup die moet verschijnen
    public float popupHeightOffset = 2f; // Hoogte waarop de pop-up zweeft boven het object
    public float popupFloatSpeed = 1f; // Snelheid van de op-en-neer beweging
    public float popupFloatAmplitude = 0.2f; // Hoe ver de pop-up op en neer gaat

    [Header("Cooldown")]
    public bool isOnCooldown = false; // Controleert of de visplek op cooldown staat

    private Transform player; // Referentie naar de speler
    private GameObject currentPopup; // Huidige instantie van de popup
    private Vector3 popupOriginalPosition; // Oorspronkelijke positie van de pop-up
    private FishingMinigameManager fishingGameManager; // Verwijzing naar FishingMinigameManager
    private Animator animator; // Verwijzing naar de animator van de popup


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        fishingGameManager = FindObjectOfType<FishingMinigameManager>();
        animator = GetComponent<Animator>(); // <-- Toegevoegd

        if (fishingGameManager == null)
        {
            Debug.LogError("FishingMinigameManager not found in the scene.");
        }
    }

    private void Update()
    {
        if (isOnCooldown) return; // Blokkeer interactie als de visplek op cooldown staat

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactionRange && fishingGameManager != null && fishingGameManager.hasFishingRod)
        {
            if (currentPopup == null)
            {
                CreatePopup();
            }

            AnimatePopup();
        }
        else
        {
            HidePopup();
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

            if (animator != null)
            {
                animator.Play("Popup");
            }
        }
    }

    private void HidePopup()
    {
        if (currentPopup != null)
        {
            Destroy(currentPopup);
        }
    }

    public void StartCooldown()
    {
        if (!isOnCooldown)
        {
            StartCoroutine(CooldownRoutine());
        }
    }

    private IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        if (animator != null) animator.SetBool("IsActive", false); // Zet uit
        HidePopup();
        Debug.Log("Fishing spot is now on cooldown for " + cooldownTime + " seconds.");

        yield return new WaitForSeconds(cooldownTime); // Wacht 2 minuten

        isOnCooldown = false;
        if (animator != null) animator.SetBool("IsActive", true); // Zet aan
        Debug.Log("Fishing spot is active again!");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isOnCooldown ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
