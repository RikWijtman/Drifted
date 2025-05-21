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

    private float timeSinceLastTick = 0f;
    private float healTimer = 10f;
    private float healCooldown = 0;
    private float healBlock = 10f;
    private bool canDash = true;
    private Vector2 lastLookDirection = Vector2.down;
    private PlayerBehaviour playerBehaviour;


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
        if (canMove)
        {
            rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);
        }

        if (running)
        {
            mana = Mathf.Clamp(mana - 0.01f, 0f, 100f);
        }

        timeSinceLastTick += Time.fixedDeltaTime;

        //regenerate health if mana is high enough
        if (mana > 0 && health < 100 && healTimer <= 0)
        {
            health = Mathf.Clamp(health + 0.03f, 0f, 100f);
            mana = Mathf.Clamp(mana - 0.02f, 0f, 100f);
        }
    }

    private void Dash()
    {
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
}

[System.Serializable]
public class Effects
{
    public Effect[] effects;
    public float effectStrength;
    public float effectLength;

    public enum Effect
    {
        Haste,          //Zorgt voor snellere beweging (3 levels)
        Strength,       //Zorgt voor meer schade (3 levels)
        Protection,     //Zorgt voor schade blokkering (4 levels)
        Slowness,       //Zorgt voor langzamere beweging (3 levels)
        Poison,         //Vergiftigd de speler (3 levels)
        Bleeding,       //Net zoals vergif maar speler doet ook minder schade (1 level) 
        Luck,           //Geeft de speler meer geluk ofzo?? (3 levels)
        SoulStalk,      //Vijanden in een bepaalde range van de speler krijgen schade per seconde (1 level)
        ManaProtection, //Zorgt voor een tijd dat de speler geen mana verliest (1 level)
        Immunity        //Geeft de speler immunity tegen poison (1 level)
    }
}