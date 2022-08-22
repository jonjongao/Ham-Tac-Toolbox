using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using System.Threading.Tasks;
using HamTac;
using System.Linq;

public class ConfirmPopup : MonoBehaviour
{
    static Dictionary<string, ConfirmPopup> m_current = new Dictionary<string, ConfirmPopup>();
    public static ConfirmPopup Get(string id) { return m_current[id]; }

    [SerializeField]
    protected string m_id;
    public string id => m_id;
    [SerializeField]
    protected CanvasGroup m_group;
    [SerializeField]
    protected RectTransform m_window;
    [SerializeField]
    protected TextMeshProUGUI m_contextText;
    [SerializeField]
    protected RectTransform m_buttonGroup;
    [SerializeField]
    protected GameObject[] m_buttonsTemplate;
    protected Selectable[] m_usingButtons;
    protected int m_selectedIndex = -1;
    [SerializeField]
    protected float m_bottomMargin = 40f;

    public static string[] DEFAULT_YES_NO = new string[] { "確認", "取消" };
    public static string[] DEFAULT_YES = new string[] { "確認" };

    protected async void Awake()
    {
        foreach (var i in m_buttonsTemplate)
        {
            var btn = i.GetComponentInChildren<Selectable>();
            if (btn is Button)
                (btn as Button).onClick.AddListener(() => OnButtonClicked(btn as Button));
            else if (btn is Toggle)
                (btn as Toggle).onValueChanged.AddListener((t) => OnToggleClicked(btn as Toggle));
        }
        if (m_group.gameObject.activeInHierarchy == false)
        {
            m_group.gameObject.SetActive(true);
            await Task.Yield();
        }
        m_group.gameObject.SetActive(false);
        if (m_current.ContainsKey(m_id))
            Debug.LogError($"ConfrimPopup already has key:{m_id}");
        else
            m_current.Add(m_id, this);
    }

    protected void OnDestroy()
    {
        foreach (var i in m_buttonsTemplate)
        {
            var btn = i.GetComponentInChildren<Selectable>();
            if (btn is Button)
                (btn as Button).onClick.RemoveAllListeners();
            else if (btn is Toggle)
                (btn as Toggle).onValueChanged.RemoveAllListeners();
        }
    }

    protected virtual void OnEnable()
    {
        
    }

    protected virtual void OnDisable()
    {
        m_selectedIndex = -1;
    }

    protected void OnToggleClicked(Toggle toggle)
    {
        if (m_usingButtons == null || m_usingButtons.Length == 0) return;
        var obj = toggle.group.GetFirstActiveToggle();
        if (obj == null) return;
        m_selectedIndex = m_usingButtons.IndexOf(obj as Selectable);
        if (toggle.group.allowSwitchOff)
            toggle.group.allowSwitchOff = false;
        JDebug.Log($"Select toggle index:{m_selectedIndex} on popup:{gameObject.name}");
    }

    protected void OnButtonClicked(Button button)
    {
        if (m_usingButtons == null || m_usingButtons.Length == 0) return;
        m_selectedIndex = m_usingButtons.IndexOf(button as Selectable);
        JDebug.Log($"Select index:{m_selectedIndex} on popup:{gameObject.name}");
    }

    public virtual async Task<int> Initialize(string[] context, string[] options, System.Action<int> callback = null)
    {
        m_contextText.text = context[0];
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
        await Extension.Async.Yield(15);
        var height = m_contextText.rectTransform.sizeDelta.y +
            m_buttonGroup.sizeDelta.y + m_bottomMargin;
        JDebug.Log($"Calc Popup window height:{m_contextText.rectTransform.sizeDelta.y}+{m_buttonGroup.sizeDelta.y}+{m_bottomMargin} on {gameObject.name}");
        var size = m_window.sizeDelta;
        size.y = height;
        m_window.sizeDelta = size;

        m_usingButtons = m_buttonGroup.GetComponentsInChildren<Selectable>();
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
