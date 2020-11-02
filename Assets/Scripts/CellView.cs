using UnityEngine;

namespace Emojigame
{
    public class CellView : MonoBehaviour
    {
        public Cell cell;
        public ParticleSystem DeathFX;
        public UnityEngine.UI.Image img;
        protected RectTransform _transform;
        protected Color enableColor;
        protected Color disableColor;

        public void Awake()
        {
            _transform = GetComponent<RectTransform>();
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
            if (cell.IsEmpty)
                return;
            GameEvents.Instance.CellClick(cell);
        }

        public void Update()
        {
            if (cell == null) return;

            if (cell.seleted)
                _transform.rotation = Quaternion.Euler(0, 0, 45f * Mathf.Sin(Time.time * 10f));
            else
                _transform.rotation = Quaternion.identity;
        }

        public void SetSprite(Sprite sprite)
        {
            img.sprite = sprite;
        }

        public void Show()
        {
            img.color = enableColor;
        }

        public void Hide()
        {
            img.color = disableColor;
            cell.cellType = Cell.NONE;
        }

        public void Explode()
        {
            Vector3 size = _transform.sizeDelta;
            ParticleSystem g = Instantiate(DeathFX, transform.parent);
            g.Play(true);
            g.transform.position = new Vector3(transform.position.x, transform.position.y, 1);
            g.transform.localScale = new Vector3(size.x * 0.05f, size.y * 0.05f, 1);
            Hide();
            Destroy(g, 5f);
        }

        public void SetPosition(Vector3 pos)
        {
            _transform.localPosition = pos;
        }

        public RectTransform RectTransofrm { get { return _transform; } }
    }
}

