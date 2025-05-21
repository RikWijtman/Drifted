using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class FeedbackManager : MonoBehaviour
{
    public GameObject feedbackImage;
    public Button confirmButton;
    public TextMeshProUGUI feedbackText;
    public GameObject feedbackTextPrefab; // De prefab die gespawned wordt
    public Transform feedbackParent; // De UI-container waar de tekst in komt
    public float fadeDuration = 1.5f; // Hoe lang het vervagen duurt
    public float floatDistance = 30f; // Hoeveel pixels de tekst omhoog beweegt

    private void Awake()
    {
        // Zorg ervoor dat de feedback standaard onzichtbaar is
        if (feedbackImage != null)
        {
            feedbackImage.SetActive(false);
        }

        // Voeg een listener toe aan de knop
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(CloseFeedback);
        }
    }

    public void ShowFeedback(string message, bool isText)
    {
        if (isText)
        {
            GameObject newFeedback = Instantiate(feedbackTextPrefab, feedbackParent);
            TextMeshProUGUI tmp = newFeedback.GetComponent<TextMeshProUGUI>();
            tmp.text = message;

            StartCoroutine(FadeAndFloat(newFeedback, tmp));
        }
        else
        {
            // Toon de feedback en stel de tekst in
            feedbackText.text = message;
            feedbackImage.SetActive(true);
        }
    }


    private void CloseFeedback()
    {
        // Verberg de feedback bij het klikken op de knop
        feedbackImage.SetActive(false);
    }

    private IEnumerator FadeAndFloat(GameObject feedbackObj, TextMeshProUGUI text)
    {
        CanvasGroup canvasGroup = feedbackObj.GetComponent<CanvasGroup>();
        RectTransform rectTransform = feedbackObj.GetComponent<RectTransform>();
        Vector3 startPos = rectTransform.anchoredPosition;
        Vector3 targetPos = startPos + new Vector3(0, floatDistance, 0);
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            canvasGroup.alpha = alpha;
            rectTransform.anchoredPosition = Vector3.Lerp(startPos, targetPos, elapsedTime / fadeDuration);
            yield return null;
        }

        Destroy(feedbackObj); // Verwijder de instantie na de fade-out
    }
}
