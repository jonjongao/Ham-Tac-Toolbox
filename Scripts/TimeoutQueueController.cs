using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
public class TimeoutQueueController : MonoBehaviour
{
    static TimeoutQueueController m_current;
    public static TimeoutQueueController current
    {
        get
        {
            if (m_current == null)
            {
                var sceneInstance = FindObjectOfType<TimeoutQueueController>();
                if (sceneInstance == null)
                {
                    var obj = new GameObject("TimeoutQueueController", typeof(TimeoutQueueController));
                    sceneInstance = obj.GetComponent<TimeoutQueueController>();
                }
                m_current = sceneInstance;
                return sceneInstance;
            }
            return m_current;
        }
        set => m_current = value;
    }

    [System.Serializable]
    public class TimeoutQueue
    {
        public UnityAction callback;
        public float timeout;
        public bool skipable;
        public float timestamp;

        public Func<bool> canSkip;
        public TimeoutQueue(UnityAction e, float t, bool s)
        {
            this.callback = e;
            this.timeout = t;
            this.skipable = s;
            this.timestamp = Time.time;
        }

        public void FreeCache()
        {
            this.callback = null;
            this.canSkip = null;
        }

        public TimeoutQueue CanSkip(Func<bool> func)
        {
            this.canSkip = func;
            return this;
        }
    }

    [SerializeField]
    List<TimeoutQueue> m_timeoutQueue = new List<TimeoutQueue>();
    const float minGapDuration = 0.25f;

    public static TimeoutQueue OnTimeout(float duration, UnityAction callback)
    {
        return current.AddTimeout(duration, true, callback);
    }

    public static TimeoutQueue OnTimeout(float duration, bool skipable, UnityAction callback)
    {
        return current.AddTimeout(duration, skipable, callback);
    }

    TimeoutQueue AddTimeout(float duration, bool skipable, UnityAction callback)
    {
        var t = new TimeoutQueue(callback, duration, skipable);
        m_timeoutQueue.Add(t);
        return t;
    }

    private void Update()
    {
        if (m_timeoutQueue == null || m_timeoutQueue.Count == 0) return;

        for (int i = 0; i < m_timeoutQueue.Count; i++)
        {
            m_timeoutQueue[i].timeout -= Time.deltaTime;
            var min = Time.time > (m_timeoutQueue[i].timestamp + minGapDuration);
            var reached = m_timeoutQueue[i].timeout <= 0f;
            var skip = m_timeoutQueue[i].skipable && (m_timeoutQueue[i].canSkip == null ? false : m_timeoutQueue[i].canSkip());
            if (min && (reached || skip))
            {
                m_timeoutQueue[i].callback.Invoke();
                m_timeoutQueue[i].FreeCache();
                m_timeoutQueue.RemoveAt(i);
                break;
            }
        }
    }
}
