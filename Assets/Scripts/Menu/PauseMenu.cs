using Ulbe;
using UnityEngine;

public class PauseMenu : GUIPage
{
    public GameObject prompt;
    public UICvarSetting[] cvarSettings;

    public void Awake()
    {
        cvarSettings = GetComponentsInChildren<UICvarSetting>();
    }

    public void Continue()
    {
        if (AllUpdated())
            Back();
        else
            prompt.SetActive(true);
    }

    public void ApplyAll()
    {
        for (int i = 0; i < cvarSettings.Length; i++)
            cvarSettings[i].Apply();
    }

    public void ExitToMain()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }

    public override void Back()
    {
        ResetAll();
        base.Back();
    }

    public void ResetAll()
    {
        for (int i = 0; i < cvarSettings.Length; i++)
            cvarSettings[i].ResetValue();
    }

    public bool AllUpdated()
    {
        for(int i = 0;i < cvarSettings.Length; i++)
            if (!cvarSettings[i].updated)
                return false;
        return true;
    }
}
