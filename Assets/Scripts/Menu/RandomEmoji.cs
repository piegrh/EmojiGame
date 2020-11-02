using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEmoji : MonoBehaviour
{
    protected static Sprite[] sprites;

    public void Awake()
    {
        GetComponent<SpriteRenderer>().sprite = GetRandomEmoji();
    }

    public static Sprite GetRandomEmoji()
    {
        if(sprites == null)
            sprites = Resources.LoadAll<Sprite>("textures/Emoji");
        return UlbeUtils.GetRandomFromCollection(sprites);
    }
}
