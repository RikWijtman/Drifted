using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlinkManager : MonoBehaviour
{
    public RectTransform topPanel;
    public RectTransform bottomPanel;
    public float blinkSpeed = 0.5f; // Snelheid van het knipperen
    public int blinkCount = 1; // Hoe vaak het oog knippert
    private Vector2 topStartPos;
    private Vector2 bottomStartPos;
    private Vector2 topEndPos;
    private Vector2 bottomEndPos;

    private void Start()
    {
        // Startposities buiten het scherm
        topStartPos = topPanel.anchoredPosition;
        bottomStartPos = bottomPanel.anchoredPosition;

        // Middenposities (waar ze elkaar raken)
        topEndPos = new Vector2(topStartPos.x, 0);
        bottomEndPos = new Vector2(bottomStartPos.x, 0);
    }

    public void StartBlink()
    {
        StartCoroutine(BlinkSequence());
    }

    private IEnumerator BlinkSequence()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            // Ogen sluiten (bewegen naar het midden)
            yield return StartCoroutine(MovePanels(topPanel, topEndPos, bottomPanel, bottomEndPos));

            // Wacht even (optioneel)
            yield return new WaitForSeconds(0.1f);

            // Ogen openen (bewegen terug naar startpositie)
            yield return StartCoroutine(MovePanels(topPanel, topStartPos, bottomPanel, bottomStartPos));
        }

        // Verberg de panelen
        topPanel.gameObject.SetActive(false);
        bottomPanel.gameObject.SetActive(false);
    }

    private IEnumerator MovePanels(RectTransform top, Vector2 topTarget, RectTransform bottom, Vector2 bottomTarget)
    {
        float elapsedTime = 0;
        Vector2 topStart = top.anchoredPosition;
        Vector2 bottomStart = bottom.anchoredPosition;

        while (elapsedTime < blinkSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / blinkSpeed;
            top.anchoredPosition = Vector2.Lerp(topStart, topTarget, t);
            bottom.anchoredPosition = Vector2.Lerp(bottomStart, bottomTarget, t);
            yield return null;
        }

        // Zorg dat ze exact op de eindpositie staan
        top.anchoredPosition = topTarget;
        bottom.anchoredPosition = bottomTarget;
    }
}
