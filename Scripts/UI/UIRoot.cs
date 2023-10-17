using HamTac;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIRoot : MonoBehaviour
{
    CanvasGroup m_canvasGroup;
    [SerializeField]
    bool m_markAsBase;
    public bool markAsBase => m_markAsBase;

    [SerializeField]
    string m_id;
    public string id => m_id;
    [SerializeField]
    bool m_hideInAwake = true;

    protected virtual void Awake()
    {
        m_canvasGroup = GetComponent<CanvasGroup>();
       
    }

    private void Start()
    {
        if (m_hideInAwake)
            Hide();
        else
            Show();
    }

    [Button("Show")]
    public void Show()
    {
        m_canvasGroup.Toggle(true);
        GetComponent<UIEventHandler>()?.Toggle(true);
    }


    [Button("Hide")]
    public void Hide()
    {
        m_canvasGroup.Toggle(false);
        GetComponent<UIEventHandler>()?.Toggle(false);
    }

    public void Mark()
    {
        m_markAsBase = true;
    }

    public void Unmark()
    {
        m_markAsBase = false;
    }

    public bool IsOn()
    {
        return m_canvasGroup.IsOn();
    }
}
