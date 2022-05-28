using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using UnityEngine.UI;
using DG.Tweening;
public class VideoPlayerHandler : MonoBehaviour
{
    public static Dictionary<string, VideoPlayerHandler> all = new Dictionary<string, VideoPlayerHandler>();
    [SerializeField]
    string m_key;
    public string key => m_key;
    VideoPlayer m_player;
    RawImage m_rawImage;
    CanvasGroup m_canvasGroup;

    public event UnityAction OnComplete;

    private void Awake()
    {
        all.Add(m_key, this);
        m_player = GetComponent<VideoPlayer>();
        m_rawImage = GetComponent<RawImage>();
        m_canvasGroup = GetComponent<CanvasGroup>();
        m_player.loopPointReached += M_player_loopPointReached;
        m_player.prepareCompleted += M_player_prepareCompleted;
        m_player.started += M_player_started;
        m_canvasGroup.Toggle(false);
    }

    public void PlayClip(VideoClip clip, UnityAction onComplete = null)
    {
        m_player.clip = clip;
        m_player.Play();
        OnComplete = onComplete;
    }

    private void M_player_started(VideoPlayer source)
    {
        Debug.Log($"video started");
        m_canvasGroup.Toggle(true);
    }

    private void M_player_prepareCompleted(VideoPlayer source)
    {
        Debug.Log($"video prepare complete");

    }

    private void M_player_loopPointReached(VideoPlayer source)
    {
        Debug.Log($"video loop point reached");
        m_canvasGroup.DOFade(0f, 1f);
        TimeoutQueueController.OnTimeout(1f, OnVideoStop,"");
    }

    void OnVideoStop()
    {
        m_canvasGroup.DOKill();
        m_canvasGroup.Toggle(false);
        OnComplete?.Invoke();
    }
}
