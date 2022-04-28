using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public class DebugButton : MonoBehaviour
{
    [SerializeField]
    Text m_text;
    [SerializeField]
    Button m_button;

    public string text
    {
        get => m_text.text;
        set => m_text.text = value;
    }
    public bool interactable
    {
        get => m_button.interactable;
        set => m_button.interactable = value;
    }
    public Vector2 size
    {
        get => (transform as RectTransform).sizeDelta;
        set
        {
            var e = GetComponent<LayoutElement>();
            e.minWidth = value.x;
            e.minHeight = value.y;
            (transform as RectTransform).sizeDelta = value;
        }
    }

    public event UnityAction OnClick
    {
        add
        {
            m_button.onClick.AddListener(value);
        }
        remove
        {
            m_button.onClick.RemoveListener(value);
        }
    }
}
