using DG.Tweening;
using HamTac;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TimeoutPopup : ConfirmPopup
{
    [SerializeField]
    float m_stayDuration = 1.5f;
    Animation m_animation;

    protected override void Awake()
    {
        base.Awake();
        m_applyDelayFrame = 10;
        m_animation = GetComponent<Animation>();
    }

    [Button("Test")]
    void TestPlay()
    {
        ConfirmPopup.Get("timeout").Initialize(new string[] { "Test" }, ConfirmPopup.DEFAULT_YES);
    }

    public override void Toggle(bool value)
    {
        if (value)
        {
            m_group.alpha = 0f;
            m_group.gameObject.SetActive(true);
        }
        else
        {
            m_group.alpha = 0f;
            m_group.gameObject.SetActive(false);
        }
    }

    float m_fadeInDuration = 0.25f;
    float m_fadeOutDuration = 0.25f;
    Sequence m_fadeInSeq;
    Sequence m_fadeOutSeq;
    [SerializeField]
    int m_times;

    public override async Task<int> Initialize(Func<int, Task<bool>> callback, string[] context, string[] options)
    {
        m_times++;
        m_animation.Play("PopupShow", PlayMode.StopAll);
        await Task.Yield();
        await Apply(context, options);

        //if (m_fadeInSeq == null)
        //{
        //    m_fadeInSeq = DOTween.Sequence();
        //    m_fadeInSeq.Append(m_group.DOFade(1f, m_fadeInDuration));
        //    m_fadeInSeq.Join(m_window.DOAnchorPosY(0f, m_fadeInDuration));
        //    m_fadeInSeq.InsertCallback(0, () =>
        //    {
        //        Toggle(true);
        //    });
        //    m_fadeInSeq.SetAutoKill(false);
        //    JDebug.W($"TimeoutPopup.1-1");
        //}
        //else
        //{
        //    m_fadeInSeq.Restart();
        //    JDebug.W($"TimeoutPopup.1-2");
        //}
        
        //await Extension.Async.Delay(m_stayDuration,()=>Input.GetMouseButtonDown(0));
        await Extension.Async.Delay(m_stayDuration);
        m_times--;
        if (m_times > 0)
            return -1;

        //if (m_fadeOutSeq == null)
        //{
        //    m_fadeOutSeq = DOTween.Sequence();
        //    m_fadeOutSeq.Append(m_group.DOFade(0f, m_fadeOutDuration));
        //    m_fadeOutSeq.Join(m_window.DOAnchorPosY(-50f, m_fadeOutDuration));
        //    m_fadeOutSeq.InsertCallback(m_fadeOutDuration, () => Toggle(false));
        //    m_fadeOutSeq.SetAutoKill(false);
        //    JDebug.W($"TimeoutPopup.2-1");
        //}
        //else
        //{
        //    m_fadeOutSeq.Restart();
        //    JDebug.W($"TimeoutPopup.2-2");
        //}
        m_animation.Play("PopupHide", PlayMode.StopAll);
        var result = -1;
        callback?.Invoke(result);
        m_selectedIndex = -1;
        return result;
    }
}
