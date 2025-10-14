using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public float attackMod;
    public float dodgeMod;
    public float blockMod;

    public float totalAttacks;
    public float totalDodges;
    public float totalBlocks;

    public float dodgeTendency;
    public float blockTendency;
    public float attackTendency;

    private void Start()
    {
        totalAttacks = PlayerPrefs.GetInt("Attacks", 0);
        totalDodges = PlayerPrefs.GetInt("Dodges", 0);
        totalBlocks = PlayerPrefs.GetInt("Blocks", 0);

        float total = totalAttacks + totalDodges + totalBlocks;
        if (total > 0)
        {
            dodgeTendency = (totalDodges * dodgeMod) / total;
            blockTendency = (totalBlocks * blockMod) / total;
            attackTendency = (totalAttacks * attackMod) / total;
        }
    }

    public void RegisterBlock(bool wasSuccessful)
    {
        if (wasSuccessful) totalBlocks++;
        PlayerPrefs.SetInt("Blocks", PlayerPrefs.GetInt("Blocks", 0) + 1);
    }
    public void RegisterDodge(bool wasSuccessful)
    {
        if (wasSuccessful) totalDodges++;
        PlayerPrefs.SetInt("Dodges", PlayerPrefs.GetInt("Dodges", 0) + 1);
    }
    public void RegisterAttack(bool wasSuccessful)
    {
        if (wasSuccessful) totalAttacks++;
        PlayerPrefs.SetInt("Attacks", PlayerPrefs.GetInt("Attacks", 0) + 1);
    }
}