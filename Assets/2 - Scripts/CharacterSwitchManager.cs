using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSwitchManager : MonoBehaviour
{
    public List<Character> characters; // Sleep hier je character ScriptableObjects in de Inspector
    public SpriteRenderer playerSpriteRenderer; // Het SpriteRenderer van de speler
    public Animator playerAnimator; // Animator van de speler

    public Image transitionCircle; // UI Image van de witte cirkel (zet op canvas, type: Filled Radial 360)
    public float transitionSpeed = 2f;

    private int currentCharacterIndex = 0;
    private bool isTransitioning = false;
    private bool switchingIn = true;
    private float transitionValue = 0f;

    public KeyCode switchKey = KeyCode.Q; // Zet standaard op Q, maar makkelijk aan te passen in de Inspector

    void Start()
    {
        ApplyCharacter(currentCharacterIndex);
        if (transitionCircle != null)
        {
            transitionCircle.gameObject.SetActive(false);
            transitionCircle.fillAmount = 0f;
        }
    }

    void Update()
    {
        // Alleen switchen als er meer dan 1 character is
        if (!isTransitioning && characters.Count > 1 && Input.GetKeyDown(switchKey))
        {
            StartCoroutine(DoCharacterSwitch());
        }
    }

    IEnumerator DoCharacterSwitch()
    {
        isTransitioning = true;
        transitionCircle.gameObject.SetActive(true);
        transitionCircle.rectTransform.localScale = Vector3.zero;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            float easedT = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3); // Cubic ease-out
            float scale = Mathf.Lerp(0f, 2f, easedT);
            transitionCircle.rectTransform.localScale = new Vector3(scale, scale, 1f);

            // Zet positie op speler
            Vector3 screenPos = Camera.main.WorldToScreenPoint(playerSpriteRenderer.transform.position);
            transitionCircle.rectTransform.position = screenPos;

            yield return null;
        }

        currentCharacterIndex = (currentCharacterIndex + 1) % characters.Count;
        ApplyCharacter(currentCharacterIndex);

        t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * transitionSpeed;
            float easedT = Mathf.Pow(Mathf.Clamp01(t), 3); // Cubic ease-in
            float scale = Mathf.Lerp(0f, 2f, easedT);
            transitionCircle.rectTransform.localScale = new Vector3(scale, scale, 1f);

            // Zet positie op speler
            Vector3 screenPos = Camera.main.WorldToScreenPoint(playerSpriteRenderer.transform.position);
            transitionCircle.rectTransform.position = screenPos;

            yield return null;
        }

        transitionCircle.gameObject.SetActive(false);
        isTransitioning = false;
    }

    void ApplyCharacter(int index)
    {
        if (characters == null || characters.Count == 0) return;
        Character character = characters[index];
        if (playerSpriteRenderer != null)
            playerSpriteRenderer.sprite = character.characterSprite;
        if (playerAnimator != null && character.animatorOverride != null)
            playerAnimator.runtimeAnimatorController = character.animatorOverride;
    }
}
