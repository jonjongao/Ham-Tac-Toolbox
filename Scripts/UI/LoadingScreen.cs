using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    UIEventHandler m_handler;
    public UIEventHandler handler => m_handler;
    [SerializeField]
    ProgressBar m_progressBar;

    private void Awake()
    {
        m_handler = GetComponent<UIEventHandler>();
        //m_handler.Toggle(false);
        m_handler.IsOn.AddListener(On);
        m_handler.IsOff.AddListener(Off);
    }

    void On()
    {

    }

    void Off()
    {
        //m_progressBar.gameObject.SetActive(false);
    }

    public void SetProgress(float value)
    {
        if (m_progressBar.gameObject.activeInHierarchy == false)
            m_progressBar.gameObject.SetActive(true);
        m_progressBar.SetNormalize(value);
    }
}
