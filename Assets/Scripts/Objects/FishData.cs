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
    public float fishSpeed = 100f; // 100 is gemiddeld, max 170
    public float fishWeight = 1f; // 1 is gemiddeld, hoe hoger hoe makkelijker te vangen
    public float wildness = 2f; // 2 is gemiddeld, hoe lager hoe wilder de vis
    public int fishSize; // 80 is normaal

    public Effect[] effect;

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

[System.Serializable]
public class Effect
{
    public Effects effect;
    public float effectStrength;
    public float effectDuration = 30;

    public enum Effects
    {
        None,
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