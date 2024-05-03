using UnityEngine.EventSystems;
using UnityEngine;
using CubeHole;

public class ButtonEffector : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.instance.Play("buttonClick",0.3f,true);
        Vibration.Vibrate(10);
    }
}
