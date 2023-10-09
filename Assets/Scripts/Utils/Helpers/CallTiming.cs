using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace Utils.Helpers
{
    public static class CallTiming
    {
        public static void DelayInvoke(this Action action, float delay, GameObject obj, bool timeScaleDependent = true)
        {
            Timing.RunCoroutine(DelayCallInvoke(delay).CancelWith(obj), timeScaleDependent ? Segment.Update : Segment.SlowUpdate);

            IEnumerator<float> DelayCallInvoke(float d)
            {
                yield return Timing.WaitForSeconds(d);
                action?.Invoke();
            }
        }
        
        public static void DelayInvoke(this Action action, float delay, string tag, bool timeScaleDependent = true)
        {
            Timing.RunCoroutine(DelayCallInvoke(delay), timeScaleDependent ? Segment.Update : Segment.SlowUpdate, tag);

            IEnumerator<float> DelayCallInvoke(float d)
            {
                yield return Timing.WaitForSeconds(d);
                action?.Invoke();
            }
        }
        
        public static void InvokeOnNextFrame(this Action action, GameObject obj, int frameCount = 1, bool timeScaleDependent = true)
        {
            Timing.RunCoroutine(CallInvokeNextFrame().CancelWith(obj), timeScaleDependent ? Segment.Update : Segment.SlowUpdate);

            IEnumerator<float> CallInvokeNextFrame()
            {
                for (int i = 0; i < frameCount; i++)
                {
                    yield return Timing.WaitForOneFrame;
                }
                action?.Invoke();
            }
        }
        
        public static void InvokeOnNextFrame(this Action action, string tag, int frameCount = 1, bool timeScaleDependent = true)
        {
            Timing.RunCoroutine(CallInvokeNextFrame(), timeScaleDependent ? Segment.Update : Segment.SlowUpdate, tag);

            IEnumerator<float> CallInvokeNextFrame()
            {
                for (int i = 0; i < frameCount; i++)
                {
                    yield return Timing.WaitForOneFrame;
                }
                action?.Invoke();
            }
        }
    }
}