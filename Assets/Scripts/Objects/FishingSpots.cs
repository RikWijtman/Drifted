using UnityEngine;

[CreateAssetMenu(fileName = "New Fishing Spot", menuName = "Fishing/Fishing Spot")]
public class FishingSpotData : ScriptableObject
{
    [System.Serializable]
    public class FishChance
    {
        public FishData fish;  // De vis die gevangen kan worden
        [Range(0f, 100f)]
        public float catchChance; // De kans om deze vis te vangen
    }

    public FishChance[] fishChances; // Lijst met mogelijke vissen en hun kansen

    // Kies een vis op basis van de kansen
    public FishData GetRandomFish()
    {
        float randomValue = Random.Range(0f, 100f);
        float cumulativeChance = 0f;

        foreach (FishChance fishChance in fishChances)
        {
            cumulativeChance += fishChance.catchChance;
            if (randomValue <= cumulativeChance)
            {
                return fishChance.fish;
            }
        }

        return null; // Voor het geval er geen vis wordt gevonden (hoewel dit niet zou moeten gebeuren)
    }
}
