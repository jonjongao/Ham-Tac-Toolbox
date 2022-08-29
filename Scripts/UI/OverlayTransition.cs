using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HamTac;
#if DOTWEEN_INSTALLED
using DG.Tweening;
#endif

[RequireComponent(typeof(CanvasGroup))]
public class OverlayTransition : MonoBehaviour
{
    protected CanvasGroup m_canvasGroup;
    protected Graphic m_graphic;
    [SerializeField]
    protected bool m_isPlaying;
    public bool isPlaying => m_isPlaying;

    protected virtual void Awake()
    {
        m_canvasGroup = GetComponent<CanvasGroup>();
        m_graphic = GetComponent<Graphic>();
        if (m_graphic is RawImage)
        {
            var instanceMat = Instantiate((m_graphic as RawImage).material);
            (m_graphic as RawImage).material = instanceMat;
            if ((m_graphic as RawImage).material.HasTexture("_MaskTex"))
            {
                (m_graphic as RawImage).material.SetTexture("_MaskTex", (m_graphic as RawImage).texture);
            }
        }
    }

    public virtual void InstantIn()
    {
        if (m_isPlaying) return;
        m_canvasGroup.Toggle(true);
        if (m_graphic is RawImage)
        {
            if ((m_graphic as RawImage).material.HasFloat("_Amount"))
                (m_graphic as RawImage).material.SetFloat("_Amount", 1f);
        }
        m_isPlaying = true;
    }

    public virtual void PlayIn()
    {
        m_canvasGroup.Toggle(true);
        m_isPlaying = true;
        if (m_graphic is RawImage)
        {
            if ((m_graphic as RawImage).material.HasFloat("_Amount"))
            {
#if DOTWEEN_INSTALLED
                (m_graphic as RawImage).material.SetFloat("_Amount", 0f);
                (m_graphic as RawImage).material.DOFloat(1f, "_Amount", 1f);
#else
                (m_graphic as RawImage).material.SetFloat("_Amount", 1f);
#endif
            }
            OnClipBegin();
        }
    }

    public virtual void PlayOut()
    {
        m_canvasGroup.Toggle(true);
        m_isPlaying = true;
        if (m_graphic is RawImage)
        {
            if ((m_graphic as RawImage).material.HasFloat("_Amount"))
            {
#if DOTWEEN_INSTALLED
                (m_graphic as RawImage).material.SetFloat("_Amount", 1f);
                (m_graphic as RawImage).material.DOFloat(0f, "_Amount", 1f).OnComplete(OnClipEnd);
#else
                (m_graphic as RawImage).material.SetFloat("_Amount", 0f);
                OnClipEnd();
#endif
            }
        }
    }

    public virtual void OnClipBegin()
    {
        m_isPlaying = true;
    }

    public virtual void OnClipEnd()
    {
        m_canvasGroup.Toggle(false);
        m_isPlaying = false;
    }
}
