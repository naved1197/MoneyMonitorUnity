using System;
using System.Collections;
using UnityEngine;
namespace CubeHole
{
    public class HelperFunctions
    {
        public static Coroutine DelayInvoke(MonoBehaviour mono, Action action, float delay, bool realtime = false)
        {
            if (mono.gameObject.activeInHierarchy)
                return mono.StartCoroutine(DelayFunction(action, delay, realtime));
            return null;
        }
        static IEnumerator DelayFunction(Action action, float delay, bool realtime = false)
        {
            if (realtime)
                yield return new WaitForSecondsRealtime(delay);
            else
                yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

    }
}
