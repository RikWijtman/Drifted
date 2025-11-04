using UnityEngine;
using TMPro; // Vereist voor TextMeshPro
using UnityEngine.UI; // Vereist voor Image-component
using System.Collections;
using UnityEngine.EventSystems;

public class FishingMinigameManager : MonoBehaviour
{
    public GameObject fishingPanel; // Verwijzing naar het vis-minigame paneel
    public RectTransform targetCircle; // Het bewegende rondje
    public RectTransform target; // De vis
    public RectTransform hookIndicator; // De vishengel-indicator
    public PlayerController playerController; // Verwijzing naar de speler-controller
    public Transform playerTransform; // Transform van de speler
    public Transform[] fishingSpots;
    public float interactionRange = 2f; // Afstand waarbinnen je kunt vissen
    public Image ringProgress; // UI-element voor de visuele ring
    public RectTransform ringProgressTra; // De cirkel die de voortgang weergeeft
    public FeedbackManager feedbackManager; // Verwijzing naar de FeedbackManager
    public Image staminaBar; // Stamina bar UI

    public float targetMoveSpeed = 100f; // Snelheid van het bewegende rondje
    public float hookMoveSpeed = 200f; // Snelheid van de hengel
    public float successTime = 3f; // Tijd die je in het rondje moet blijven
    public float ProgressionMultiplier = 1f;
    public float ProgressionDrainMultiplier = 2f;

    private Vector2 panelBounds; // Grenzen van het paneel
    private float timeInTarget = 0f; // Tijd in het rondje
    private bool isFishing = false; // Is de minigame bezig?
    private Vector2 currentDirection; // Richting waarin de vis beweegt
    private float directionChangeTimer = 0f; // Timer voor richtingsverandering
    private float timeToChangeDirection = 2f; // Hoe vaak de richting verandert
    public bool hasFishingRod = false; // Controleert of de speler de hengel heeft
    private Vector2 fishSize = new Vector2(80, 80); //grootte van de vis is het spel
    private float stamina = 100f; // Stamina van de speler
    private float maxStamina = 100f;
    private float staminaMultiplier = 0.03f; // Snelheid waarmee stamina verliest

    public FishData currentFish; // De gevangen vis
    private FishingSpotConnection currentFishingSpotScript;
    public FishingSpotData currentFishingSpot;
    private FishingSpot fishingPopupScript;
    private string fishingFeedback;

    [Header("Fish Caught Notification UI")]
    public GameObject notificationPanel; // The notification panel
    public Image fishImage; // Picture of the caught fish
    public TMP_Text fishNameText; // Name of the caught fish
    public TMP_Text fishDescriptionText; // Description of the fish
    public TMP_Text fishRarityText; // Rarity of the fish
    public TMP_Text fishNutritionText; // Nutrition of the fish
    public TMP_Text fishEffectText; // Effect of the fish
    public Button confirmButton; // Button to confirm
    public Button eatButton; // Button to eat fish

    private void Start()
    {
        // Bereken de grenzen van het paneel
        panelBounds = new Vector2(fishingPanel.GetComponent<RectTransform>().rect.width / 2,
                                  fishingPanel.GetComponent<RectTransform>().rect.height / 2);



        // Verberg het vis-minigame paneel
        fishingPanel.SetActive(false);

        // Reset de ring-progressie
        ringProgress.fillAmount = 0f;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isFishing && hasFishingRod && !IsPointerOverUI())
        {
            if (CanStartFishing())
            {
                StartFishingMinigame();
            }
            else
            {
                feedbackManager.ShowFeedback(fishingFeedback, true);
            }
        }

        if (isFishing)
        {
            // Beweeg het rondje en de hengel als de minigame actief is
            MoveTarget();
            MoveHook();

            // // Zet de progressiering op dezelfde positie als de targetCircle
            // ringProgressTra.anchoredPosition = targetCircle.anchoredPosition;

            // Update de visuele ring en controleer op succes
            UpdateRingProgress();
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    private bool CanStartFishing()
    {
        // Doorloop alle visplekken om te kijken of de speler dichtbij is en in de juiste richting kijkt
        foreach (Transform fishingSpot in fishingSpots)
        {
            // Bereken de afstand tussen de speler en de fishing spot
            float distanceToFishingSpot = Vector3.Distance(playerTransform.position, fishingSpot.position);

            // Controleer of de speler dichtbij genoeg is (d.w.z. binnen de interactieafstand)
            if (distanceToFishingSpot <= interactionRange)
            {
                fishingPopupScript = fishingSpot.GetComponent<FishingSpot>();
                if (!fishingPopupScript.isOnCooldown)
                {
                    currentFishingSpotScript = fishingSpot.GetComponent<FishingSpotConnection>(); // Haal het FishingSpot-script op
                    currentFishingSpot = currentFishingSpotScript.fishingSpotData;
                    return true; // De speler kan vissen bij deze visplek
                }
                else
                {
                    fishingFeedback = "This fishing spot is currently inactive, try again later.";
                }
            }
            else
            {
                fishingFeedback = "You need to be near a fishing spot to fish.";
            }
        }

        return false; // De speler is niet in de juiste positie of kijkt niet naar een visplek

    }

    public void SetFishingRodStatus(bool status)
    {
        hasFishingRod = status;
    }

    public bool GetFishingRodStatus()
    {
        return hasFishingRod;
    }

    private void StartFishingMinigame()
    {
        //bepaal welke vis aan lijn
        currentFish = currentFishingSpot.GetRandomFish();
        Debug.Log("Momenteel aan lijn: " + currentFish.name);

        //zet grootte van de vis
        fishSize.x = currentFish.fishSize;
        fishSize.y = currentFish.fishSize;
        RectTransform rectTransform = targetCircle.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(fishSize.x, fishSize.y);

        RectTransform rectTransform2 = ringProgressTra.GetComponent<RectTransform>();
        rectTransform2.sizeDelta = new Vector2(fishSize.x, fishSize.y);

        //zet de rest van de stats van de vis
        targetMoveSpeed = currentFish.fishSpeed;
        timeToChangeDirection = currentFish.wildness;
        directionChangeTimer = currentFish.wildness;
        ProgressionMultiplier = currentFish.fishWeight;
        staminaMultiplier = currentFish.fishStaminaMultiplier;

        // Start de minigame
        isFishing = true;
        fishingPanel.SetActive(true);
        timeInTarget = 0f;

        // Schakel spelerbesturing uit
        playerController.enabled = false;

        // Plaats de target en hengel willekeurig
        PlaceRandomly(target);
        PlaceRandomly(hookIndicator);

        // // Zet de ring bovenop de viscirkel
        // ringProgressTra.anchoredPosition = targetCircle.anchoredPosition;
    }

    public void EndFishingMinigame(bool success)
    {
        isFishing = false;
        fishingPanel.SetActive(false);

        if (currentFishingSpotScript != null)
        {
            fishingPopupScript.StartCooldown();
        }

        if (success)
        {
            ShowFishNotification(currentFish);
        }
        else
        {
            playerController.enabled = true;
            feedbackManager.ShowFeedback("The fish got away...", true);
        }
    }

    private void ShowFishNotification(FishData fish)
    {
        notificationPanel.SetActive(true); // Toon de notificatie
        fishImage.sprite = fish.fishSprite; // Zet de afbeelding van de vis
        fishNameText.text = fish.fishName; // Zet de naam van de vis
        fishDescriptionText.text = fish.description; // Zet de beschrijving van de vis

        // Toon nutrition info, alleen als waarde > 0
        string nutritionLines = "";
        if (fish.nutrition > 0)
            nutritionLines += $"Mana restoration: {fish.nutrition}\n";
        if (fish.regeneration > 0)
            nutritionLines += $"HP restoration: {fish.regeneration}\n";
        if (fish.shield > 0)
            nutritionLines += $"Shield restoration: {fish.shield}\n";
        fishNutritionText.text = nutritionLines.TrimEnd('\n');

        // Toon alle effecten (max 3) op aparte regels
        if (fish.effect != null && fish.effect.Length > 0)
        {
            string effectLines = "";
            int maxEffects = Mathf.Min(3, fish.effect.Length);
            for (int i = 0; i < maxEffects; i++)
            {
                var eff = fish.effect[i];
                effectLines += $"{eff.effects} (Strength: {eff.effectStrength}, Duration: {eff.effectLength}s)";
                if (i < maxEffects - 1) effectLines += "\n";
            }
            fishEffectText.text = effectLines;
        }
        else
        {
            fishEffectText.text = "Effect: None";
        }

        fishRarityText.text = fish.rarity.ToString();

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() => CloseFishNotification());

        eatButton.onClick.RemoveAllListeners();
        eatButton.onClick.AddListener(() => EatFish(fish));
    }


    private void CloseFishNotification()
    {
        playerController.enabled = true;
        notificationPanel.SetActive(false); 
    }

    public void EatFish(FishData fish)
    {
        if (fish.nutrition > 0)
            playerController.mana = Mathf.Clamp(playerController.mana + fish.nutrition, 0f, 100f);
        if (fish.regeneration > 0)
            playerController.health = Mathf.Clamp(playerController.health + fish.regeneration, 0f, 100f);
        if (fish.shield > 0)
            playerController.shield = Mathf.Clamp(playerController.shield + fish.shield, 0f, 100f);

        if (fish.effect != null)
        {
            foreach (var eff in fish.effect)
            {
                playerController.ApplyEffect(
                    (Effects.Effect)eff.effects, 
                    eff.effectStrength, 
                    eff.effectLength
                );
            }
        }
        
        CloseFishNotification();

        if (feedbackManager != null)
            feedbackManager.ShowFeedback("You ate the fish!", true);
    }

    private void MoveTarget()
    {
        // Laat de vis bewegen in de huidige richting
        Vector3 newPos = target.anchoredPosition + (Vector2)currentDirection * targetMoveSpeed * Time.deltaTime;

        // Controleer of de vis tegen een rand botst
        if (newPos.x <= -panelBounds.x + target.rect.width / 2 || newPos.x >= panelBounds.x - target.rect.width / 2)
        {
            currentDirection.x *= -1; // Keer horizontale richting om
            newPos.x = Mathf.Clamp(newPos.x, -panelBounds.x, panelBounds.x);
        }

        if (newPos.y <= -panelBounds.y + target.rect.height / 2 || newPos.y >= panelBounds.y - target.rect.height / 2)
        {
            currentDirection.y *= -1; // Keer verticale richting om
            newPos.y = Mathf.Clamp(newPos.y, -panelBounds.y, panelBounds.y);
        }

        // Verander de richting na verloop van tijd
        directionChangeTimer += Time.deltaTime;
        if (directionChangeTimer >= timeToChangeDirection)
        {
            directionChangeTimer = 0f;
            ChangeDirection();
        }

        // Beweeg de vis
        target.anchoredPosition = newPos;

        // Laat de vis naar zijn bewegingsrichting kijken (alleen als hij beweegt)
        if (currentDirection.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
            target.rotation = Quaternion.Euler(0, 0, angle - 90f); // -90f zodat de sprite met zijn "kop" naar voren wijst
            targetCircle.rotation = Quaternion.identity; // Zorgt dat de ring altijd recht blijft
        }
    }

    private void ChangeDirection()
    {
        // Kies een nieuwe willekeurige richting en normaliseer deze
        currentDirection = Random.insideUnitCircle.normalized;
        currentDirection = Vector2.Lerp(currentDirection, Random.insideUnitCircle.normalized, 0.5f).normalized;
    }

    private void MoveHook()
    {
        // Bestuur de hengel met W, A, S, D
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Zorg dat diagonale beweging niet sneller is
        if (input.sqrMagnitude > 1f)
        {
            input = input.normalized;
        }

        // Bereken daadwerkelijke verplaatsing deze frame (in UI units)
        Vector2 moveDelta = input * hookMoveSpeed * Time.deltaTime;
        float moveDistance = moveDelta.magnitude;

        // Drain stamina op basis van hoe hard de hengel écht beweegt
        // staminaMultiplier is de drain per (UI unit) beweging, pas aan naar wens
        if (moveDistance > 0.0001f)
        {
            float drain = staminaMultiplier * moveDistance;
            stamina -= drain;
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);
            staminaBar.fillAmount = stamina / maxStamina;

            if (stamina <= 0f)
            {
                EndFishingMinigame(false);
                return;
            }
        }

        // Bereken nieuwe positie
        Vector3 newPos = hookIndicator.anchoredPosition + (Vector2)input * hookMoveSpeed * Time.deltaTime;

        // Zorg ervoor dat het binnen de grenzen blijft
        newPos.x = Mathf.Clamp(newPos.x, -panelBounds.x + hookIndicator.rect.width / 2, panelBounds.x - hookIndicator.rect.width / 2);
        newPos.y = Mathf.Clamp(newPos.y, -panelBounds.y + hookIndicator.rect.height / 2, panelBounds.y - hookIndicator.rect.height / 2);

        // Pas de nieuwe positie toe
        hookIndicator.anchoredPosition = newPos;
    }


    private void UpdateRingProgress()
    {
        // Bereken de afstand tussen de hengel en het rondje
        float distance = Vector2.Distance(hookIndicator.anchoredPosition, target.anchoredPosition);

        // Controleer of de hengel binnen het rondje is
        if (distance <= targetCircle.sizeDelta.x / 2)
        {
            timeInTarget += Time.deltaTime * ProgressionMultiplier;
            ringProgress.fillAmount = timeInTarget / successTime;

            // Controleer of de ring volledig is
            if (ringProgress.fillAmount >= 1f)
            {
                EndFishingMinigame(true);
            }
        }
        else
        {
            // Reset de ring-progressie als de hengel buiten het rondje beweegt
            timeInTarget = Mathf.Max(0, timeInTarget - Time.deltaTime * ProgressionDrainMultiplier);
            ringProgress.fillAmount = timeInTarget / successTime;
        }
    }

    private void PlaceRandomly(RectTransform obj)
    {
        // Plaats het object willekeurig binnen het paneel
        float x = Random.Range(-panelBounds.x + targetCircle.rect.width / 2, panelBounds.x - targetCircle.rect.width / 2);
        float y = Random.Range(-panelBounds.y + targetCircle.rect.height / 2, panelBounds.y - targetCircle.rect.height / 2);
        obj.anchoredPosition = new Vector2(x, y);
    }
}
