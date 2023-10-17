using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if TextMeshPro
using TMPro;
#endif
using System.Threading;
using System.Threading.Tasks;
using HamTac;
using System.Linq;
using System;
using AhnosArk;

public class ConfirmPopup : MonoBehaviour
{
    static Dictionary<string, ConfirmPopup> m_current = new Dictionary<string, ConfirmPopup>();
    public static ConfirmPopup Get(string id) { return m_current[id]; }
    public static ConfirmPopup Get() { return m_current["default"]; }

    [SerializeField]
    protected string m_id;
    public string id => m_id;
    [SerializeField]
    protected CanvasGroup m_group;
    [SerializeField]
    protected RectTransform m_window;
    [SerializeField]
    protected Graphic m_contextText;
    [SerializeField]
    protected RectTransform m_buttonGroup;
    [SerializeField]
    protected GameObject[] m_buttonsTemplate;
    public int buttonLength => m_buttonsTemplate.Length;
    protected Selectable[] m_usingButtons;
    protected int m_selectedIndex = -1;
    [SerializeField]
    protected float m_bottomMargin = 40f;

    public static string[] DEFAULT_YES_NO => new string[] { TermModel.Get(Term.YES), TermModel.Get(Term.NO) };
    public static string[] DEFAULT_YES => new string[] { TermModel.Get(Term.YES) };
    public static string DEFAULT_JUST_YES => TermModel.Get(Term.YES);

    protected virtual async void Awake()
    {
        foreach (var i in m_buttonsTemplate)
        {
            var btn = i.GetComponentInChildren<Selectable>();
            if (btn is Button)
                (btn as Button).onClick.AddListener(() => OnButtonClicked(btn as Button));
            else if (btn is Toggle)
                (btn as Toggle).onValueChanged.AddListener((t) => OnToggleClicked(btn as Toggle));
        }
        if (m_group.gameObject.activeSelf == false)
        {
            //m_group.gameObject.SetActive(true);
            Toggle(true);
            await Task.Yield();
        }
        //m_group.gameObject.SetActive(false);
        Toggle(false);
        if (m_current.ContainsKey(m_id))
            Debug.LogError($"ConfrimPopup already has key:{m_id} gameObject:{gameObject?.name}");
        else if (string.IsNullOrEmpty(m_id))
            Debug.LogWarning($"ConfirmPopup {gameObject.name} has no id");
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
        if (string.IsNullOrEmpty(m_id) == false && m_current.ContainsKey(m_id))
            m_current.Remove(m_id);
    }

    public virtual void SetContext(string[] context)
    {
        if (m_contextText is Text)
            (m_contextText as Text).text = context[0];
#if TextMeshPro
        else if (m_contextText is TMPro.TextMeshProUGUI)
            (m_contextText as TMPro.TextMeshProUGUI).text = context[0];
#endif
    }

    public virtual GameObject GetButtonByIndex(int index)
    {
        try
        {
            return m_buttonsTemplate[index];
        }
        catch (System.Exception error)
        {
            return null;
        }
    }

    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {
        m_selectedIndex = -1;
    }

    public void Show() { Toggle(true); }
    public void Hide() { Toggle(false); }
    public virtual void Toggle(bool value)
    {
        //Debug.LogError($"On popup:{m_id} toggle:{value}");
        //Debug.Log($"Set group:{m_group.gameObject} active:{value}");
        m_group.gameObject.SetActive(value);
    }

    protected virtual void OnToggleClicked(Toggle toggle)
    {
        if (m_usingButtons == null || m_usingButtons.Length == 0 ||
            toggle.group == null) return;
        JDebug.Log($"Select toggle object:{toggle.name}");

        var obj = toggle.group.GetFirstActiveToggle();
        if (obj == null)
        {
            m_selectedIndex = -1;
            return;
        }
        m_selectedIndex = m_usingButtons.IndexOf(obj as Selectable);
        if (toggle.group.allowSwitchOff)
            toggle.group.allowSwitchOff = false;
        JDebug.Log($"Select toggle index:{m_selectedIndex} on popup:{gameObject.name} obj:{obj?.name}");
    }

    protected virtual void OnButtonClicked(Button button)
    {
        if (m_usingButtons == null || m_usingButtons.Length == 0) return;
        m_selectedIndex = m_usingButtons.IndexOf(button as Selectable);
        JDebug.Log($"Select index:{m_selectedIndex} on popup:{gameObject.name}");
    }

    public virtual async Task<int> Initialize(Func<int, Task<bool>> callback, string context, params string[] options)
    {
        return await Initialize(callback, new string[] { context }, options);
    }

    public virtual async Task<int> Initialize(string context, Action<int> callback, params string[] options)
    {
        return await Initialize((i) =>
        {
            callback?.Invoke(i);
            return new Task<bool>(() => { return true; });
        }, new string[] { context }, options);
    }

    public virtual async Task<int> Initialize(string[] context, string[] options, Action<int> callback = null)
    {
        return await Initialize((i) =>
        {
            callback?.Invoke(i);
            return new Task<bool>(() => { return true; });
        }, context, options);
    }

    public virtual async Task<int> Initialize(Func<int, Task<bool>> callback, string[] context, string[] options)
    {
        await Apply(context, options);

        while (m_selectedIndex < 0)
        {
            await Task.Yield();
            if (Application.isPlaying == false)
                break;
        }
        var result = m_selectedIndex;
        Toggle(false);
        callback?.Invoke(result);
        m_selectedIndex = -1;
        return result;
    }

    public virtual async Task Apply(string[] context,
        string[] options)
    {
        await Apply(context, options, -1);
    }

    public virtual async Task Apply(string[] context,
        string[] options, int defaultSelectedIndex)
    {
        var yMargin = 0f;
        var xMargin = 0f;
        if (m_contextText is Text)
        {
            (m_contextText as Text).text = context[0];
            yMargin = 20f;
            xMargin = 20f;
            m_contextText.rectTransform.anchoredPosition = new Vector2(m_contextText.rectTransform.anchoredPosition.x, -yMargin);
            m_contextText.rectTransform.sizeDelta = new Vector2(-xMargin * 2f, m_contextText.rectTransform.sizeDelta.y);
        }
#if TextMeshPro
        else if (m_contextText is TMPro.TextMeshProUGUI)
            (m_contextText as TMPro.TextMeshProUGUI).text = context[0];
#endif
        for (int i = 0; i < m_buttonsTemplate.Length; i++)
        {
            m_buttonsTemplate[i].gameObject.SetActive(i < options.Length);
            if (i < options.Length)
            {
#if TextMeshPro
                var a = m_buttonsTemplate[i].GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (a)
                {
                    a.text = options[i];
                    continue;
                }
#endif
                var b = m_buttonsTemplate[i].GetComponentInChildren<Text>();
                if (b) b.text = options[i];
            }
        }
        Toggle(true);
        await Extension.Async.Yield(m_applyDelayFrame);
        var height = m_contextText.rectTransform.sizeDelta.y +
            m_buttonGroup.sizeDelta.y + m_bottomMargin + (yMargin * 2f);
        JDebug.Log($"Calc Popup window height:{m_contextText.rectTransform.sizeDelta.y}+{m_buttonGroup.sizeDelta.y}+{m_bottomMargin} on {gameObject.name}");
        var size = m_window.sizeDelta;
        size.y = height;
        m_window.sizeDelta = size;

        m_usingButtons = m_buttonsTemplate.Select(x => x.GetComponent<Selectable>()).ToArray();
        //m_usingButtons = m_buttonGroup.GetComponentsInChildren<Selectable>();
    }

    protected int m_applyDelayFrame = 15;
}
