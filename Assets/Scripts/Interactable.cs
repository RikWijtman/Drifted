using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public float interactionRange = 3f;
    public string interactionPrompt = "Press E to interact";

    private Transform player;
    private static Interactable currentFocus; // Nieuw: houdt bij wie de focus heeft

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("No GameObject with tag 'Player' found for Interactable!");
    }

    private void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactionRange)
        {
            // Alleen focus als je de dichtstbijzijnde bent
            if (currentFocus == null || distance < Vector3.Distance(player.position, currentFocus.transform.position))
            {
                if (currentFocus != null && currentFocus != this)
                    currentFocus.HidePrompt();

                currentFocus = this;
                ShowPrompt();

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Interact();
                }
            }
        }
        else
        {
            if (currentFocus == this)
            {
                HidePrompt();
                currentFocus = null;
            }
        }
    }

    public abstract void Interact();

    protected virtual void ShowPrompt()
    {
        Debug.Log(interactionPrompt);
    }

    protected virtual void HidePrompt()
    {
        Debug.Log("Prompt hidden");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
