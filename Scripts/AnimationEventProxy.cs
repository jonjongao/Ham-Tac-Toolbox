using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
public class AnimationEventProxy : MonoBehaviour
{
    public Dictionary<string, UnityAction> eventList = new Dictionary<string, UnityAction>();

    public bool HasEvent(string key)
    {
        return eventList.ContainsKey(key);
    }

    public void RegistEvent(string key, UnityAction callback)
    {
        if (eventList == null) eventList = new Dictionary<string, UnityAction>();
        if (eventList.ContainsKey(key))
        {
            eventList[key] += callback;
        }
        else
        {
            eventList.Add(key, callback);
        }
    }

    public void UnregistEvent(string key)
    {
        if (eventList == null) eventList = new Dictionary<string, UnityAction>();
        if (eventList.ContainsKey(key))
        {
            eventList[key] = null;
        }
    }

    public void ExecuteEvent(string key)
    {
        if (eventList == null) eventList = new Dictionary<string, UnityAction>();
        if (eventList.ContainsKey(key))
        {
            eventList[key]?.Invoke();
        }
        else
        {
            eventList.Add(key, null);
        }
    }

    public virtual void Disable()
    {
        if (HasEvent("Disable"))
            ExecuteEvent("Disable");
        gameObject.SetActive(false);
    }

    public virtual void Enable()
    {
        if (HasEvent("Enable"))
            ExecuteEvent("Enable");
        gameObject.SetActive(true);
    }
}
