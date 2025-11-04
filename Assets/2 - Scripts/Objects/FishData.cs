using UnityEngine;

[CreateAssetMenu(fileName = "New Fish", menuName = "Fishing/Fish")]
public class FishData : ScriptableObject
{
    public string fishName;        // Naam van de vis
    public string description;     // Beschrijving van de vis
    public Rarity rarity;          // Zeldzaamheid van de vis (kan meerdere waarden hebben)
    public Sprite fishSprite;      // Afbeelding van de vis

    public float nutrition = 0f; // Schaalt van 0-100 voor het aantal Mana dat je opleverd aan de vis
    public float regeneration = 0f; // Schaalt van 0-100 voor het aantal Hp dat je opleverd aan de vis
    public float shield = 0f; // Schaalt van 0-100 voor het aantal Shield dat je opleverd aan de vis
    public float fishSpeed = 70f; // 70 is gemiddeld, max 120
    public float fishWeight = 1f; // 1 is gemiddeld, hoe hoger hoe makkelijker te vangen
    public float wildness = 2f; // 2 is gemiddeld, hoe lager hoe wilder de vis
    public int fishSize = 100; // 100 is normaal
    public float fishStaminaMultiplier = 0.03f; // Hoe snel je stamina verliest van de vis

    public Effects[] effect;

    public enum Rarity
    {
        Common,    
        Regional,
        Rare,
        Exotic,
        Infected,
        Mutated,
    }
}
