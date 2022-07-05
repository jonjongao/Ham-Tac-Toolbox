using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
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

    Sequence m_seq;
    RectTransform rectTransform => transform as RectTransform;

    private void Awake()
    {
        m_end = rectTransform.anchoredPosition;
        if (m_stopOnAwake)
            Stop();
    }

    public void Play()
    {
        if (m_seq == null)
        {
            m_seq = DOTween.Sequence();
            m_seq.SetAutoKill(false);
            m_seq.Append(rectTransform.DOAnchorPos(relativedFrom, 0f));
            m_seq.Append(rectTransform.DOAnchorPos(m_end, m_duration).SetEase(Ease.OutBack));
        }
        else
            m_seq.Restart();
    }

    public void Stop()
    {
        if (m_seq != null && m_seq.IsPlaying())
            m_seq.Complete();
        rectTransform.anchoredPosition = relativedFrom;
    }

    public void SetPlay(bool value)
    {
        if (value) Play();
        else Stop();
    }
}
