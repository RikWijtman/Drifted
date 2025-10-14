using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class EffectBarUI : MonoBehaviour
{
    public PlayerController player; // Sleep je speler hier in de inspector
    public GameObject effectIconPrefab; // Sleep je prefab hier in de inspector

    // Koppel elk effect aan een sprite
    public Sprite hasteSprite, strengthSprite, protectionSprite, slownessSprite, poisonSprite, bleedingSprite, luckSprite, soulStalkSprite, manaProtectionSprite, immunitySprite, stunSprite;

    private void Update()
    {
        // Verwijder oude iconen
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        // Voeg voor elk actief effect een icoon toe
        foreach (var effect in player.currentEffects)
        {
            GameObject icon = Instantiate(effectIconPrefab, transform);
            var img = icon.GetComponent<Image>();
            img.sprite = GetSpriteForEffect(effect.effects);

            // Knipper als effectLength <= 5
            if (effect.effectLength <= 5f)
            {
                // Knipper elke 0.5 seconde
                float blink = Mathf.PingPong(Time.time * 2f, 1f);
                Color c = img.color;
                c.a = Mathf.Lerp(0.3f, 1f, blink); // tussen 30% en 100% zichtbaar
                img.color = c;
            }
            else
            {
                // Zorg dat alpha altijd 1 is als niet knipperend
                Color c = img.color;
                c.a = 1f;
                img.color = c;
            }
        }
    }

    private Sprite GetSpriteForEffect(Effects.Effect effect)
    {
        switch (effect)
        {
            case Effects.Effect.Haste: return hasteSprite;
            case Effects.Effect.Strength: return strengthSprite;
            case Effects.Effect.Protection: return protectionSprite;
            case Effects.Effect.Slowness: return slownessSprite;
            case Effects.Effect.Poison: return poisonSprite;
            case Effects.Effect.Bleeding: return bleedingSprite;
            case Effects.Effect.Luck: return luckSprite;
            case Effects.Effect.SoulStalk: return soulStalkSprite;
            case Effects.Effect.ManaProtection: return manaProtectionSprite;
            case Effects.Effect.Immunity: return immunitySprite;
            case Effects.Effect.Stun: return stunSprite;
            default: return null;
        }
    }
}