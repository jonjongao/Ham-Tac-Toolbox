using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExtendedToggleGroup : ToggleGroup
{
    [SerializeField]
    int m_selectIndexOnEnable = 0;

    protected override void OnEnable()
    {
        var arr = GetComponentsInChildren<Toggle>();
        for (int i = 0; i < arr.Length; i++)
        {
            if (i == m_selectIndexOnEnable)
                arr[i].isOn = true;
            else
                arr[i].isOn = false;
            arr[i].onValueChanged?.Invoke(arr[i].isOn);
        }
        base.OnEnable();
    }
}
