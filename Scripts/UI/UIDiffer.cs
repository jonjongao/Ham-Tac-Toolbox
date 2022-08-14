using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HamTac;

public class UIDiffer : MonoBehaviour
{
    [SerializeField]
    Graphic[] m_colorTintTargets;
    [SerializeField]
    Graphic[] m_spriteSwapTargets;
    [SerializeField]
    Graphic[] m_textApplyTargets;

    public void DoApply(DifferStyle style)
    {
        if (style.hasTint)
        {
            foreach (var i in m_colorTintTargets)
                i.CrossFadeColor(style.tint, 0f, true, false);
        }
        if (style.hasSprite)
        {
            foreach (var i in m_spriteSwapTargets)
            {
                if (i is Image)
                    (i as Image).sprite = style.sprite;
            }
        }
        if (style.hasText)
        {
            foreach (var i in m_textApplyTargets)
            {
                if (i is Text)
                    (i as Text).text = style.text;
                if (i is TMPro.TextMeshProUGUI)
                    (i as TMPro.TextMeshProUGUI).text = style.text;
            }
        }
    }
}

[System.Serializable]
public class DifferStyle
{
    public bool hasTint { private set; get; }
    protected Color m_tint;
    public Color tint
    {
        get => m_tint;
        set
        {
            hasTint = true;
            m_tint = value;
        }
    }
    public bool hasSprite => m_sprite != null;
    protected Sprite m_sprite;
    public Sprite sprite => m_sprite;
    public bool hasText { private set; get; }
    protected string m_text;
    public string text
    {
        get => m_text;
        set
        {
            hasText = true;
            m_text = value;
        }
    }
}