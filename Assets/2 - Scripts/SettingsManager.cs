using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public KeyCode settingsKey = KeyCode.Escape; // Standaard op Esc
    public GameObject settingsMenu; // Sleep je settingsmenu in de Inspector

    void Update()
    {
        if (Input.GetKeyDown(settingsKey))
        {
            if (settingsMenu != null)
                settingsMenu.SetActive(!settingsMenu.activeSelf);
        }
    }
}
