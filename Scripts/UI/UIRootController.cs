using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIRootController : MonoBehaviour
{
    static UIRootController m_current;
    public static UIRootController current => m_current;
    [SerializeField]
    bool m_autoGetChildRoots;
    [SerializeField]
    List<UIRoot> m_roots = new List<UIRoot>();
    [SerializeField]
    UIRoot m_nowBase;

    private void Awake()
    {
        m_current = this;
    }

    private void Start()
    {
        if (m_autoGetChildRoots)
            m_roots = GetComponentsInChildren<UIRoot>().ToList();
    }

    private void Update()
    {
        foreach (var i in m_roots)
        {
            if (i == null)
                continue;
            if (i.IsOn())
                return;
        }
        //todo沒有任何Root顯示
        foreach (var i in m_roots)
        {
            if (i == null)
                continue;
            if (i.markAsBase)
            {
                SwitchToAsBase(i.id);
                break;
            }
        }
    }

    public bool SwitchToAsBase(string id)
    {
        return SwitchTo(id, false, true, false);
    }
    public bool SwitchToAsDefault(string id)
    {
        return SwitchTo(id,true,false,true);
    }

    public UIRoot GetRoot(string id)
    {
        foreach(var i in m_roots)
        {
            if (i.Equals(id))
                return i;
        }
        return null;
    }

    public bool SwitchTo(string id, bool toggleIfAlreadyOn, bool markAsBase, bool keepBase)
    {
        Debug.Log($"UIRoot SwitchTo:{id} toggle:{toggleIfAlreadyOn} mark:{markAsBase}");
        var result = false;
        //todo單純處理主頁標記，不做開關
        if (markAsBase)
        {
            foreach (var i in m_roots)
            {
                if (i == null)
                    continue;
                if (i.markAsBase)
                    i.Unmark();
            }
            foreach (var i in m_roots)
            {
                if (i == null)
                    continue;
                if (i.id.Equals(id))
                {
                    i.Mark();
                    m_nowBase = i;
                    break;
                }
            }
        }

        //todo關閉所有已開啟頁面
        foreach (var i in m_roots)
        {
            if (i == null)
                continue;
            //todo保持主頁狀態
            if (keepBase && i.markAsBase)
                continue;
            if (i.IsOn() && i.id.Equals(id) == false)
            {
                i.Hide();
            }
        }
        var exist = false;
        foreach (var i in m_roots)
        {
            if (i == null)
                continue;
            if (i.id.Equals(id))
            {
                if (toggleIfAlreadyOn)
                {
                    if (i.IsOn())
                    {
                        i.Hide();
                        result = false;
                    }
                    else
                    {
                        i.Show();
                        result = true;
                    }
                }
                else
                {
                    i.Show();
                    result = true;
                }
                exist = true;
            }
        }
        if (!exist)
            Debug.LogWarning($"Cant find root:{id}");
        return result;
    }

    public void ClearAllMark()
    {
        foreach (var i in m_roots)
        {
            if (i == null)
                continue;
            if (i.markAsBase)
                i.Unmark();
        }
    }
    public void UnmarkFrom(string id)
    {
        Debug.Log($"UIRoot UnmarkFrom:{id}");
        foreach (var i in m_roots)
        {
            if (i == null)
                continue;
            if (i.id.Equals(id))
                i.Unmark();
        }
    }

    public void MarkTo(string id)
    {
        Debug.Log($"UIRoot MarkTo:{id}");
        foreach (var i in m_roots)
        {
            if (i == null)
                continue;
            if (i.id.Equals(id))
                i.Mark();
        }
    }

    //[Button]
    //public void SwitchToInventroy()
    //{
    //    SwitchTo("Inventroy", true, false);
    //}

    //[Button]
    //public void SwitchToOption()
    //{
    //    SwitchTo("Option", true, false);
    //}

    //[Button]
    //public void SwithToHUDThenSetBase()
    //{
    //    SwitchTo("Hud", false, true);
    //}
    //[Button]
    //public void SwithToBattleThenSetBase()
    //{
    //    SwitchTo("Battle", false, true);
    //}
}
