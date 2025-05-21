using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class HomeScreenManager : MonoBehaviour
{
    /*public void StartGame()
    {
        SceneManager.LoadScene("PeacefulWaters");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Closed"); 
    }*/
    public BlinkManager blinkManager; // Verwijzing naar het knipper-effect
    public Button startButton; // Verwijzing naar de startknop
    public float sceneLoadDelay = 0.7f; // Tijd voordat scene wordt geladen

    private void Start()
    {
       // startButton.onClick.AddListener(StartGame);
    }

    public void StartGame()
    {
        //startButton.interactable = false; // Voorkomt dubbel klikken
        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence()
    {
        blinkManager.StartBlink(); // Start de knipperanimatie

        yield return new WaitForSeconds(sceneLoadDelay); // Wacht totdat animatie klaar is

        SceneManager.LoadScene("PeacefulWaters"); // Vervang met de naam van je game scene
    }
}
