using UnityEngine;


public class GameEvents : MonoBehaviour
{
    private static GameEvents s_instance;
    public static GameEvents Instance => s_instance ?? new GameObject("GameEvents").AddComponent<GameEvents>();

    private void Awake()
    {
        if (s_instance == null)
            s_instance = this;
        else
            Destroy(gameObject);
    }

    public event System.Action<Cell> OnCellClick;
    public event System.Action<int> OnCellSelect;
    public event System.Action<int> OnScore;
    public event System.Action OnReset;
    public event System.Action<int,int,bool> OnGameOver;

    public void GameOver(int score, int celltypes, bool perfect)
    {
        OnGameOver?.Invoke(score, celltypes, perfect);
    }

    public void ResetGame()
    {
        OnReset?.Invoke();
    }

    public void Score(int score)
    {
        OnScore?.Invoke(score);
    }

    public void CellClick(Cell id)
    {
        OnCellClick?.Invoke(id);
    }

    public void CellSelect(int selectedCnt)
    {
        OnCellSelect?.Invoke(selectedCnt);
    }
}