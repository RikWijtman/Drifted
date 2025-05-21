using UnityEngine;

public class CameraFlow : MonoBehaviour
{
    public Transform player; // Referentie naar de speler
    public float smoothSpeed = 0.2f; // De snelheid van de vloeiende beweging
    public Vector3 offset = new(0,0,-10); // Offset van de camera ten opzichte van de speler

    private void FixedUpdate()
    {
        // Gewenste positie van de camera
        Vector3 desiredPosition = player.position + offset;

        // Vloeiend bewegen van de huidige positie naar de gewenste positie
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Pas de nieuwe positie toe
        transform.position = smoothedPosition;
    }
}
