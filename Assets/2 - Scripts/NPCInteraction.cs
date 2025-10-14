using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NPCInteraction : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcName;
    public float interactionRange = 6f;

    [Header("Dialogue Segments")]
    public DialogueSegment[] dialogueSegments;
    private int currentSegmentIndex = 0;

    [Header("UI Elements")]
    public GameObject dialogBox;
    public TMP_Text dialogText;
    public GameObject choiceBox;
    public Button[] choiceButtons;
    public TMP_Text[] choiceTexts;
    public float typingSpeed = 0.05f;

    [Header("Popup Settings")]
    public GameObject popupPrefab;
    public float popupHeightOffset = 2f;
    public float popupFloatSpeed = 1f;
    public float popupFloatAmplitude = 0.2f;

    private bool isDialogActive = false;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private int nextForcedSegment = -1;
    private GameObject currentPopup;
    private Vector3 popupOriginalPosition;
    private Transform player;
    private PlayerController playerController;


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController>();
    }

    private void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactionRange)
        {
            if (currentPopup == null) CreatePopup();
            AnimatePopup();

            if (Input.GetKeyDown(KeyCode.E) && (!choiceBox.activeSelf || isDialogActive))
            {
                HandleInteraction();
            }
        }
        else
        {
            HidePopup();
        }
    }

    private void HandleInteraction()
    {
        if (isTyping)
        {
            CompleteCurrentText();
        }
        else if (!isDialogActive)
        {
            StartDialog();
        }
        else
        {
            ShowNextDialogLine();
        }
    }

    public void StartDialog()
    {
        isDialogActive = true;
        dialogBox.SetActive(true);
        currentSegmentIndex = 0;

        if (playerController != null)
        {
            playerController.canMove = false; // Beweging uitschakelen
        }

        ShowDialogueSegment();
    }

    private void ShowNextDialogLine()
    {
        if (nextForcedSegment != -1)
        {
            currentSegmentIndex = nextForcedSegment;
            nextForcedSegment = -1; // Reset zodat hij niet oneindig blijft forceren
        }
        else
        {
            currentSegmentIndex++;
        }

        if (currentSegmentIndex < dialogueSegments.Length)
        {
            ShowDialogueSegment();
        }
        else
        {
            EndDialog();
        }
    }

    private void ShowDialogueSegment()
    {
        if (isTyping)
        {
            CompleteCurrentText();
            return; // Zorgt ervoor dat je niet twee keer klikt per frame
        }

        DialogueSegment segment = dialogueSegments[currentSegmentIndex];

        if (segment.isChoice)
        {
            ShowChoices(segment);
        }
        else
        {
            typingCoroutine = StartCoroutine(TypeText(segment.text));
        }
    }

    private void ShowChoices(DialogueSegment segment)
    {
        choiceBox.SetActive(true);
        dialogText.text = segment.text; 

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < segment.choices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceTexts[i].text = segment.choices[i];

                int nextIndex = segment.nextSegments[i];
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => SelectChoice(nextIndex));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void SelectChoice(int nextIndex)
    {
        choiceBox.SetActive(false);

        if (nextIndex < 0 || nextIndex >= dialogueSegments.Length)
        {
            Debug.LogError("Ongeldige segment index: " + nextIndex);
            EndDialog();
            return;
        }

        currentSegmentIndex = nextIndex;

        // Controleer of het segment een geforceerd vervolg heeft
        nextForcedSegment = dialogueSegments[currentSegmentIndex].nextSegmentAfterChoice;

        ShowDialogueSegment();
    }

    private IEnumerator TypeText(string line)
    {
        isTyping = true;
        dialogText.text = "";

        foreach (char letter in line.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void CompleteCurrentText()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogText.text = dialogueSegments[currentSegmentIndex].text;
        isTyping = false;
    }

    private void EndDialog()
    {
        isDialogActive = false;
        dialogBox.SetActive(false);

        if (playerController != null)
        {
            playerController.canMove = true; // Beweging weer inschakelen
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

    public void StartSpecificDialogue(int index)
    {
        if (index >= 0 && index < dialogueSegments.Length)
        {
            isDialogActive = true;
            dialogBox.SetActive(true);
            currentSegmentIndex = index;
            ShowDialogueSegment();
        }
    }
}

[System.Serializable]
public class DialogueSegment
{
    public bool isChoice; // Is dit een keuze moment?
    public string text; // De tekst die wordt getoond (voor dialogen of de vraag bij keuzes)

    public string[] choices; // Keuze opties (indien van toepassing)
    public int[] nextSegments; // Indexen van de volgende segmenten gebaseerd op keuze
    public int nextSegmentAfterChoice = -1; // Waarheen na een keuze-dialoog (-1 = standaard volgen)
}
