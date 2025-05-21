using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public float interactionRange = 3f; // Maximale afstand voor interactie
    public string interactionPrompt = "Press E to interact"; // Tekstprompt

    private Transform player; // Referentie naar de speler

    private void Start()
    {
        // Zoek de speler in de scene (zorg dat de speler de tag "Player" heeft)
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        // Controleer de afstand tussen speler en interactable
        if (Vector3.Distance(player.position, transform.position) <= interactionRange)
        {
            ShowPrompt();

            if (Input.GetKeyDown(KeyCode.E))
            {
                Interact(); // Roep de specifieke interactie aan
            }
        }
        else
        {
            HidePrompt();
        }
    }

    // Abstracte methode die elk interactief object moet implementeren
    public abstract void Interact();

    // Toon de prompt (kan worden overschreven voor unieke UI)
    protected virtual void ShowPrompt()
    {
        Debug.Log(interactionPrompt); // Vervang dit met je eigen prompt-logica (bijv. UI)
    }

    // Verberg de prompt (kan worden overschreven)
    protected virtual void HidePrompt()
    {
        Debug.Log("Prompt hidden"); // Pas dit aan voor je UI-systeem
    }

    private void OnDrawGizmosSelected()
    {
        // Teken een cirkel in de Editor om het interactiebereik te visualiseren
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
