using UnityEngine;

[System.Serializable]
public class Effects
{
    public Effect effects;
    [Range(1, 3)]
    public int effectStrength;
    public float effectLength;

    public enum Effect
    {
        Haste,          // Snellere beweging (3 levels)
        Strength,       // Meer schade (3 levels)
        Protection,     // Schade blokkering (3 levels)
        Slowness,       // Langzamere beweging (3 levels)
        Poison,         // Vergiftigd, vermindert mana langzaam (3 levels)
        Bleeding,       // Vermindert hp langzaam (1 level) 
        Luck,           // Grotere kans op crits (3 levels)
        SoulStalk,      // Vijanden in range krijgen schade per seconde (1 level)
        ManaProtection, // Tijdelijk geen mana verlies (1 level)
        Immunity,       // Immuniteit tegen poison (1 level)
        Stun,           // Kan niet bewegen of aanvallen (1 level) 
        Sleep           // Kan niet bewegen of aanvallen, maar wanneer je wordt aangevallen, word je wakker (1 level)
    }
}