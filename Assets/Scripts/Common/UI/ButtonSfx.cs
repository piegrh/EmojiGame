using UnityEngine;
using UnityEngine.EventSystems;


namespace Ulbe.UI
{
    public class ButtonSfx : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        public AudioClip HoverSfx;
        public AudioClip ClickSfx;
        public float volumeClick = 1f;
        public float volumeHover = 1f;

        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            Hover();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Click();
        }

        public void Hover()
        {
            if (HoverSfx == null)
                return;
            SoundMaster.Instance.PlayGlobalSound(HoverSfx, volumeHover);
        }

        public void Click()
        {
            if (ClickSfx == null)
                return;
            SoundMaster.Instance.PlayGlobalSound(ClickSfx, volumeClick);
        }
    }
}