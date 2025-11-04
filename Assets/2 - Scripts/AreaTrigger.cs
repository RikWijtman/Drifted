using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AreaTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public string triggerID; 
    public DialogManager dialogManager;
    public string[] dialog;

    [Header("Extra Options")]
    public bool allowPlayerMovement = false; // Mag speler bewegen tijdens dialog?
    public bool autoAdvanceDialog = false;   // Moet dialog automatisch doorgaan?
    public float autoAdvanceTime = 3f;       // Tijd tot volgende dialog (indien autoAdvanceDialog)

    [Header("Event Settings")]
    public bool requireEvent = false;
    public string requiredEventKey; // Bijvoorbeeld "HasFishingRod"

    private bool hasTriggered = false;
    private Transform player;
    private PlayerController playerController;
    private Animator playerAnimator;

    private void Start()
    {
        // Check of deze trigger al geactiveerd is via PlayerPrefs
        if (!requireEvent && PlayerPrefs.GetInt("AreaTrigger_" + triggerID, 0) == 1)
        {
            hasTriggered = true;
        }
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController>();
        playerAnimator = player.GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (requireEvent && PlayerPrefs.GetInt(requiredEventKey, 0) == 1)
            {
                return;
            }
            TriggerDialogue();
        }
    }

    private void TriggerDialogue()
    {
        if (dialogManager != null && !hasTriggered)
        {
            dialogManager.TriggerDialog(
                dialog,
                allowPlayerMovement,
                autoAdvanceDialog,
                autoAdvanceTime
            );

            if (!requireEvent) {
                hasTriggered = true;
                PlayerPrefs.SetInt("AreaTrigger_" + triggerID, 1);
                PlayerPrefs.Save();
            }else{
                MovePlayerBack();
            }
        }
    }

    private void MovePlayerBack()
    {
        if (player == null || dialogManager == null || dialogManager.dialogBox == null) return;

        // Bepaal de richting op basis van de rotatie van de dialogbox
        float angle = dialogManager.dialogBox.transform.eulerAngles.z;
        Vector2 moveDir = Vector2.down; // default

        if (angle > 45 && angle <= 135)
            moveDir = Vector2.left;
        else if (angle > 135 && angle <= 225)
            moveDir = Vector2.up;
        else if (angle > 225 && angle <= 315)
            moveDir = Vector2.right;

        // Start de coroutine voor langzaam teruglopen
        StartCoroutine(MovePlayerBackCoroutine(moveDir, 1.5f, playerController.baseMoveSpeed));
    }

    private IEnumerator MovePlayerBackCoroutine(Vector2 moveDir, float distance, float speed = 2f)
    {
        if (playerController == null) yield break;

        playerController.canMove = false;

        // Zet loop-animatie aan
        if (playerAnimator != null)
        {
            int direction = 0; // Down
            if (moveDir == Vector2.up)
                direction = 1;
            else if (moveDir == Vector2.right)
                direction = 2;
            else if (moveDir == Vector2.left)
                direction = 3;

            playerAnimator.SetFloat("Speed", speed);
            playerAnimator.SetFloat("Direction", direction);
        }

        float moved = 0f;
        Vector2 start = player.position;

        while (moved < distance)
        {
            float step = speed * Time.deltaTime;
            player.position += (Vector3)(moveDir * step);
            moved = Vector2.Distance(start, player.position);
            yield return null;
        }

        // Zet loop-animatie uit
        if (playerAnimator != null)
            playerAnimator.SetBool("IsWalking", false);

        playerController.canMove = true;
    }
}

