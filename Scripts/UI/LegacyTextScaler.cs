using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LegacyTextScaler : UIBehaviour
{
    [SerializeField]
    MaskableGraphic m_sampleText;
    [SerializeField]
    MaskableGraphic m_displayText;
    [SerializeField]
    [Multiline]
    string m_text;
    public string text
    {
        get => m_text;
        set
        {
            m_text = value;
            RefreshText();
        }
    }
    public int fontSize = 28;
    public Vector2 margin;
    [Range(1f, 5f)]
    public float scalingRatio = 2f;

    protected override void OnEnable()
    {
        base.OnEnable();
        RefreshText();
    }

    [ContextMenu("RefreshText")]
    public void RefreshText()
    {
        m_displayText.rectTransform.localScale = new Vector3(1f / scalingRatio, 1f / scalingRatio, 1f);
        var fitter = m_sampleText.GetComponent<ContentSizeFitter>();
        if (fitter == null)
            fitter = m_sampleText.gameObject.AddComponent<ContentSizeFitter>();
        if (fitter.horizontalFit != ContentSizeFitter.FitMode.PreferredSize)
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        if (fitter.verticalFit != ContentSizeFitter.FitMode.PreferredSize)
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        if (m_sampleText.color != Color.clear)
            m_sampleText.color = Color.clear;
        if (m_sampleText is Text)
        {
            (m_sampleText as Text).text = m_text;
            (m_sampleText as Text).fontSize = fontSize;
        }
        else if (m_sampleText is TextMeshProUGUI)
        {
            (m_sampleText as TextMeshProUGUI).text = m_text;
            (m_sampleText as TextMeshProUGUI).fontSize = fontSize;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_sampleText.rectTransform);
        //await Task.Yield();
        if (m_displayText is Text)
        {
            (m_displayText as Text).text = m_text;
            (m_displayText as Text).fontSize = Mathf.FloorToInt(fontSize * scalingRatio);
            (m_displayText as Text).horizontalOverflow = HorizontalWrapMode.Overflow;
            (m_displayText as Text).verticalOverflow = VerticalWrapMode.Overflow;
        }
        else if (m_displayText is TextMeshProUGUI)
        {
            (m_displayText as TextMeshProUGUI).text = m_text;
            (m_displayText as TextMeshProUGUI).fontSize = Mathf.FloorToInt(fontSize * scalingRatio);
            (m_displayText as TextMeshProUGUI).overflowMode = TextOverflowModes.Overflow;
        }
        m_displayText.rectTransform.sizeDelta = m_sampleText.rectTransform.sizeDelta * scalingRatio;
        (transform as RectTransform).sizeDelta = m_sampleText.rectTransform.sizeDelta + margin;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Initialize")]
    public void Initialize()
    {
        if (m_sampleText == null)
        {
            var obj = new GameObject("Sample", typeof(Text), typeof(ContentSizeFitter));
            obj.transform.SetParent(transform, false);
            m_sampleText = obj.GetComponent<Text>();
            text = "NewText";
            var fitter = obj.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(obj);
#endif
        }
        if (m_displayText == null)
        {
            var obj = new GameObject("Display", typeof(Text));
            obj.transform.SetParent(transform, false);
            m_displayText = obj.GetComponent<Text>();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(obj);
#endif
        }
        RefreshText();
    }
}
