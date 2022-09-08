using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using HamTac;

public class ExtendedToggleGroup : ToggleGroup
{
    [SerializeField]
    int m_selectIndexOnEnable = 0;
    public UnityEvent<Toggle> OnSelected;
    int m_selectedIndex;
    public int selectedIndex => m_selectedIndex;
    List<Toggle> m_childToggles;

    protected override void Awake()
    {
        m_childToggles = GetComponentsInChildren<Toggle>().ToList();
        base.Awake();
    }

    protected override void OnEnable()
    {
        m_childToggles = GetComponentsInChildren<Toggle>().ToList();
        for (int i = 0; i < m_childToggles.Count; i++)
        {
            if (i == m_selectIndexOnEnable)
            {
                m_childToggles[i].isOn = true;
            }
            else
                m_childToggles[i].isOn = false;
            m_childToggles[i].onValueChanged.AddListener(OnAnyToggleValueChanged);
            m_childToggles[i].onValueChanged?.Invoke(m_childToggles[i].isOn);
        }
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        m_childToggles = GetComponentsInChildren<Toggle>().ToList();
        foreach (var i in m_childToggles)
        {
            i.onValueChanged.RemoveListener(OnAnyToggleValueChanged);
            i.isOn = false;
        }
        base.OnDisable();
    }

    void OnAnyToggleValueChanged(bool value)
    {
        var obj = GetFirstActiveToggle();
        var index = m_childToggles.IndexOf(obj);
        m_selectedIndex = index;
        OnSelected?.Invoke(obj);
    }

    public int IndexOfToggle(Toggle value)
    {
        return m_childToggles.IndexOf(value);
    }
}
