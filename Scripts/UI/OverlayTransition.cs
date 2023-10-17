using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HamTac;
using System.Threading.Tasks;
#if DOTWEEN_INSTALLED
using DG.Tweening;
#endif

[RequireComponent(typeof(CanvasGroup))]
public class OverlayTransition : MonoBehaviour
{
    protected CanvasGroup m_canvasGroup;
    [SerializeField]
    protected bool m_isPlaying;
    public bool isPlaying => m_isPlaying;
    
    protected virtual void Awake()
    {
        m_canvasGroup = GetComponent<CanvasGroup>();
        InstantOut();
    }

    public virtual void InstantIn()
    {
        if (m_isPlaying) return;
        m_canvasGroup.Toggle(true);
        m_isPlaying = true;
    }

    public virtual void InstantOut()
    {
        m_canvasGroup.Toggle(false);
        m_isPlaying = false;
    }

    public virtual async Task PlayInAsync()
    {
        m_canvasGroup.Toggle(true);
        m_isPlaying = true;
        OnClipBegin();
        await Extension.Async.Delay(1f);
    }

    public virtual async Task PlayOutAsync()
    {
        m_canvasGroup.Toggle(true);
        m_isPlaying = true;
        OnClipEnd();
        await Extension.Async.Delay(1f);
    }

    public virtual void OnClipBegin()
    {
        JDebug.W($"OverlayTransition OnClipBegin");
        m_isPlaying = true;
    }

    public virtual void OnClipEnd()
    {
        JDebug.W($"OverlayTransition OnClipEnd");
        m_canvasGroup.Toggle(false);
        m_isPlaying = false;
    }
}
