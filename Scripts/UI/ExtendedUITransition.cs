using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExtendedUITransition : UIBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField]
    bool m_listenEventHandler = true;
    [SerializeField]
    Graphic[] m_spriteSwapTargets;
    [SerializeField]
    Graphic[] m_colorTintTargets;
    [SerializeField]
    Graphic[] m_alphaTargets;
    [Header("SpriteSwap")]
    public Sprite selectSprite;
    public Sprite deselectSprite;
    [Header("ColorTint")]
    public Color selectColor = Color.white;
    public Color deselectColor = Color.gray;
    [Header("Alpha")]
    public float selectAlpha = 1f;
    public float deselectAlpha = 0f;

    protected override void OnEnable()
    {
       
    }

    protected override void OnDisable()
    {
        DoDeselect();

    }

    public void OnToggle(bool value)
    {
        if (value) DoSelect();
        else DoDeselect();
    }

    public void DoSelect()
    {
        foreach (var i in m_spriteSwapTargets)
        {
            if (i is Image)
                (i as Image).sprite = selectSprite;
        }
        foreach (var i in m_colorTintTargets)
        {
            i.CrossFadeColor(selectColor, 0f, true, false);
        }
        foreach (var i in m_alphaTargets)
        {
            i.CrossFadeAlpha(selectAlpha, 0f, true);
        }
    }

    public void DoDeselect()
    {
        foreach (var i in m_spriteSwapTargets)
        {
            if (i is Image)
                (i as Image).sprite = deselectSprite;
        }
        foreach (var i in m_colorTintTargets)
        {
            i.CrossFadeColor(deselectColor, 0f, true, false);
        }
        foreach (var i in m_alphaTargets)
        {
            i.CrossFadeAlpha(deselectAlpha, 0f, true);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (m_listenEventHandler) DoSelect();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (m_listenEventHandler) DoDeselect();
    }
}
