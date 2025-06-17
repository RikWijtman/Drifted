using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Combat/Enemy")]
public class Enemy : ScriptableObject
{
    public string enemyName; //Name of the enemy
    [TextArea(3, 5)] 
    public string description; //Description
    public Sprite enemySprite; //Sprite
    [Range(1, 5)]
    public int difficulty; //Difficulty scale (1-5)
    public float health; //Hit points
    [Range(0, 10)]
    public float speed; //Speed
    [Range(0, 20)]
    public float detectionRange; //Range to detect player
    public bool canCluster; //If the enemy can group with others? 
    public Hostility hostility; //Will it attack the player?
    public Moves[] moveset; //Moves the enemy can use

    public enum Hostility
    {
        Hostile,
        AggressiveOnAttack,
        Passive
    }
}

[System.Serializable]
public class Moves
{
    public string moveName; //name or ID of the move
    public AttackTypes moveType;
    public float damage; //damage the move will do (only for attack type moves)
    public float cooldown; //cooldown the move has before it can be used again
    public float probability; //number that determines the chance this move will be used or another
    public float baseProbability; //base prob
    public float moveRange; //distance the enemy has to be in, to be able to use the move
                            //(for buffing, the buff range. For healing the range it has be away from the player)
    public float chargeTime = 0; //how long does the charging take? Zero for a quick attack
    public Effects[] attackEffect; //Effect the attack will trigger on the player when hit
    public AnimationClip moveAnim; //Coresponding animation clip

    [Header("Block")]
    [Range(0, 100)]
    [HideInInspector] public float blockPercentage = 100; //% damage blocked 
    [HideInInspector] public float blockTime = 3f; //time the enemy will block

    [Header("Dash")]
    [HideInInspector] public float dashSpeed = 20f; //the speed of the dash
    [HideInInspector] public float dashDuration = 0.4f; //how long the dash takes
    [HideInInspector] public GameObject dashIndicatorPrefab; // Sleep hier je cirkel prefab in de inspector
    [HideInInspector] public Color dashIndicatorColor; // Sleep hier je cirkel prefab in de inspector

    [Header("Dodge")]
    [HideInInspector] public float dodgeSpeed = 20f; //the speed of the dodge
    [HideInInspector] public float dodgeDuration = 0.4f; //how long the dodge takes

    [Header("Healing")]
    [HideInInspector] public float healingAmount = 5; //amount of healing done per second

    [Header("Buffing")]
    [HideInInspector] public float speedBuff = 1.2f; //multiplier for movespeed (1 for no buff)
    [HideInInspector] public float attackBuff = 1.2f; //multiplier for attack (1 for no buff)
    [HideInInspector] public float cooldownBuff = 1.2f; //multiplier for cooldown (1 for no buff)

    [Header("Ranged")]
    [HideInInspector] public GameObject projectile;
    [HideInInspector] public float projectileSpeed;

    [Header("Special")]
    [HideInInspector] public SpecialTypes specialType; //type of special move   
    [HideInInspector] public float specialRange = 5f; //range of the special move, how far it can reach
    [HideInInspector] public GameObject circleIndicatorPrefab; // Sleep hier je cirkel prefab in de inspector
    [HideInInspector] public Color indicatorColor; // Sleep hier je cirkel prefab in de inspector

    /*different types of moves that can be inplemented
     * simple, quick attack
     * charged, attack that takes some time
     * block, enemy is able to predict your move and has a chance to block, this blocks a % of damage
     * Dash, Enemy is able to dash towards or away from the player
     * Healing, Enemy will run away from the player and heal itself, {healingAmount} per second
     * Buffing, When other enemies are in {moveRange} of this move, some of their stats will get buffed
     * Ranged, A ranged attack that will shoot a projectile towards the player
     */
    public enum AttackTypes
    {
        Melee,
        Block,
        Dash,
        Dodge,
        Healing,
        Buffing,
        Ranged,
        Special
    }
    //Plans: Camouflage, Trapping, Digging, Summoning, Lasers, Effect

    /*different types of special moves that can be implemented
     * Circle, A circle that will be placed on the ground, with the enemy in the middle
     * Straight, A straight line that will be placed on the ground, the enemy will move to the end and then explode
     *
    */
    public enum SpecialTypes
    {
        Circle,
        Straight
    }
}