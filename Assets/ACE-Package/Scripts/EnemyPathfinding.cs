using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{
    public Transform player;
    public LayerMask obstacleMask;
    public float detectionRange = 10f;

    public Vector2? lastSeenPosition { get; private set; } = null;

    public bool CanSeePlayer()
    {
        //check of the player exists
        if (player == null)
            return false;

        //direction from this transform (the enemy) to the player
        Vector2 direction = player.position - transform.position;
        //distance van de transform naar de speler
        float distance = direction.magnitude;

        //if the player is to far from the enemy, return false
        if (distance > detectionRange)
            return false;

        //perform a raycast, store the collision in a variable
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, distance, obstacleMask);

        //check if there isn't anything between the enemy and the player
        if (hit.collider == null || hit.collider.transform == player)
        {
            //change the last seen position to the current position of the player
            lastSeenPosition = player.position;
            return true;
        }

        return false;
    }
}