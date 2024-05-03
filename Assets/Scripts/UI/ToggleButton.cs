using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    
    [SerializeField] private RectTransform Knob;
    [SerializeField] private Toggle toggle;
    [SerializeField] private Image bg;
    private float OnPos;
    private float OffPos;

    private void Awake()
    {
        OnPos = bg.rectTransform.rect.width - (Knob.rect.width / 2) - 10;
        OffPos = (Knob.rect.width / 2) + 10;
        ToggleGroup(GetComponent<Toggle>().isOn);
    }
    public void ToggleGroup(bool result)
    {
        toggle.interactable = false;
        if (result)
        {
            bg.DOFade(1, 0.4f).From(0).OnComplete(()=> { toggle.interactable = true; });
          
            Knob.DOAnchorPosX(OnPos, 0.4f).From(Vector2.right* OffPos);
        }
        else
        {
            bg.DOFade(0, 0.4f).From(1).OnComplete(() => { toggle.interactable = true; });

            Knob.DOAnchorPosX(OffPos, 0.4f).From(Vector2.right * OnPos);
        }
    }
}
