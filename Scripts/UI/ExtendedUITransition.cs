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
    [Header("SpriteSwap")]
    public Sprite selectSprite;
    public Sprite deselectSprite;
    [Header("ColorTint")]
    public Color selectColor = Color.white;
    public Color deselectColor = Color.gray;

    protected override void OnEnable()
    {
        DoDeselect();
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
        //m_targetGraphic.sprite = selectSprite;
        foreach (var i in m_spriteSwapTargets)
        {
            if (i is Image)
                (i as Image).sprite = selectSprite;
        }
        foreach (var i in m_colorTintTargets)
        {
            i.CrossFadeColor(selectColor, 0f, true, false);
        }
    }

    public void DoDeselect()
    {
        //m_targetGraphic.sprite = deselectSprite;
        foreach (var i in m_spriteSwapTargets)
        {
            JDebug.Log($"on deselect:{i.name}");

            if (i is Image)
                (i as Image).sprite = deselectSprite;
        }
        foreach (var i in m_colorTintTargets)
        {
            i.CrossFadeColor(deselectColor, 0f, true, false);
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
