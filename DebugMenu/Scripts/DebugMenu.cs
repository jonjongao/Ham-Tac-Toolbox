using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public static class DebugMenu
{
    static RectTransform root;
    static UnityAction updateHook;
    static Dictionary<string, DebugGroup> list = new Dictionary<string, DebugGroup>();

    public static void Init(Canvas parent)
    {
        parent.sortingOrder = 32767;
        root = BeginContainer(parent.transform);
    }

    public static bool Toggle(string key)
    {
        return list[key].group.Toggle();
    }
    public static void Toggle(string key, bool show)
    {
        list[key].group.Toggle(show);
    }

    public static void OnUpdate(UnityAction callback)
    {
        updateHook += callback;
    }

    public static void Update()
    {
        if (updateHook != null)
            updateHook.Invoke();
    }

    public static void Destroy()
    {
        root = null;
        updateHook = null;
        list.Clear();
    }

    public static RectTransform BeginCanvas(Transform parent)
    {
        var res = Resources.Load("DebugCanvas") as GameObject;
        var obj = GameObject.Instantiate(res, parent);
        return obj.transform as RectTransform;
    }

    static RectTransform BeginContainer(Transform parent)
    {
        var res = Resources.Load("DebugContainer") as GameObject;
        var obj = GameObject.Instantiate(res, parent);
        return obj.transform as RectTransform;
    }

    public static DebugGroup BeginVGroup(string key)
    {
        if (root == null)
        {
            Debug.LogWarningFormat("Root is not init yet");
            return null;
        }
        var obj = BeginVGroup(root);
        obj.gameObject.name = key;
        list.Add(key, obj);
        return obj;
    }
    static DebugGroup BeginVGroup(Transform parent)
    {
        var res = Resources.Load("VGroup") as GameObject;
        var obj = GameObject.Instantiate(res, parent);
        var script = obj.GetComponent<DebugGroup>();
        return script;
    }

    public static DebugGroup BeginVScrollGroup(string key)
    {
        if (root == null)
        {
            Debug.LogWarningFormat("Root is not init yet");
            return null;
        }
        var obj = BeginVScrollGroup(root);
        obj.gameObject.name = key;
        list.Add(key, obj);
        return obj;
    }
    static DebugGroup BeginVScrollGroup(Transform parent)
    {
        var res = Resources.Load("VScrollGroup") as GameObject;
        var obj = GameObject.Instantiate(res, parent);
        var script = obj.GetComponent<DebugGroup>();
        return script;
    }

    public static void AddButton(this DebugGroup group, string label, UnityAction callback)
    {
        var res = Resources.Load("DebugButton") as GameObject;
        var size = new Vector2((group.transform as RectTransform).sizeDelta.x, (res.transform as RectTransform).sizeDelta.y);
        AddButton(group, res, size, label, callback);
    }
    public static void AddButton(this DebugGroup group, Vector2 size, string label, UnityAction callback)
    {
        var res = Resources.Load("DebugButton") as GameObject;
        AddButton(group, res, size, label, callback);
    }
    public static void AddButton(this DebugGroup group, GameObject template, Vector2 size, string label, UnityAction callback)
    {
        var obj = GameObject.Instantiate(template, group.container);
        obj.gameObject.name = label;
        var script = obj.GetComponent<DebugButton>();
        script.size = size;
        script.text = label;
        script.OnClick += callback;
    }

    public static void SetResolution(Vector2Int resolution)
    {
        if (root == null)
        {
            Debug.LogWarningFormat("Root is not init yet");
            return;
        }
        var scaler = root.GetComponentInParent<CanvasScaler>();
        if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = resolution;
    }
}
