using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;
using MyBox;
namespace CubeHole
{
    public class DGAnimator : MonoBehaviour
    {
        public enum AnimationType { Move, Rotate, Scale, Color, Fade };

        [SerializeField] private AnimationType type;
        [SerializeField] private Vector3 toValue;
        [SerializeField] private Vector3 fromValue;
        [SerializeField] private Ease easeType;
        [SerializeField] private float animationSpeed;
        [SerializeField] private float delayBetweenLoop;
        [SerializeField] private float startDelay;
        [SerializeField] private int numberOfLoops = -1;
        [SerializeField] private LoopType loopType;
        [ConditionalField("type", false, AnimationType.Color)][SerializeField] private Color toColor;
        [ConditionalField("type", false, AnimationType.Color)][SerializeField] private Color fromColor;

        private void OnEnable()
        {
            Animate(this, type, toValue, fromValue, animationSpeed, easeType, numberOfLoops, loopType, startDelay, delayBetweenLoop, toColor, fromColor);
        }
        public static Tween Animate(MonoBehaviour target, AnimationType type, Vector3 toValue, Vector3 fromValue,
                                   float animationSpeed, Ease easeType, int numberOfLoops = 0, LoopType loopType = LoopType.Yoyo, float startDelay = 0, float delayBetweenLoop = 0,
                                   Color toColor = new Color(), Color fromColor = new Color())
        {
            target.TryGetComponent(out RectTransform rect);
            Tween animation = null;
            switch (type)
            {
                case AnimationType.Move:
                    if (rect != null)
                        animation = rect.DOAnchorPos(toValue, animationSpeed).From(fromValue);
                    else
                        animation = rect.DOMove(toValue, animationSpeed).From(fromValue);
                    break;
                case AnimationType.Rotate:
                    animation = target.transform.DORotate(toValue, animationSpeed).From(fromValue);
                    break;
                case AnimationType.Scale:
                    animation = target.transform.DOScale(toValue, animationSpeed).From(fromValue);
                    break;
                case AnimationType.Color:
                    if (target.TryGetComponent(out Image image))
                        animation = image.DOColor(toColor, animationSpeed).From(fromColor);
                    break;
                case AnimationType.Fade:
                    if (target.TryGetComponent(out CanvasGroup grp))
                        animation = grp.DOFade(toValue.x, animationSpeed).From(fromValue.x);
                    break;
            }
            if (numberOfLoops == -1)
                delayBetweenLoop = -1;
            if (animation != null)
                animation.SetEase(easeType).SetLoops(numberOfLoops, loopType).SetDelay(startDelay).SetId(type.ToString() + target.GetInstanceID()).SetAutoKill(false).Play().OnComplete(() =>
                {
                    if (delayBetweenLoop > 0)
                        HelperFunctions.DelayInvoke(target, () => { animation.Restart(); }, delayBetweenLoop);
                });

            return animation;
        }
        [ButtonMethod]
        public void ResetAnimation()
        {
            OnDisable();
            Animate(this, type, toValue, fromValue, animationSpeed, easeType, numberOfLoops, loopType, startDelay, delayBetweenLoop, toColor, fromColor);
        }
        private void OnDisable()
        {
            DOTween.Kill(type.ToString() + gameObject.GetInstanceID());
        }
    }
}