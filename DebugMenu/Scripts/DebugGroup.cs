using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DebugGroup : MonoBehaviour
{
    [SerializeField]
    CanvasGroup m_group;
    [SerializeField]
    RectTransform m_container;

    public CanvasGroup group
    {
        get
        {
            if (m_group == null)
                m_group = GetComponent<CanvasGroup>();
            return m_group;
        }
    }

    public bool interactable
    {
        get => m_group.interactable;
        set
        {
            m_group.interactable = m_group.blocksRaycasts = value;
        }
    }

    public RectTransform container
    {
        get
        {
            if (m_container == null) return transform as RectTransform;
            else return m_container;
        }
    }
}
