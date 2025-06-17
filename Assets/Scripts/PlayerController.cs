using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float baseMoveSpeed = 1.5f;
    public float runMultiplier = 1.5f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 5f;
    public bool canMove = true; 

    [Header("Player status")]
    public float health = 100f;
    public GameObject healthBar;
    public float shield = 0f;
    public GameObject shieldBar;
    public float mana = 100f;
    public GameObject manaBar;
    public Effects[] currentEffects;

    [Header("Other")]
    public GameObject dialog;
    public GameObject deathPanel;

    private float currentSpeed;
    private Vector2 movement;
    private bool running;

    private Rigidbody2D rb;
    private Animator animator;

    private float timeSinceLastTickBleed = 0f;
    private float timeSinceLastTickPoison = 0f;
    private float healTimer = 10f;
    private float healCooldown = 0;
    private float healBlock = 10f;
    private bool canDash = true;
    private Vector2 lastLookDirection = Vector2.down;
    private PlayerBehaviour playerBehaviour;
    private float effectTickTimer = 0f;
    [HideInInspector] public bool isStunned = false;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = baseMoveSpeed;
        animator = GetComponent<Animator>();
        playerBehaviour = GetComponent<PlayerBehaviour>();
    }

    void Update()
    {
        if (!canMove) 
        {
            movement = Vector2.zero;
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.sqrMagnitude > 0.01f)
        {
            lastLookDirection = movement.normalized;
        }

        animator.SetFloat("Speed", movement.sqrMagnitude);

        if (movement.sqrMagnitude > 1)
        {
            movement.Normalize();
        }

        running = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && mana > 0;
        currentSpeed = running ? baseMoveSpeed * runMultiplier : baseMoveSpeed;

        if (movement.x > 0) animator.SetFloat("Direction", 2);
        else if (movement.x < 0) animator.SetFloat("Direction", 3);
        else if (movement.y > 0) animator.SetFloat("Direction", 1);
        else if (movement.y < 0) animator.SetFloat("Direction", 0);

        UpdateBars();


        healCooldown -= Time.deltaTime;

        if (healCooldown <= 0f)
        {
            healTimer--; 
            healCooldown = 1f; 
        }


        if (Input.GetKeyDown(KeyCode.Space) && canDash)
        {
            Dash();
        }
    }

    private void UpdateBars()
    {
        healthBar.GetComponent<Image>().fillAmount = health / 100;
        shieldBar.GetComponent<Image>().fillAmount = shield / 100;
        manaBar.GetComponent<Image>().fillAmount = mana / 100;
    }

    void FixedUpdate()
    {
        // --- EFFECTEN ---
        // STUN: blokkeer alles
        isStunned = currentEffects.Any(e => e.effects == Effects.Effect.Stun);
        if (isStunned)
        {
            rb.velocity = Vector2.zero;
            return; // speler kan niet bewegen of aanvallen
        }

        // HASTE & SLOWNESS: bepaal speed multiplier
        float speedMultiplier = 1f;
        var haste = currentEffects.FirstOrDefault(e => e.effects == Effects.Effect.Haste);
        var slow = currentEffects.FirstOrDefault(e => e.effects == Effects.Effect.Slowness);

        if (haste != null)
            speedMultiplier += 0.15f * haste.effectStrength; // 15%, 30%, 45%
        if (slow != null)
            speedMultiplier -= 0.15f * slow.effectStrength;  // -15%, -30%, -45%

        // Clamp zodat je niet negatief of te snel wordt
        speedMultiplier = Mathf.Clamp(speedMultiplier, 0.1f, 2f);

        // Pas snelheid aan
        float effectiveSpeed = currentSpeed * speedMultiplier;

        // --- BEWEGING ---
        if (canMove)
        {
            rb.MovePosition(rb.position + movement * effectiveSpeed * Time.fixedDeltaTime);
        }

        if (running && movement.sqrMagnitude > 0.01f)
        {
            mana = Mathf.Clamp(mana - 0.01f, 0f, 100f);
        }

        //regenerate health if mana is high enough
        if (mana > 0 && health < 100 && healTimer <= 0)
        {
            health = Mathf.Clamp(health + 0.03f, 0f, 100f);
            mana = Mathf.Clamp(mana - 0.02f, 0f, 100f);
        }

        // Effect timer logic
        effectTickTimer += Time.fixedDeltaTime;
        if (effectTickTimer >= 1f)
        {
            effectTickTimer = 0f;
            List<Effects> toRemove = new List<Effects>();
            foreach (var effect in currentEffects)
            {
                effect.effectLength -= 1f;
                if (effect.effectLength <= 0f)
                {
                    toRemove.Add(effect);
                }
            }
            // remove expired effects
            if (toRemove.Count > 0)
            {
                currentEffects = currentEffects.Except(toRemove).ToArray();
            }
        }

        //if player has poison effect, reduce mana
        if (
            currentEffects.Any(effect => effect.effects == Effects.Effect.Poison) &&
            !currentEffects.Any(effect => effect.effects == Effects.Effect.Immunity)
        )
        {
            timeSinceLastTickPoison += Time.fixedDeltaTime;

            if (timeSinceLastTickPoison >= 1f)
            {
                var poisonEffect = currentEffects.FirstOrDefault(effect => effect.effects == Effects.Effect.Poison);
                float strength = poisonEffect != null ? poisonEffect.effectStrength : 0f;
                mana = Mathf.Clamp(mana - (1f * strength), 0f, 100f);
                timeSinceLastTickPoison = 0f;
            }
        }

        //if player has bleeding effect, reduce health
        if (
            currentEffects.Any(effect => effect.effects == Effects.Effect.Bleeding) &&
            !currentEffects.Any(effect => effect.effects == Effects.Effect.Immunity)
        )
        {
            timeSinceLastTickBleed += Time.fixedDeltaTime;

            if (timeSinceLastTickBleed >= 1f)
            {
                var bleedingEffect = currentEffects.FirstOrDefault(effect => effect.effects == Effects.Effect.Bleeding);
                float strength = bleedingEffect != null ? bleedingEffect.effectStrength : 0f;
                health = Mathf.Clamp(health - (1f * strength), 0f, 100f);
                UpdateBars();
                timeSinceLastTickBleed = 0f;
            }
        }
    }

    private void Dash()
    {
        if (isStunned) return;

        canMove = false;

        Vector2 dashDirection = movement.sqrMagnitude > 0.01f ? movement : lastLookDirection;
        rb.velocity = dashDirection.normalized * dashSpeed;
        mana -= 2f;

        Invoke(nameof(ResetDash), dashDuration);
        StartCoroutine(DashCooldown());

        //check if the dodge should get registered
        float detectionRadius = 5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        bool enemyInRange = hits.Any(hit => hit.CompareTag("Enemy")); // Zorg dat vijanden deze tag hebben

        if (enemyInRange)
        {
            playerBehaviour.RegisterDodge(true);
        }
    }

    private IEnumerator DashCooldown()
    {
        canDash = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    private void ResetDash()
    {
        rb.velocity = Vector2.zero;
        canMove = true;
    }

    public void TakeDamage(float damageAmount)
    {
        health = Mathf.Clamp(health - damageAmount, 0f, 100f);
        healTimer = healBlock;
        UpdateBars();
        GetComponent<DamageFlash>()?.Flash();

        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        deathPanel.SetActive(true);
        canMove = false;
        //play animation
    }

    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    } 

    public void ApplyEffect(Effects.Effect effectType, float strength, float length)
    {
        // Kijk of het effect al actief is
        var existing = currentEffects.FirstOrDefault(e => e.effects == effectType);
        if (existing != null)
        {
            // Refresh de duur en sterkte als het effect al bestaat
            existing.effectLength = Mathf.Max(existing.effectLength, length);
            existing.effectStrength = Mathf.Max(existing.effectStrength, (int)strength);
        }
        else
        {
            // Voeg nieuw effect toe
            var newEffect = new Effects
            {
                effects = effectType,
                effectStrength = (int)strength,
                effectLength = length
            };
            var list = currentEffects.ToList();
            list.Add(newEffect);
            currentEffects = list.ToArray();
        }
    }
}