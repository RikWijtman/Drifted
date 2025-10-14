using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject helpPanel;
    public GameObject extrasPanel;

    public void TogglePanel(int panel)
    {
        if (panel == 1)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf); // Zet aan/uit
        }else if (panel == 2){
            helpPanel.SetActive(!settingsPanel.activeSelf); // Zet aan/uit
        }
        else
        {
            extrasPanel.SetActive(!settingsPanel.activeSelf); // Zet aan/uit
        }

    }
}
