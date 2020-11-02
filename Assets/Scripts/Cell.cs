
using UnityEngine;

namespace Emojigame
{
    public class Cell : MonoBehaviour
    {
        public const int NONE = -1;
        public int CellTypeID = NONE;
        public Vector2Int pos = Vector2Int.zero;
        public bool seleted = false;
        public ParticleSystem DeathFX;
        public UnityEngine.UI.Image img;
        protected RectTransform _transform;
        protected Color enableColor;
        protected Color disableColor;
        public Sprite sprite;

        public void Awake()
        {
            img = GetComponent<UnityEngine.UI.Image>();
            SetSpriteColors(new Color(img.color.r, img.color.g, img.color.b, 1f), new Color(img.color.r, img.color.g, img.color.b, 0f));
        }

        public void SetSpriteColors(Color enable, Color disable)
        {
            enableColor = enable;
            disableColor = disable;
        }

        public void OnMouseDown()
        {
            if (IsEmpty)
                return;
            GameEvents.Instance.CellClick(this);
        }

        public void Update()
        {
            if (seleted)
                _transform.rotation = Quaternion.Euler(0, 0, 45f * Mathf.Sin(Time.time * 10f)); // not here
        }

        public void SetSelected(bool value)
        {
            seleted = value;
            if (!value)
                _transform.rotation = Quaternion.identity;
        }

        public void SetSprite(Sprite sprite)
        {
            img.sprite = sprite;
        }

        public void EnableCell()
        {
            img.color = enableColor;
        }

        public void DisableCell()
        {
            CellTypeID = NONE;
            img.color = disableColor;
        }

        public void Explode()
        {
            Vector3 size = _transform.sizeDelta;
            ParticleSystem g = Instantiate(DeathFX, transform.parent);
            g.Play(true);
            g.transform.position = new Vector3(transform.position.x, transform.position.y, 1);
            g.transform.localScale = new Vector3(size.x * 0.05f, size.y * 0.05f, 1);
            DisableCell();
            Destroy(g, 5f);
        }

        public bool IsEmpty { get { return CellTypeID == NONE; } }
    }

}