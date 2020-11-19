using Emojigame.Effects;
using UnityEngine;

namespace Emojigame
{
    public class EmojiView : MonoBehaviour
    {
        public Emoji emoji;
        public ParticleSystem DeathFX;
        public UnityEngine.UI.Image img;
        protected RectTransform _transform;
        protected Color enableColor;
        protected Color disableColor;
        protected Roll roller;

        public void Awake()
        {
            _transform = GetComponent<RectTransform>();
            img = img ?? GetComponentInChildren<UnityEngine.UI.Image>();
            roller = roller ?? img.GetComponent<Roll>();
            roller.enabled = false;
            SetSpriteColors(new Color(img.color.r, img.color.g, img.color.b, 1f), new Color(img.color.r, img.color.g, img.color.b, 0f));
        }

        public void SetSpriteColors(Color enable, Color disable)
        {
            enableColor = enable;
            disableColor = disable;
        }

        public void OnMouseDown()
        {
            if (emoji.IsEmpty)
                return;
            GameEvents.Instance.CellClick(emoji);
        }

        public void Update()
        {
            if (emoji.seleted && !roller.enabled || (!emoji.seleted && roller.enabled))
                roller.enabled = !roller.enabled;
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
            emoji.emojiType = Emoji.NONE;
        }

        public void Explode()
        {
            Vector3 size = _transform.sizeDelta;
            ParticleSystem g = Pooler.Instance.Dequeue("explode", _transform.position).GetComponent<ParticleSystem>();
            g.Clear();
            g.Play(true);
            g.transform.position = new Vector3(transform.position.x, transform.position.y, 1);
            g.transform.localScale = new Vector3(size.x * 0.05f, size.y * 0.05f, 1);
            Hide();
        }

        public void SetPosition(Vector3 pos)
        {
            _transform.localPosition = pos;
        }

        public void SetPadding(Vector2 padding)
        {
            padding = new Vector2(Mathf.Abs(padding.x), Mathf.Abs(padding.y));
            img.GetComponent<RectTransform>().sizeDelta = _transform.sizeDelta - padding;
        }

        public void Move(Vector3 pos, float speed = 1000f)
        {
            SetPosition(Vector3.MoveTowards(RectTransofrm.localPosition, pos, speed * Time.fixedDeltaTime));
        }

        public RectTransform RectTransofrm { get { return _transform; } }
    }
}

