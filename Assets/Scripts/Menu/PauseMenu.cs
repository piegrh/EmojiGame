using Ulbe;
using UnityEngine;

public class PauseMenu : GUIPage
{
    public GameObject prompt;
    public SettingSlider[] sliders;

    public void Awake()
    {
        sliders = GetComponentsInChildren<SettingSlider>();
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
        for (int i = 0; i < sliders.Length; i++)
            sliders[i].Apply();
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
        for (int i = 0; i < sliders.Length; i++)
            sliders[i].ResetValue();
    }

    public bool AllUpdated()
    {
        for(int i = 0;i < sliders.Length; i++)
            if (!sliders[i].updated)
                return false;
        return true;
    }
}
