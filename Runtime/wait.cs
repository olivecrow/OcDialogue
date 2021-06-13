using System;
using System.Collections;

#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
using UnityEngine;

public class wait : MonoBehaviour
{
    public static wait Instance => _instance;
    static wait _instance;
    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        _instance = new GameObject("wait instance").AddComponent<wait>();
        
        DontDestroyOnLoad(_instance);
    }

    public static Coroutine frame(int frame, Action e)
    {
        return _instance.StartCoroutine(WaitFrame(frame, e));
    }
    
#if UNITY_EDITOR
    public static EditorCoroutine editorFrame(int frame, Action e)
    {
        return EditorCoroutineUtility.StartCoroutineOwnerless(WaitFrame(frame, e));
    }
#endif
    
    public static Coroutine time(float sec, Action e, bool ignoreTimescale = false)
    {
        return _instance.StartCoroutine(WaitTime(sec, e, ignoreTimescale));
    }
    
#if UNITY_EDITOR
    public static EditorCoroutine editorTime(float time, Action e)
    {
        return EditorCoroutineUtility.StartCoroutineOwnerless(WaitTime(time, e, true));
    }
#endif

    /// <summary>
    /// predicate가 true 가 되면 실행
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="e"></param>
    public static Coroutine until(Func<bool> predicate, Action e)
    {
        return _instance.StartCoroutine(WaitUntil(predicate, e));
    }

    static IEnumerator WaitFrame(int frame, Action e)
    {
        for (int i = 0; i < frame; i++)
            yield return null;
        e.Invoke();
    }

    static IEnumerator WaitTime(float sec, Action e, bool ignoreTimeScale)
    {
        for (var f = 0f; f < sec; f += ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime)
            yield return null;
        e.Invoke();
    }

    static IEnumerator WaitUntil(Func<bool> predicate, Action e)
    {
        while (!predicate.Invoke())
            yield return null;
        
        e.Invoke();
    }
}