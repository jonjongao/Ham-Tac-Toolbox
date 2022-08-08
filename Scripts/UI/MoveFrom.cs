using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if DOTWEEN_INSTALLED
using DG.Tweening;
#endif
using UnityEngine.UI;

public class MoveFrom : MonoBehaviour
{
    Vector2 m_end;
    [SerializeField]
    Vector2 m_from;
    Vector2 relativedFrom => m_end + m_from;
    [SerializeField]
    float m_duration = 0.5f;
    [SerializeField]
    bool m_stopOnAwake = true;

#if DOTWEEN_INSTALLED
    Sequence m_seq;
#endif
    RectTransform rectTransform => transform as RectTransform;

    private void Awake()
    {
        m_end = rectTransform.anchoredPosition;
        if (m_stopOnAwake)
            Stop();
    }

    public void Play()
    {
#if DOTWEEN_INSTALLED
        if (m_seq == null)
        {
            m_seq = DOTween.Sequence();
            m_seq.SetAutoKill(false);
            m_seq.Append(rectTransform.DOAnchorPos(relativedFrom, 0f));
            m_seq.Append(rectTransform.DOAnchorPos(m_end, m_duration).SetEase(Ease.OutBack));
        }
        else
            m_seq.Restart();
#endif
    }

    public void Stop()
    {
#if DOTWEEN_INSTALLED
        if (m_seq != null && m_seq.IsPlaying())
            m_seq.Complete();
#endif
        rectTransform.anchoredPosition = relativedFrom;
    }

    public void SetPlay(bool value)
    {
        if (value) Play();
        else Stop();
    }
}
