using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : GUIPage
{
    public GameObject prompt;

    public void Continue()
    {
        if (SettingSlider.AllUpdated())
            Back();
        else
            prompt.SetActive(true);
    }

    public void ApplyAll()
    {
        SettingSlider.ApplyAll();
    }

    public void ExitToMain()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }

    public override void Back()
    {
        SettingSlider.ResetAll();
        base.Back();
    }
}
