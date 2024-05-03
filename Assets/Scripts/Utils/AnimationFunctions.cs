using System;
using UnityEngine;

public class AnimationFunctions : MonoBehaviour
{
    public Animator MyAnimator;
   public event Action OnAnimationComplete;
    public void AnimationComplete()
    {
        Vibration.Vibrate(50);
        OnAnimationComplete?.Invoke();
    }
}
