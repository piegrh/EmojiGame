using Ulbe;
using UnityEngine;

namespace Emojigame
{
    public class RandomEmoji : MonoBehaviour
    {
        protected static Sprite[] sprites;

        public void Awake()
        {
            GetComponent<SpriteRenderer>().sprite = GetRandomEmoji();
        }

        public static Sprite GetRandomEmoji()
        {
            if (sprites == null)
                sprites = Resources.LoadAll<Sprite>("textures/Emoji");
            return Utils.GetRandomFromCollection(sprites);
        }
    }

}