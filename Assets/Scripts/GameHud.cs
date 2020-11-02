using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHud : MonoBehaviour
{
    public Text scoreText;
    public Text selectionValue;
    int score = 0;
    int addValue = 0;

    // Start is called before the first frame update
    void Start()
    {
        GameEvents.Instance.OnScore += SetScore;
        GameEvents.Instance.OnCellSelect += CellSelection;
        GameEvents.Instance.OnReset += ResetGame;

        SetScore(0);
    }

    void SetScore(int s)
    {
        addValue += s;
        score += addValue;
        StartCoroutine(CountUp());
        CellSelection(0); // reset score value
    }

    protected IEnumerator CountUp()
    {
        while (addValue >= 0)
        {
            scoreText.text = (score - addValue).ToString();
            yield return new WaitForFixedUpdate();
            addValue -= GetSpeed(addValue);
        }
        addValue = 0;
    }

    protected int GetSpeed(int v)
    {
        int value = v / 40;
        return value > 0 ? value : v % 40;
    }

    protected void CellSelection(int cnt)
    {
        if(cnt == 0)
        {
            selectionValue.text = "";
            return;
        }
        selectionValue.text = UlbeColorString.ColorizeString(string.Format("^3{0} ^7emojis will get you ^3{1} ^7points!", cnt, GameScorer.GetScore(cnt)));
    }

    private void OnDestroy()
    {
        GameEvents.Instance.OnScore -= SetScore;
        GameEvents.Instance.OnReset -= ResetGame;
        GameEvents.Instance.OnCellSelect -= CellSelection;
    }

    private void ResetGame()
    {
        addValue = 0;
        score = 0;
        SetScore(0);
    }
}
