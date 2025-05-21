using UnityEngine;

public class AreaTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public string triggerID; 
    public DialogManager dialogManager;
    public string[] dialog;

    private bool hasTriggered = false;

    private void Start()
    {
        // Check of deze trigger al geactiveerd is via PlayerPrefs
        if (PlayerPrefs.GetInt("AreaTrigger_" + triggerID, 0) == 1)
        {
            hasTriggered = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            TriggerDialogue();
        }
    }

    private void TriggerDialogue()
    {
        if (dialogManager != null)
        {
            dialogManager.TriggerDialog(dialog);
            hasTriggered = true;
            PlayerPrefs.SetInt("AreaTrigger_" + triggerID, 1); // Opslaan dat deze dialoog is getoond
            PlayerPrefs.Save();
        }
    }
}

