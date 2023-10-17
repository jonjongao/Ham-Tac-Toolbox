using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExtendedUITransition : UIBehaviour, ISelectHandler, IDeselectHandler, IPointerClickHandler, ISubmitHandler
{
    [SerializeField]
    bool m_listenEventHandler = true;
    public Mode mode = Mode.SelectAndDeselect;
    public enum Mode
    {
        SelectAndDeselect,
        PointerClickAndSubmit
    }
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
            i.color = selectColor;
        }
        foreach (var i in m_alphaTargets)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, selectAlpha);
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
            i.color = deselectColor;
        }
        foreach (var i in m_alphaTargets)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, deselectAlpha);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (m_listenEventHandler && mode == Mode.SelectAndDeselect) DoSelect();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (m_listenEventHandler && mode == Mode.SelectAndDeselect) DoDeselect();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        JDebug.W($"UITrans PointerClick.0");
        if (!m_listenEventHandler || mode != Mode.PointerClickAndSubmit) return;

        DoSelect();
        JDebug.W($"UITrans PointerClick.1");
        StartCoroutine(OnFinishSubmit());
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (!m_listenEventHandler || mode != Mode.PointerClickAndSubmit) return;

        DoSelect();
        StartCoroutine(OnFinishSubmit());
    }

    private IEnumerator OnFinishSubmit()
    {
        var fadeTime = 0.25f;
        var elapsedTime = 0f;
        JDebug.W($"UITrans PointerClick.2");
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        JDebug.W($"UITrans PointerClick.3");
        DoDeselect();
    }
}
