using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBar : MonoBehaviour
{
    [SerializeField]
    Slider m_slider;
    [SerializeField]
    TextMeshProUGUI m_text;

    private void OnEnable()
    {
        SetNormalize(0f);
    }

    public void SetNormalize(float value)
    {
        m_slider.value = Mathf.Clamp01(value);
        m_text.text = $"{Mathf.CeilToInt(value * 100)}%";
    }
}
