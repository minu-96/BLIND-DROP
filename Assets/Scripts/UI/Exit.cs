using UnityEngine;

public class Exit : MonoBehaviour
{
    public GameObject exitPanel;
    public bool onUI;
    public void Quit()
    {
        #if UNITY_EDITOR
        {
                UnityEditor.EditorApplication.isPlaying = false;
        }

        #else
        {
            Application.Quit();
        }

        #endif
    }

    public void OffExit()
    {
        Time.timeScale = 1f;
        exitPanel.SetActive(false);
        onUI = false;
    }
}
