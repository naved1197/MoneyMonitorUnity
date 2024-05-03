using CubeHole.MM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchScreen : MonoBehaviour
{
    [SerializeField] private UIScreens from;
    [SerializeField] private UIScreens to;
    [SerializeField] private TransitionType type;
    [SerializeField] private bool isReversible;
    public void Switch()
    {
        ScreenSwitcher.Instance.SwitchScreen(from, to, type, isReversible);
    }
}
