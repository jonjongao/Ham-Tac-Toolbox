using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class AutoFitSizeOnRefresh : UIBehaviour
{
    [SerializeField]
    Graphic m_graphic;

    protected override void OnEnable()
    {
        m_graphic.RegisterDirtyLayoutCallback(OnDirty);
    }

    protected override void OnDisable()
    {
        m_graphic.UnregisterDirtyLayoutCallback(OnDirty);
    }

    [ContextMenu("Refresh")]
    public void OnDirty()
    {
        (transform as RectTransform).sizeDelta = m_graphic.rectTransform.sizeDelta;
    }
}
