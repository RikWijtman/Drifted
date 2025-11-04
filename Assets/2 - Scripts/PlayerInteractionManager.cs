using UnityEngine;
using System.Collections.Generic;

public class PlayerInteractionManager : MonoBehaviour
{
    public float interactionRange = 2f;

    void Update()
    {
        InteractableItem closest = null;
        float closestDist = float.MaxValue;

        foreach (var item in InteractableItem.allItems)
        {
            if (item == null) continue;
            float dist = Vector3.Distance(transform.position, item.transform.position);
            if (dist < interactionRange && dist < closestDist)
            {
                closest = item;
                closestDist = dist;
            }
        }

        if (closest != null)
        {
            // Toon prompt (optioneel)
            // closest.ShowPrompt();

            if (Input.GetKeyDown(closest.interactionKey))
            {
                closest.Interact();
            }
        }
    }
}