using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class EnemyBasicScript : MonoBehaviour
{
    public Enemy enemyType;
    private Moves[] moveset; // Moveset of the enemy, fetched from the enemy object
    private float dashSpeed = 10f; // Speed of the dash
    private float dashDuration = 0.2f; // How long the dash lasts
    public float attackTimeOut = 0.5f; //short time after an attack before the enemy can attack again
    public float stunTime = 1.5f; // How long the enemy is stunned after hitting a player who's blocking
    public Transform healthFill; // Reference to the health bar fill transform
    public GameObject notification; // Notification prefab to notify the player when the enemy sees them

    public IdleStyle idleStyle;

    public enum IdleStyle
    {
        Idle,
        Roaming,
        Patrol
    }

    public Vector2[] patrollingPoints;                // patrolling points when on Patrol
    public float roamingRange;                        // Range where in the enemy roams

    private Transform player;                         // Reference to the player
    private Rigidbody2D rb;                           // Rigidbody for movement
    private Animator animator;                        // Animator for attacks
    private PlayerController playerScript;            // Player controller script
    private CombatController playerCombatScript;      // Player combat script
    private EnemyPathfinding enemyPathfinder;         // Pathfinding script
    private PlayerBehaviour playerBehaviour;          // Player behaviour tracking script
    private bool canAttack = true;                    // can the enemy attack?
    private float health;                             // health
    private float maxHealth;                          // max health
    private Dictionary<Moves, float> moveCooldowns = new();
    // use of a dictionary to store the cooldown of each attack/move
    private float speed;              // Speed of the enemy - fetched from enemy object
    private float detectionRange;     // Detection range of the enemy - fetched from enemy object
    private float closedRange;        // Range where the enemy is close enough to the player - fetched from enemy moveset
    private bool isBusy = false;      // Is the enemy busy?
    private bool stunned = false;     // Is the enemy Stunned?
    private bool isBlock = false;     // Is the enemy blocking?
    private bool performingDash = false; // Is the enemy currently performing a dash?
    private float blockAmount = 0;    // What percentage of damage does the enemy block? - fetched from enemy moveset
    private Queue<Vector2> currentPath = new();  // Current path the enemy is following or has to follow
    private Collider2D enemyCollider; // Enemy collider
    private Moves currentDashMove;    // Dash move
    private bool isDash = false;      // Is the enemy in a dash?
    private DamageFlash flasher;      // Script for the enemy flash
    private bool isAgro = false;      // Is the enemy in an aggresive state
    private float AOTAgroTimer = 0;
    private bool hasNotifiedPlayer = false; // Has the enemy notified the player
    private GameObject notifInstance; // Voeg dit veld toe bovenaan bij de andere velden

    private int currentPatrolIndex = 0;
    private float patrolPointThreshold = 0.1f; // Hoe dicht je bij een punt moet zijn om hem als "bereikt" te zien

    private Vector2? roamingTarget = null;
    private Vector2 startPosition;

    private float roamingMoveTimer = 0f;
    private float roamingPauseTimer = 0f;
    private bool isRoamingPaused = false;

    private Moves[] GetMoves()
    {
        return moveset
            .ToArray();
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        //get the player and it's scripts
        playerScript = FindObjectOfType<PlayerController>();
        if (playerScript == null)
        {
            Debug.LogError("PlayerController niet gevonden");
            return;
        }
        player = playerScript.transform;
        playerBehaviour = player.GetComponent<PlayerBehaviour>();
        playerCombatScript = player.GetComponent<CombatController>();
        enemyPathfinder = GetComponent<EnemyPathfinding>();
        enemyPathfinder.player = player;
        enemyPathfinder.enemyObject = enemyType; // <-- Voeg deze regel toe
        enemyCollider = GetComponent<Collider2D>();
        flasher = GetComponent<DamageFlash>();

        //check if player is found
        if (player == null)
        {
            Debug.LogError("Speler niet gevonden");
        }

        //get the rigidBody and animator of the enemy
        rb = GetComponent<Rigidbody2D>();

        //fetch and set stats and moves of the enemy 
        moveset = enemyType.moveset;
        detectionRange = enemyType.detectionRange;
        speed = enemyType.speed;
        health = enemyType.health;
        maxHealth = health;
        isAgro = enemyType.hostility == Enemy.Hostility.Hostile;

        //set all the move cooldowns to 0 
        foreach (Moves move in moveset)
        {
            moveCooldowns[move] = 0f;
        }

        //chance the move probabilities based on the players behaviour
        PlayerBehaviourCheck();

        // find the move with the lowest attack range
        Moves moveWithSmallestRange = moveset
            .Where(move => IsMoveReady(move))
            .OrderBy(move => move.moveRange)
            .FirstOrDefault();

        // if there's a valid move save this number as closedRange
        if (moveWithSmallestRange != null)
        {
            closedRange = moveWithSmallestRange.moveRange;
        }

        startPosition = transform.position; 
    }

    //function to check if a move is off cooldown
    private bool IsMoveReady(Moves move)
    {
        return Time.time >= moveCooldowns[move];
    }

    //function to check if a certain distance is in range
    public bool InRange(float dis, float minRange, float maxRange) =>
    dis > minRange && dis < maxRange;

    void FixedUpdate()
    {
        if (notifInstance != null)
        {
            notifInstance.transform.position = transform.position + Vector3.up;
        }
        
        if (AOTAgroTimer > 0)
        {
            AOTAgroTimer--;
        }
        
        //check if the enemy is currently performing a block, attack or etc.
        if (isBusy || stunned || performingDash)
        {
            return;
        }

        if (!isAgro)
        {
            if (idleStyle == IdleStyle.Patrol)
            {
                Patrol();
            }
            else if (idleStyle == IdleStyle.Roaming)
            {
                Roaming();
            }
            else if (idleStyle == IdleStyle.Idle)
            {
                rb.velocity = Vector2.zero;
                UpdateAnimator(Vector2.zero);
            }
            return;
        }
        //if the enemy can't see the player, go to the last seen position of the player                          *****
        else if (!enemyPathfinder.CanSeePlayer() && enemyPathfinder.lastSeenPosition != null)
        {
            //check if there's an available path to take
            if (currentPath.Count == 0)
            {
                //otherwise create a new path, from the enemy to the last seen position of the player
                var path = Pathfinder.FindPath(transform.position, enemyPathfinder.lastSeenPosition.Value);
                Debug.Log($"[PathRequest] Enemy pos: {transform.position}, LastSeen: {enemyPathfinder.lastSeenPosition.Value}");
                if (path != null && path.Count > 0)
                {
                    currentPath = new Queue<Vector2>(path);
                }
            }

            ChaseLastSeenPosition();
        }
        else if (player != null && enemyPathfinder.CanSeePlayer())
        {
            if (!hasNotifiedPlayer)
            {
                //if the enemy can see the player, notify the player
                if (notification != null)
                {
                    notifInstance = Instantiate(notification, transform.position + Vector3.up, Quaternion.identity);
                    Destroy(notifInstance, 0.5f);
                }
                hasNotifiedPlayer = true;
            }
            //get the distance to the player
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            //get direction and speed for animations
            Vector2 direction = (player.position - transform.position).normalized;

            UpdateAnimator(direction);

            //check if the enemy can attack
            if (canAttack)
            {
                //chance the move probabilities based on the players behaviour
                PlayerBehaviourCheck();

                //filter all moves that are in the right distance, and are out of cooldown
                Moves[] possibleMoves = moveset
                    .Where(move =>
                        // Alleen als vijand dichtbij genoeg is
                        InRange(distanceToPlayer, 0f, move.moveRange) &&
                        IsMoveReady(move))
                    .ToArray();

                //check if there's an available move
                if (possibleMoves.Length > 0)
                {
                    float totalChance = possibleMoves.Sum(move => move.probability); //total sum of probabilities
                    float randomValue = Random.Range(0, totalChance); //random value for the chance
                    float cumulativeChance = 0f; //float to add up the chances
                    Moves selectedMove = null;

                    //go through each moves until one is found
                    foreach (Moves move in possibleMoves)
                    {
                        cumulativeChance += move.probability;
                        if (randomValue <= cumulativeChance)
                        {
                            selectedMove = move;
                            break;
                        }
                    }

                    //check if there's a move selected
                    if (selectedMove != null)
                    {
                        //check for it's movetype to perform it's coresponding function
                        switch (selectedMove.moveType)
                        {
                            case Moves.AttackTypes.Melee:
                            case Moves.AttackTypes.Ranged:
                            case Moves.AttackTypes.Special:
                                StartCoroutine(PerformAttack(selectedMove));
                                break;
                            case Moves.AttackTypes.Dash:
                                StartCoroutine(Dash(selectedMove));
                                break;
                            case Moves.AttackTypes.Dodge:
                                Dodge(selectedMove);
                                break;
                            case Moves.AttackTypes.Block:
                                Block(selectedMove);
                                break;
                            // If you want to add a new attack type with a new function, add it in the switch function here !!
                        }
                    }
                }
                //if enemy can't attack chase player
                else if (InRange(distanceToPlayer, closedRange, detectionRange))
                {
                    ChasePlayer();
                }
                else
                {
                    rb.velocity = Vector2.zero;
                }
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
            animator.SetFloat("Speed", 0);
        }
    }

    private void PlayerBehaviourCheck()
    {
        var possibleMoves = GetMoves();

        foreach (Moves move in possibleMoves)
        {
            switch (move.moveType)
            {
                case Moves.AttackTypes.Block:
                    move.probability = move.baseProbability * playerBehaviour.attackTendency;
                    break;
                case Moves.AttackTypes.Dodge:
                    move.probability = move.baseProbability * playerBehaviour.attackTendency;
                    break;
                case Moves.AttackTypes.Dash:
                    move.probability = move.baseProbability * playerBehaviour.dodgeTendency;
                    break;
            }

            if (move.chargeTime > 0)
            {
                move.probability = move.baseProbability * playerBehaviour.blockTendency;
            }
        }
    }

    private IEnumerator PerformAttack(Moves move)
    {
        bool isChargedMove = move.chargeTime > 0;
        canAttack = false;
        isBusy = true;
        rb.velocity = Vector2.zero; // Stop de vijand

        rb.isKinematic = true;

        GameObject circleInstance = null;

        if (move.moveType == Moves.AttackTypes.Special)
        {
            if (move.specialType == Moves.SpecialTypes.Circle)
            {
                // Instantieer de cirkel ZONDER parent
                if (move.circleIndicatorPrefab != null)
                {
                    circleInstance = Instantiate(move.circleIndicatorPrefab, transform.position, Quaternion.identity);
                    float scale = move.specialRange * 2f;
                    circleInstance.transform.localScale = new Vector3(scale, scale, 1f);

                    
                }
            }
            else if (move.specialType == Moves.SpecialTypes.Straight)
            {
                if (move.circleIndicatorPrefab != null)
                {
                    // Richting naar speler
                    Vector2 dir = (player.position - transform.position).normalized;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                    // Instantieer lijn
                    circleInstance = Instantiate(move.circleIndicatorPrefab, transform.position, Quaternion.Euler(0, 0, angle));
                    // Zet de schaal: lengte = specialRange, dikte = 1f
                    circleInstance.transform.localScale = new Vector3(move.specialRange, 1f, 1f);

                    // Verplaats de lijn zodat het beginpunt bij de vijand ligt
                    circleInstance.transform.position = transform.position + (Vector3)(dir * (move.specialRange / 2f));
                }
            }
            var sr = circleInstance.GetComponent<SpriteRenderer>();
                    if (sr != null)
                        sr.color = move.indicatorColor;
        }

        if (isChargedMove)
        {
            if (animator != null)
            {
                animator.SetBool("Charging", true);
            }

            rb.velocity = Vector2.zero;

            yield return new WaitForSeconds(move.chargeTime); // Wacht de oplaadtijd af

            if (animator != null)
            {
                animator.SetBool("Charging", false);
            }
        }

        if (animator != null)
        {
            animator.SetBool(move.moveName, true);
        }

        if (move.moveAnim != null)
        {
            yield return new WaitForSeconds(move.moveAnim.length); // Korte animatie tijd voordat damage wordt gedaan
        }

        

        // Verwijder de cirkel na de aanval
        if (circleInstance != null)
        {
            Destroy(circleInstance);
        }

        rb.isKinematic = false;

        if (move.moveType == Moves.AttackTypes.Ranged)
        {
            if (move.projectile != null)
            {
                Collider2D playerCollider = player.GetComponent<Collider2D>();
                Vector2 targetPos = (Vector2)player.position;

                if (playerCollider != null)
                {
                    targetPos = playerCollider.bounds.center;
                }

                Vector2 direction = (targetPos - (Vector2)transform.position).normalized;

                GameObject projectileInstance = Instantiate(
                    move.projectile,
                    transform.position,
                    Quaternion.identity
                );

                ProjectileController projectileScript = projectileInstance.GetComponent<ProjectileController>();
                if (projectileScript != null)
                {
                    projectileScript.direction = direction;
                    projectileScript.damage = move.damage;
                    projectileScript.speed = move.projectileSpeed;
                    projectileScript.isCharged = isChargedMove;
                }
            }
            else
            {
                Debug.LogWarning($"Geen projectile prefab ingesteld voor move: {move.moveName}");
            }
        }
        else if (move.specialType == Moves.SpecialTypes.Circle && move.moveType == Moves.AttackTypes.Special)
        {
            DealDamage(move, move.specialRange, true);
        }
        else if (move.specialType == Moves.SpecialTypes.Straight && move.moveType == Moves.AttackTypes.Special)
        {
            // Check of speler in de lijn zit
            Vector2 dir = (player.position - transform.position).normalized;
            Vector2 toPlayer = (player.position - transform.position);
            float distance = toPlayer.magnitude;
            float angle = Vector2.Angle(dir, toPlayer.normalized);

            // Dikte van de lijn is 1f, dus check afstand tot lijn
            float perpendicular = Mathf.Abs(Vector2.Perpendicular(dir).normalized.x * toPlayer.x + Vector2.Perpendicular(dir).normalized.y * toPlayer.y);

            if (distance <= move.specialRange && perpendicular < 0.5f) // 0.5f is de helft van de dikte
            {
                DealDamage(move, move.specialRange, true);
            }
        }
        else if (isChargedMove || !playerCombatScript.isBlock)
        {
            DealDamage(move, move.moveRange, true);
        }
        else
        {
            playerBehaviour.RegisterBlock(true);
            Stun(stunTime);
        }

        if (animator != null)
        {
            animator.SetBool(move.moveName, false);
        }

        moveCooldowns[move] = Time.time + move.cooldown;
        isBusy = false;

        yield return new WaitForSeconds(attackTimeOut); // wait a short time until the enemy can't attack again

        canAttack = true;
    }

    private void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        Move(direction, speed);
        UpdateAnimator(direction);
    }

    private void ChaseLastSeenPosition()
    {
        if (currentPath.Count == 0) return;

        Vector2 nextPoint = currentPath.Peek();
        float dist = Vector2.Distance(transform.position, nextPoint);

        if (dist < 0.1f)
        {
            currentPath.Dequeue();
        }
        else
        {
            Vector2 dir = (nextPoint - (Vector2)transform.position).normalized;
            Move(dir, speed);
            UpdateAnimator(dir);
        }
    }

    private void Move(Vector2 dir, float spd)
    {
        rb.velocity = dir * spd;
    }

    public IEnumerator Dash(Moves dashMove)
    {
        if (player != null )
        {
            isBusy = true;
            rb.isKinematic = true;
            currentDashMove = dashMove;
            rb.velocity = Vector2.zero; 
            Vector2 dashDirection = (player.position - transform.position).normalized;

            GameObject dashIndicator = null;
            if (dashMove.dashIndicatorPrefab != null)
            {
                float angle = Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg;
                dashIndicator = Instantiate(dashMove.dashIndicatorPrefab, transform.position, Quaternion.Euler(0, 0, angle));
                dashIndicator.transform.localScale = new Vector3(dashMove.dashSpeed, 1f, 1f); // dashSpeed als lengte, pas aan indien gewenst
                dashIndicator.transform.position = transform.position + (Vector3)(dashDirection * (dashMove.dashSpeed / 2f));
                var sr = dashIndicator.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.color = dashMove.dashIndicatorColor;
            }

            if (animator != null)
            {
                animator.SetBool(dashMove.moveName, true);
            }

            if (dashMove.moveAnim != null)
            {
                yield return new WaitForSeconds(dashMove.moveAnim.length);
            }

            if (animator != null)
            {
                animator.SetBool(dashMove.moveName, false);
            }

            if (dashIndicator != null)
                Destroy(dashIndicator);

            rb.isKinematic = false;
            isDash = true;
            rb.velocity = dashDirection * dashSpeed;

            if (enemyCollider != null)
            {
                enemyCollider.isTrigger = true;
            }

            moveCooldowns[dashMove] = Time.time + dashMove.cooldown;

            // Zorg dat de vijand weer kan bewegen na de dash
            Invoke(nameof(ResetDash), dashDuration);
        }
    }

    public void Dodge(Moves dodgeMove)
    {
        if (player != null)
        {
            Vector2 dodgeDirection = (player.position - transform.position).normalized * -1;

            isBusy = true;
            rb.velocity = dodgeDirection * dashSpeed;

            if (enemyCollider != null)
            {
                enemyCollider.isTrigger = true;
            }

            moveCooldowns[dodgeMove] = Time.time + dodgeMove.cooldown;

            // Zorg dat de vijand weer kan bewegen na de dash
            Invoke(nameof(ResetDash), dashDuration);
        }
    }

    private void Block(Moves blockMove)
    {
        //start the block
        animator.SetBool("Block", true);
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        isBusy = true;

        //set block amount
        blockAmount = blockMove.blockPercentage;

        // Start Coroutine met correcte tijden
        StartCoroutine(BlockCooldown(blockMove));
    }

    private IEnumerator BlockCooldown(Moves blockMove)
    {
        //short period so that the player can see the block coming
        yield return new WaitForSeconds(0.2f);

        isBlock = true;
        // Active block time - this is between it's max, and half of that time
        var ranBlockTime = (blockMove.blockTime * 0.5f) + (blockMove.blockTime * (Random.Range(0, 5) / 10));
        yield return new WaitForSeconds(ranBlockTime - 0.2f);

        // Stop blocking
        isBlock = false;
        rb.isKinematic = false;
        isBusy = false;
        animator.SetBool("Block", false);

        //set on cooldown
        moveCooldowns[blockMove] = Time.time + blockMove.cooldown;

        // Eventueel: wacht op cooldown voordat opnieuw blokkeren mogelijk is
        yield return new WaitForSeconds(blockMove.cooldown);
    }


    private void ResetDash()
    {
        isBusy = false;
        isDash = false;
        rb.velocity = Vector2.zero;

        if (enemyCollider != null)
        {
            enemyCollider.isTrigger = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDash && rb.velocity.magnitude > 0.1f) // only during dash
        {
            DealDamage(currentDashMove, 0, false);
        }
    }

    private void Stun(float duration)
    {
        stunned = true;
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        //animator.SetBool("Stunned", true);

        Invoke(nameof(ResetStun), duration);
    }

    private void ResetStun()
    {
        stunned = false;
        rb.isKinematic = false;
        //animator.SetBool("Stunned", false);
    }

    public void UpdateHealthBar()
    {
        float currentHealthPercentage = health / maxHealth;
        healthFill.localScale = new Vector3(currentHealthPercentage, 0.1f, 1);

        healthFill.localPosition = new Vector3(currentHealthPercentage / 2f - 1 / 2f, 0, 0);
    }

    public void DealDamage(Moves move, float range, bool isMelee)
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= range || !isMelee)
        {
            playerScript.TakeDamage(move.damage);

            // Voeg effecten toe aan de speler
            if (move != null && move.attackEffect != null)
            {
                foreach (var effect in move.attackEffect)
                {
                    playerScript.ApplyEffect(
                        effect.effects,
                        effect.effectStrength,
                        effect.effectLength
                    );
                }
            }
        }
    }

    public bool TakeDamage(float amount, bool byPlayer)
    {
        if (byPlayer)
        {
            playerBehaviour.RegisterAttack(true);
        }

        if (enemyType.hostility == Enemy.Hostility.AggressiveOnAttack)
        {
            if (!isAgro)
            {
                isAgro = true;
            }
            AOTAgroTimer = 600;
        }

        if (!isBlock)
        {
            health -= amount;
            flasher.Flash();
            UpdateHealthBar();
            
            if (health <= 0)
            {
                Destroy(this.gameObject);
            }
            
            return false;
        }
        else
        {
            if (blockAmount < 100)
            {
                health -= amount * (Mathf.Abs(blockAmount - 100) / 100);
                flasher.Flash();
                UpdateHealthBar();

                if (health <= 0)
                {
                    Destroy(this.gameObject);
                }
            }
            
            return true;
        }
    }
    private void OnDrawGizmos()
    {
        if (enemyType != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyType.detectionRange);

            if (enemyType.moveset != null)
            {
                // Houd bij hoe vaak een bepaalde range al is getekend
                Dictionary<float, int> rangeCounts = new Dictionary<float, int>();
                float pixelOffset = 0.02f; // ongeveer 1 pixel in Unity units

                for (int i = 0; i < enemyType.moveset.Length; i++)
                {
                    Moves move = enemyType.moveset[i];
                    float range = move.moveRange;

                    // Bepaal de offset voor deze range
                    int count = 0;
                    if (rangeCounts.ContainsKey(range))
                    {
                        count = rangeCounts[range] + 1;
                        rangeCounts[range] = count;
                    }
                    else
                    {
                        rangeCounts[range] = 0;
                    }
                    float adjustedRange = range + (count * pixelOffset);

                    // Kies kleur op basis van moveType
                    switch (move.moveType)
                    {
                        case Moves.AttackTypes.Melee:
                            Gizmos.color = Color.yellow;
                            break;
                        case Moves.AttackTypes.Ranged:
                            Gizmos.color = Color.blue;
                            break;
                        case Moves.AttackTypes.Special:
                            Gizmos.color = Color.magenta;
                            break;
                        case Moves.AttackTypes.Dash:
                            Gizmos.color = Color.green;
                            break;
                        case Moves.AttackTypes.Dodge:
                            Gizmos.color = Color.cyan;
                            break;
                        case Moves.AttackTypes.Block:
                            Gizmos.color = Color.white;
                            break;
                        default:
                            Gizmos.color = Color.gray;
                            break;
                    }

                    Gizmos.DrawWireSphere(transform.position, adjustedRange);
                }
            }

            if (patrollingPoints != null && patrollingPoints.Length > 0)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
                foreach (var point in patrollingPoints)
                {
                    Gizmos.DrawSphere(point, 0.1f); 
                }
            }
        }

        if (idleStyle == IdleStyle.Roaming)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); 
            Vector2 center = (startPosition == Vector2.zero) ? (Vector2)transform.position : startPosition;
            Gizmos.DrawWireSphere(center, roamingRange);
        }
    }

    private void Patrol()
    {
        if (patrollingPoints == null || patrollingPoints.Length == 0)
            return;

        Vector2 target = patrollingPoints[currentPatrolIndex];
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        Move(direction, speed);
        UpdateAnimator(direction); // <-- voeg deze regel toe

        float distance = Vector2.Distance(transform.position, target);
        if (distance < patrolPointThreshold)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrollingPoints.Length;
        }
    }

    private void Roaming()
    {
        // Pauzeer om de 10 seconden voor 2 seconden
        if (isRoamingPaused)
        {
            roamingPauseTimer -= Time.fixedDeltaTime;
            rb.velocity = Vector2.zero;
            UpdateAnimator(Vector2.zero);
            if (roamingPauseTimer <= 0f)
            {
                isRoamingPaused = false;
                roamingMoveTimer = 0f;
            }
            return;
        }
        else
        {
            roamingMoveTimer += Time.fixedDeltaTime;
            if (roamingMoveTimer >= 7f)
            {
                isRoamingPaused = true;
                roamingPauseTimer = 2f;
                rb.velocity = Vector2.zero;
                UpdateAnimator(Vector2.zero);
                return;
            }
        }

        if (roamingTarget == null || Vector2.Distance(transform.position, roamingTarget.Value) < patrolPointThreshold)
        {
            // Kies een nieuw random punt binnen roamingRange
            Vector2 randomCircle = Random.insideUnitCircle * roamingRange;
            roamingTarget = startPosition + randomCircle;
        }

        Vector2 direction = ((Vector2)roamingTarget - (Vector2)transform.position).normalized;
        Move(direction, speed);
        UpdateAnimator(direction);
    }

    private void UpdateAnimator(Vector2 direction)
    {
        if (animator == null) return;

        // Richting bepalen
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            animator.SetFloat("Direction", direction.x > 0 ? 2 : 1); // 2 = rechts, 1 = links
        }
        else
        {
            animator.SetFloat("Direction", direction.y > 0 ? 3 : 0); // 3 = omhoog, 0 = omlaag
        }

        animator.SetFloat("Speed", direction.magnitude);
    }
}