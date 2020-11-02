
using UnityEngine;
using System.Threading.Tasks;

public class Cell : MonoBehaviour
{
    public const int NONE = -1;
    public int CellTypeID = NONE;
    public Vector2Int pos = Vector2Int.zero;
    public bool seleted = false;
    protected bool updated = false;
    public ParticleSystem DeathFX;
    protected UnityEngine.UI.Image img;

    public void Awake()
    {
        img = GetComponent<UnityEngine.UI.Image>();
    }

    public void OnMouseDown()
    {
        if (CellTypeID == NONE)
            return;
        GameEvents.Instance.CellClick(this);
    }

    protected void GSUpdated()
    {
        updated = true;
    }

    public void Update()
    {
        if (seleted)
            transform.rotation = Quaternion.Euler(0, 0, 45f * Mathf.Sin(Time.time * 10f));
    }

    public void SetSelected(bool value)
    {
        seleted = value;
        if (!value)
            transform.rotation = Quaternion.identity;
    }

    public void SetSprite(Sprite sprite)
    {
        img.sprite = sprite;
    }

    public void EnableCell()
    {
        Color temp = new Color(img.color.r, img.color.g, img.color.b, 1f);
        img.color = temp;
    }

    public void DisableCell()
    {
        Color temp = new Color(img.color.r, img.color.g, img.color.b, 0f);
        CellTypeID = NONE;
        img.color = temp;
    }

    public void Explode()
    {
        Vector3 size = GetComponent<RectTransform>().sizeDelta;
        ParticleSystem g = Instantiate(DeathFX, transform.parent);
        g.Play(true);
        g.transform.position = new Vector3(transform.position.x, transform.position.y, 1);
        g.gameObject.transform.localScale = new Vector3(size.x * 0.05f, size.y * 0.05f, 1);
        DisableCell();
        Destroy(g, 5f);
    }

    public bool IsEmpty { get { return CellTypeID == NONE; } }
}
