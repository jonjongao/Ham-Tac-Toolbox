using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using System.Threading.Tasks;
using HamTac;

public class ConfirmPopup : MonoBehaviour
{
    static ConfirmPopup m_current;
    public static ConfirmPopup current
    {
        get
        {
            if (m_current == null)
                m_current = FindObjectOfType<ConfirmPopup>();
            return m_current as ConfirmPopup;
        }
        set => m_current = value;
    }
    [SerializeField]
    CanvasGroup m_group;
    [SerializeField]
    TextMeshProUGUI m_contextText;
    [SerializeField]
    RectTransform m_buttonGroup;
    [SerializeField]
    GameObject[] m_buttonsTemplate;
    Button[] m_usingButtons;
    int m_selectedIndex = -1;
    [SerializeField]
    float m_bottomMargin = 40f;

    private void Awake()
    {
        foreach (var i in m_buttonsTemplate)
        {
            var btn = i.GetComponentInChildren<Button>();
            if (btn)
                btn.onClick.AddListener(() => OnButtonClicked(btn));
        }
        m_group.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        foreach (var i in m_buttonsTemplate)
        {
            var btn = i.GetComponentInChildren<Button>();
            if (btn)
                btn.onClick.RemoveAllListeners();
        }
    }

    private void OnDisable()
    {
        m_selectedIndex = -1;
    }

    void OnButtonClicked(Button button)
    {
        m_selectedIndex = m_usingButtons.IndexOf(button);
        JDebug.Log($"Select index:{m_selectedIndex} on popup:{gameObject.name}");
    }

    public async Task<int> Initialize(string context, string[] options, System.Action<int> callback = null)
    {
        m_contextText.text = context;
        for (int i = 0; i < m_buttonsTemplate.Length; i++)
        {
            m_buttonsTemplate[i].gameObject.SetActive(i < options.Length);
            if (i < options.Length)
            {
                var t = m_buttonsTemplate[i].GetComponentInChildren<TextMeshProUGUI>();
                if (t) t.text = options[i];
            }
        }
        m_group.gameObject.SetActive(true);
        await Task.Yield();
        var height = m_contextText.rectTransform.sizeDelta.y +
            m_buttonGroup.sizeDelta.y + m_bottomMargin;
        var size = (m_group.transform as RectTransform).sizeDelta;
        size.y = height;
        (m_group.transform as RectTransform).sizeDelta = size;

        m_usingButtons = m_buttonGroup.GetComponentsInChildren<Button>();
        while (m_selectedIndex < 0)
        {
            await Task.Yield();
            if (Application.isPlaying == false)
                break;
        }
        var result = m_selectedIndex;
        m_group.gameObject.SetActive(false);
        callback?.Invoke(result);
        m_selectedIndex = -1;
        return result;
    }
}
