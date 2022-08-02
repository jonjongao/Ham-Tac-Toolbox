using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    CanvasGroup m_group;
    [SerializeField]
    ProgressBar m_progressBar;

    private void Awake()
    {
        m_group = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        SetProgress(0f);
        m_group.Toggle(true);
    }

    private void OnDisable()
    {
        m_group.Toggle(false);
    }

    public void SetProgress(float value)
    {
        if (m_progressBar.gameObject.activeInHierarchy == false)
            m_progressBar.gameObject.SetActive(true);
        m_progressBar.SetNormalize(value);
    }
}
