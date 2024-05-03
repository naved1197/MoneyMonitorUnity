using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace CubeHole.MM
{
    public class ScrollSnapController : MonoBehaviour
    {
        [SerializeField] private HorizontalScrollSnap horizontalScroll;
        [SerializeField] private float autoScrollDelay;

        private int CurrentPageIndex = -1;

        public void OnPageChanged(int pageIndex)
        {
            CurrentPageIndex = pageIndex;
            if (CurrentPageIndex >= 1)
                BackButtonHandler.SetBackAction(() => { horizontalScroll.ChangePage(CurrentPageIndex - 1); });
        }
    }
}
