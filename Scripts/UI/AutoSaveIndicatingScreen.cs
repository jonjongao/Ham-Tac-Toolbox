using HamTac;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoSaveIndicatingScreen : MonoBehaviour
{
    [SerializeField]
    Image m_backgroundImage;
    [SerializeField]
    RectTransform m_indicatorObject;

    private void Awake()
    {
    }

    private void Start()
    {
        AhnosEventDispatcher.On(GameEvent.OnSaveBegin, OnAutosaveBegin);
        AhnosEventDispatcher.On(GameEvent.OnBackgroundSaveBegin, OnBackgroundSaveBegin);
        AhnosEventDispatcher.On(GameEvent.OnSaveEnd, OnAutosaveEnd);
        m_backgroundImage.gameObject.SetActive(false);
        m_indicatorObject.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        AhnosEventDispatcher.Remove(GameEvent.OnSaveBegin, OnAutosaveBegin);
        AhnosEventDispatcher.Remove(GameEvent.OnBackgroundSaveBegin, OnBackgroundSaveBegin);
        AhnosEventDispatcher.Remove(GameEvent.OnSaveEnd, OnAutosaveEnd);
    }

    void OnAutosaveBegin()
    {
        //gameObject.SetActive(true);
        //m_group.Toggle(true);
        m_backgroundImage.gameObject.SetActive(true);
        m_indicatorObject.gameObject.SetActive(true);
    }

    void OnAutosaveEnd()
    {
        //gameObject.SetActive(false);
        //m_group.Toggle(false);
        m_backgroundImage.gameObject.SetActive(false);
        m_indicatorObject.gameObject.SetActive(false);
    }

    void OnBackgroundSaveBegin()
    {
        m_backgroundImage.gameObject.SetActive(false);
        m_indicatorObject.gameObject.SetActive(true);
    }
    
}
