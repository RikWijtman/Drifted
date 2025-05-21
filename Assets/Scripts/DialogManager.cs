using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialogBox;
    public TMP_Text dialogText;

    private int currentSegmentIndex = 0;
    private bool isTyping = false;
    private bool isDialogActive = false;
    private Transform player;
    private PlayerController playerController;
    private Coroutine typingCoroutine;
    private string[] dialogs;
    private float typingSpeed = 0.05f;
    private int nextForcedSegment = -1;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isDialogActive)
        {
            HandleInteraction();
        }
    }

    public void TriggerDialog(string[] dialog)
    {
        dialogs = dialog;
        StartDialog();
    }

    private void HandleInteraction()
    {
        if (isTyping)
        {
            CompleteCurrentText();
        }
        else if (isDialogActive)
        {
            ShowNextDialogLine();
        }
    }

    private void CompleteCurrentText()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogText.text = dialogs[currentSegmentIndex];
        isTyping = false;
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

    private void ShowDialogueSegment()
    {
        if (isTyping)
        {
            CompleteCurrentText();
            return; // Zorgt ervoor dat je niet twee keer klikt per frame
        }

        typingCoroutine = StartCoroutine(TypeText(dialogs[currentSegmentIndex]));
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

        if (currentSegmentIndex < dialogs.Length)
        {
            ShowDialogueSegment();
        }
        else
        {
            EndDialog();
        }
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
}
