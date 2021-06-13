using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class SimpleEventTrigger : MonoBehaviour
{
    public bool useMultipleEvent;
    [HideIf("useMultipleEvent")]public UnityEvent e;
    [ShowIf("useMultipleEvent"), TableList]public EventKeyPair[] events;

    [HideIf("useMultipleEvent"), Button]
    public void Invoke()
    {
        e.Invoke();
    }
        
    public void InvokeByKey(string key)
    {
        var e = events.FirstOrDefault(x => x.key == key);
        if (e == null)
        {
            Debug.Log($"해당 Key값의 이벤트가 없음 | key : {key}");
            return;
        }
            
        e.e.Invoke();
    }
        
    public void InvokeByIndex(int index)
    {
        if (index > events.Length - 1)
        {
            Debug.Log($"유효하지 않은 인덱스 | index : {index}");
            return;
        }
            
        events[index].e.Invoke();
    }

    [Serializable]
    public class EventKeyPair
    {
        [TableColumnWidth(100, false)]public string key;
        [VerticalGroup("E")]public UnityEvent e;

        [VerticalGroup("E"), Button]
        void Invoke()
        {
            e.Invoke();
        }
    }
}