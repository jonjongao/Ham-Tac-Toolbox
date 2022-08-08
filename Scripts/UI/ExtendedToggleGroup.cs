using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

public class ExtendedToggleGroup : ToggleGroup
{
    [SerializeField]
    int m_selectIndexOnEnable = 0;
    public UnityEvent<Toggle> OnSelected;
    int m_selectedIndex;
    public int selectedIndex => m_selectedIndex;

    protected override void Awake()
    {
        m_Toggles = GetComponentsInChildren<Toggle>().ToList();
        base.Awake();
    }

    protected override void OnEnable()
    {
        for (int i = 0; i < m_Toggles.Count; i++)
        {
            if (i == m_selectIndexOnEnable)
                m_Toggles[i].isOn = true;
            else
                m_Toggles[i].isOn = false;
            m_Toggles[i].onValueChanged.AddListener(OnAnyToggleValueChanged);
            m_Toggles[i].onValueChanged?.Invoke(m_Toggles[i].isOn);
        }
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        foreach (var i in m_Toggles)
            i.onValueChanged.RemoveListener(OnAnyToggleValueChanged);
        base.OnDisable();
    }

    void OnAnyToggleValueChanged(bool value)
    {
        var obj = GetFirstActiveToggle();
        var index = m_Toggles.IndexOf(obj);
        m_selectedIndex = index;
        OnSelected?.Invoke(obj);
    }

    public int IndexOfToggle(Toggle value)
    {
        return m_Toggles.IndexOf(value);
    }
}
