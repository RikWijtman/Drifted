using System.Collections;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    [Header("Global")]
    public AnimationClip attackAnim;               //animation for the attack
    public LayerMask enemyLayer;                   //layer the enemies are on

    [Header("Stats")]
    public float attackDamage = 5f;                //Damage dealt per attack
    public float attackRange = 1.7f;               //range that the enemy has to be in to take damage
    public float attackCooldown = 1f;              //cooldown to perform another attack
    public float blockCooldown = 3f;               //cooldown to block again
    public float maxBlockTime = 3f;                //max time the player can block

    public bool isBlock = false;                   //is the player blocking?

    private Animator animator;                     //player animator
    private PlayerController playerController;     //player controller script
    private bool hasSword = false;                 //sword status
    private bool hasShield = false;                //shield status
    private bool canAttack = true;                 //can the player attack
    private bool canBlock = true;                  //can the player block
    private Rigidbody2D rb;                        //player rb
    private bool isAttack = false;                 //trigger for the player attacking

    private void Start()
    {
        //get all necesary components
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        //all inputs
        if (Input.GetMouseButtonDown(0) && hasSword && canAttack)
        {
            Attack();
        }

        if (Input.GetMouseButtonDown(1) && hasShield && canBlock)
        {
            Block(true);
        }

        if (Input.GetMouseButtonUp(1) && hasShield && isBlock)
        {
            Block(false);
        }
    }

    private void Attack()
    {
        //perform animation
        animator.SetBool("Attack", true);
        //set bools, this way player can't perform any other moves till the end of the attack
        playerController.canMove = false;
        canAttack = false;
        rb.isKinematic = true;
        canBlock = false;

        //coroutines for the attack
        StartCoroutine(PerformAttack());
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator PerformAttack()
    {
        //wait until the attack is completed
        yield return new WaitForSeconds(0.3f);

        //stop animation
        animator.SetBool("Attack", false);
        //reset bools, so player can move again
        playerController.canMove = true;
        canBlock = true;
        //set rigidbody to kinematic, so no force affects the player
        rb.isKinematic = false;

        //perform the attack
        isAttack = true;
        yield return new WaitForSeconds(0.1f);
        isAttack = false;

        yield return null;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        //if the player performs an attack and hits an enemy
        if (collision.gameObject.CompareTag("Enemy") && isAttack)
        {
            //deal damage
            EnemyBasicScript enemy = collision.gameObject.GetComponent<EnemyBasicScript>();
            if (enemy.TakeDamage(attackDamage, true)) {
                playerController.mana -= 5;
            }
            isAttack = false;
        }
    }

    private IEnumerator AttackCooldown()
    {
        //can't attack till cooldown reaches zero
        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    private void Block(bool feedback)
    {
        //play animation
        animator.SetBool("Block",feedback);
        //set bools, so player can't perform any moves
        isBlock = feedback;
        canAttack = !feedback;
        playerController.canMove = !feedback;
        if (feedback)
        {
            canBlock = false;
        }
        //set rigidbody to kinematic, so no force affects the player
        rb.isKinematic = feedback;

        if (!feedback)
        {
            //als de knop wordt losgelaten, begint de cooldown voordat je weer kan blocken
            StartCoroutine(BlockCooldown());
        }
        else
        {
            //als je de knop vasthoud, begin de timer hoelang de speler kan blocken
            StartCoroutine(BlockTimer());
        }
    }

    private IEnumerator BlockTimer()
    {
        //cancel block after some time
        yield return new WaitForSeconds(maxBlockTime);

        Block(false);
    }

    private IEnumerator BlockCooldown()
    {
        //cooldown until player can block again
        yield return new WaitForSeconds(blockCooldown);

        canBlock = true;
    }

    //public functions to set if the player has a sword/shield
    public void SetSwordStatus(bool status)
    {
        hasSword = status;
    }

    public void SetShieldStatus(bool status)
    {
        hasShield = status;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
