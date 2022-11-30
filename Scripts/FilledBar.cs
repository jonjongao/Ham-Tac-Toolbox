using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HamTac;
using DG.Tweening;

public class FilledBar : MonoBehaviour
{
    [SerializeField]
    Image m_bgImage;
    [SerializeField]
    Image m_filledImage;
    Color m_defaultFilledColor;
    Sequence m_tween;
    [SerializeField]
    Color m_increasePopColor = Color.green;
    [SerializeField]
    Color m_decreasePopColor = Color.red;
    [SerializeField]
    bool m_autoHideIfZeroValue = false;
    CanvasGroup m_group;


    private void Awake()
    {
        m_filledImage.fillAmount = 0f;
        m_group = GetComponent<CanvasGroup>();
        if (m_autoHideIfZeroValue)
            m_group.alpha = 0f;
        m_defaultFilledColor = m_filledImage.color;
    }

    public void SetNormalizeValue(float value)
    {
        var preValue = m_filledImage.fillAmount;
        var isAdd = value >= preValue;
        var color = isAdd ? m_increasePopColor : m_decreasePopColor;

        if (m_autoHideIfZeroValue &&
            value > 0.01f &&
            m_group.alpha <= 0.01f)
            m_group.alpha = 1f;

        var t = DOTween.Sequence();
        JDebug.W($"Set fill amount from:{m_filledImage.fillAmount} to:{value}");
        t.Append(m_filledImage.DOFillAmount(value, 0.5f));
        t.Join(m_bgImage.transform.DOPunchScale(new Vector3(0.4f, 0.4f, 0f), 0.25f));
        t.Join(m_filledImage.DOColor(color, 0.2f));
        t.Insert(0.4f, m_filledImage.DOColor(m_defaultFilledColor, 0.3f));
        t.OnComplete(() =>
        {
            m_bgImage.transform.localScale = Vector3.one;
            m_filledImage.color = m_defaultFilledColor;
            if (m_filledImage.fillAmount <= 0.01f &&
                m_autoHideIfZeroValue &&
                m_group.alpha > 0.01f)
            {
                m_group.alpha = 0f;
            }
        });
        t.SetId(gameObject);
    }

    private void OnDestroy()
    {
        DOTween.Kill(gameObject);
    }
}
