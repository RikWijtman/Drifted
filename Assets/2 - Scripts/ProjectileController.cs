using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public Vector2 direction;
    private float lifetime = 4f;
    public bool isCharged = false;
    public Moves sourceMove; 

    private void Start()
    {
        Destroy(gameObject, lifetime); // Auto-destroy
    }

    private void Update()
    {
        transform.Translate(direction.normalized * sourceMove.projectileSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            CombatController combat = player.GetComponent<CombatController>();
            if (!combat.isBlock || isCharged)
            {
                player.TakeDamage(sourceMove.damage);

                if (sourceMove != null && sourceMove.attackEffect != null)
                {
                    foreach (var effect in sourceMove.attackEffect)
                    {
                        player.ApplyEffect(
                            effect.effects,
                            effect.effectStrength,
                            effect.effectLength
                        );
                    }
                }
            }
            Destroy(gameObject);
        }
    }
}
