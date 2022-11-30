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
    protected static KeyedObjects<OverlayTransition> m_instance = new KeyedObjects<OverlayTransition>();
    public static KeyedObjects<OverlayTransition> instance => m_instance;
    protected CanvasGroup m_canvasGroup;
    protected Graphic m_graphic;
    [SerializeField]
    string m_key;
    [SerializeField]
    protected bool m_isPlaying;
    public bool isPlaying => m_isPlaying;
    
    public static bool AnyTransitionIsPlaying
    {
        get
        {
            foreach(var i in m_instance)
            {
                if (i.Value.isPlaying) return true;
            }
            return false;
        }
    }

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
        if (m_instance == null)
            m_instance = new KeyedObjects<OverlayTransition>();
        m_instance.Add(m_key, this);
        InstantOut();
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

    public virtual void InstantOut()
    {
        m_canvasGroup.Toggle(false);
        if (m_graphic is RawImage)
        {
            if ((m_graphic as RawImage).material.HasFloat("_Amount"))
                (m_graphic as RawImage).material.SetFloat("_Amount", 0f);
        }
        m_isPlaying = false;
    }

    public virtual async Task PlayInAsync()
    {
        m_canvasGroup.Toggle(true);
        m_isPlaying = true;
        var tcs = new TaskCompletionSource<bool>();
        if (m_graphic is RawImage)
        {
            if ((m_graphic as RawImage).material.HasFloat("_Amount"))
            {
#if DOTWEEN_INSTALLED
                (m_graphic as RawImage).material.SetFloat("_Amount", 0f);
                (m_graphic as RawImage).material.DOFloat(1f, "_Amount", 1f).OnComplete(()=>tcs.SetResult(true));
                OnClipBegin();
                await tcs.Task;
#else
                (m_graphic as RawImage).material.SetFloat("_Amount", 1f);
                 OnClipBegin();
#endif
            }
        }
    }

    public virtual async Task PlayOutAsync()
    {
        m_canvasGroup.Toggle(true);
        m_isPlaying = true;
        var tcs = new TaskCompletionSource<bool>();
        if (m_graphic is RawImage)
        {
            if ((m_graphic as RawImage).material.HasFloat("_Amount"))
            {
#if DOTWEEN_INSTALLED
                (m_graphic as RawImage).material.SetFloat("_Amount", 1f);
                (m_graphic as RawImage).material.DOFloat(0f, "_Amount", 1f).OnComplete(()=>tcs.SetResult(true));

                await tcs.Task;
                OnClipEnd();
#else
                (m_graphic as RawImage).material.SetFloat("_Amount", 0f);
                OnClipEnd();
#endif
            }
        }
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
