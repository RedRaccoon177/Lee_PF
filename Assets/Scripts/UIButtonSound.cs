using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager soundManager = FindObjectOfType<SoundManager>();
        if (soundManager != null)
        {
            soundManager.PlayHoverSound(); // 마우스를 올릴 때 효과음 실행
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager soundManager = FindObjectOfType<SoundManager>();
        if (soundManager != null)
        {
            soundManager.PlayClickSound(); // 버튼 클릭할 때 효과음 실행
        }
    }
}
