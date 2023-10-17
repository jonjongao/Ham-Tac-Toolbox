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

    bool m_defaultAllowSwitchOff;

    protected override void Awake()
    {
        m_childToggles = GetComponentsInChildren<Toggle>().ToList();
        m_defaultAllowSwitchOff = allowSwitchOff;
        base.Awake();
    }

    public void CustomSetToggleValue(int index,bool sendCallback=true)
    {
        allowSwitchOff = true;

        if (sendCallback)
        {
            for (var i = 0; i < m_childToggles.Count; i++)
            {
                if(i==index)
                {
                    m_childToggles[i].isOn = true;
                    m_childToggles[i].onValueChanged?.Invoke(true);
                }
                else
                {
                    m_childToggles[i].isOn = false;
                    m_childToggles[i].onValueChanged?.Invoke(false);
                }
              
            }
        }
        else
        {
            for (var i = 0; i < m_childToggles.Count; i++)
            {
                if (i == index)
                {
                    m_childToggles[i].SetIsOnWithoutNotify(true);
                }
                else
                {
                    m_childToggles[i].SetIsOnWithoutNotify(false);
                }
            }
        }

        allowSwitchOff = m_defaultAllowSwitchOff;
        m_selectedIndex = index;
    }

    public void CustomSetAllTogglesOff(bool sendCallback = true)
    {
        //bool oldAllowSwitchOff = allowSwitchOff;
        allowSwitchOff= true;

        if (sendCallback)
        {
            for (var i = 0; i < m_childToggles.Count; i++)
            {
                m_childToggles[i].isOn = false;
                m_childToggles[i].onValueChanged?.Invoke(false);
            }
        }
        else
        {
            for (var i = 0; i < m_childToggles.Count; i++)
                m_childToggles[i].SetIsOnWithoutNotify(false);
        }

        allowSwitchOff = m_defaultAllowSwitchOff;
        m_selectedIndex = -1;
    }

    protected override void OnEnable()
    {
        m_childToggles = GetComponentsInChildren<Toggle>().ToList();
        CustomSetAllTogglesOff(true);
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
