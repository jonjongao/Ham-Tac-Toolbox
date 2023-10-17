using DG.Tweening;
using HamTac;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlidePopup : MonoBehaviour
{
    static Dictionary<string, SlidePopup> m_current = new Dictionary<string, SlidePopup>();
    public static SlidePopup Get(string id)
    {
        if (m_current == null || m_current.ContainsKey(id) == false)
            return null;
        return m_current[id];
    }
    public static SlidePopup Get() { return Get("default"); }

    [SerializeField]
    protected string m_id;
    public string id => m_id;
    [SerializeField]
    private CanvasGroup m_group;
    [SerializeField]
    int m_pageIndex;
    [SerializeField]
    int m_startIndex = 1;
    [SerializeField]
    RectTransform m_pageContainer;
    [SerializeField]
    MaskableGraphic m_pageNumberText;
    [SerializeField]
    Selectable m_prevIndicator;
    [SerializeField]
    Selectable m_nextIndicator;
    [SerializeField]
    GameObject m_closeIndicator;
    Selectable m_closeButton;
    [SerializeField]
    bool m_hideCloseButtonUntilLastPage = false;
    [SerializeField]
    List<RectTransform> m_pages = new List<RectTransform>();
    [SerializeField]
    float m_blendDuration = 0f;

    public static bool IS_ANY_DISPLAYING()
    {
        foreach(var i in m_current)
        {
            if(i.Value.gameObject.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    private void Awake()
    {
        for (int i = 0; i < m_pageContainer.childCount; i++)
        {
            var p = m_pageContainer.GetChild(i);
            m_pages.Add(p as RectTransform);
        }
        gameObject.SetActive(false);
        if (m_current.ContainsKey(m_id))
            Debug.LogError($"ConfrimPopup already has key:{m_id} gameObject:{gameObject?.name}");
        else
            m_current.Add(m_id, this);
    }

    protected void OnDestroy()
    {
        if (m_current.ContainsKey(m_id))
            m_current.Remove(m_id);
    }

    public virtual void Toggle(bool value)
    {
        gameObject.SetActive(value);
    }


    private void OnEnable()
    {
        if (m_prevIndicator is Button)
            (m_prevIndicator as Button).onClick.AddListener(OnClickPrev);
        if (m_nextIndicator is Button)
            (m_nextIndicator as Button).onClick.AddListener(OnClickNext);
        if (m_closeIndicator)
        {
            m_closeButton = m_closeIndicator.GetComponentInChildren<Button>();
            if (m_closeButton && m_closeButton is Button)
                (m_closeButton as Button).onClick.AddListener(OnClickClose);
        }
        ApplyPage(m_startIndex);
    }

    private void OnDisable()
    {
        if (m_prevIndicator is Button)
            (m_prevIndicator as Button).onClick.RemoveListener(OnClickPrev);
        if (m_nextIndicator is Button)
            (m_nextIndicator as Button).onClick.RemoveListener(OnClickNext);
        if (m_closeIndicator && m_closeButton)
        {
            if (m_closeButton is Button)
                (m_closeButton as Button).onClick.RemoveListener(OnClickClose);
        }
        foreach (var i in m_pages)
        {
            i.gameObject.SetActive(false);
        }
    }

    public void OnClickClose()
    {
        Toggle(false);
    }

    public void OnClickPrev()
    {
        ApplyPage(m_pageIndex - 1);
    }

    public void OnClickNext()
    {
        ApplyPage(m_pageIndex + 1);
    }

    public virtual async Task<bool> Initialize()
    {
        if (TBSFController.current != null)
        {
            if (TBSFController.current.turnOperateHelper.IsOperating())
            {
                JDebug.E($"Turn is operating, wait until its end");
                await Extension.Async.WaitUntil(() => TBSFController.current.turnOperateHelper.IsOperating() == false);
            }
        }

        Toggle(true);
        while (gameObject.activeInHierarchy)
        {
            await Task.Yield();
            if (Application.isPlaying == false)
                break;
        }
        Toggle(false);
        PlayerOperateRespondQueue.EXECUTE_RESP($"SlidePopup/{m_id}");
        return false;
    }

    public async void ApplyPage(int index)
    {
        var hasPrev = false;
        var hasNext = false;
        var maxNum = m_pages.Count;
        index = Mathf.Clamp(index, 0, maxNum - 1);

        if (m_group && m_blendDuration > 0f)
        {
            m_group.DOFade(0f, m_blendDuration * 0.5f);
            await Extension.Async.Delay(m_blendDuration * 0.5f);
        }

        for (int i = 0; i < m_pages.Count; i++)
        {
            if (i == index)
            {
                m_pages[i].gameObject.SetActive(true);
            }
            else
            {
                m_pages[i].gameObject.SetActive(false);
            }
        }
        if (index != 0)
            hasPrev = true;
        if (index != maxNum - 1)
            hasNext = true;
        if (m_pageNumberText)
        {
            var numberStr = $"{index + 1}/{maxNum}";
            if (m_pageNumberText is Text)
            {
                (m_pageNumberText as Text).text = numberStr;
            }
            else if (m_pageNumberText is TextMeshProUGUI)
            {
                (m_pageNumberText as TextMeshProUGUI).text = numberStr;
            }
        }
        if (m_prevIndicator)
            m_prevIndicator.gameObject.SetActive(hasPrev);
        if (m_nextIndicator)
            m_nextIndicator.gameObject.SetActive(hasNext);
        if (m_hideCloseButtonUntilLastPage)
        {
            if (hasNext == false)
            {
                if (m_closeIndicator)
                    m_closeIndicator.gameObject.SetActive(true);
            }
            else
            {
                if (m_closeIndicator)
                    m_closeIndicator.gameObject.SetActive(false);
            }
        }
        else
        {
            if (m_closeIndicator && m_closeIndicator.gameObject.activeInHierarchy == false)
                m_closeIndicator.gameObject.SetActive(true);
        }
        m_pageIndex = index;

        if (m_group && m_blendDuration > 0f)
        {
            m_group.DOFade(1f, m_blendDuration * 0.5f);
        }
    }
}
