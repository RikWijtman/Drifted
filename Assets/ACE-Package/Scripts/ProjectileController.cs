using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float speed;
    public float damage;
    public Vector2 direction;
    private float lifetime = 4f;
    public bool isCharged = false;

    private void Start()
    {
        Destroy(gameObject, lifetime); // Auto-destroy
    }

    private void Update()
    {
        transform.Translate(direction.normalized * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            CombatController combat = player.GetComponent<CombatController>();
            if (!combat.isBlock || isCharged)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}
