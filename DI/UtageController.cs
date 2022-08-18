using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading;
using System.Threading.Tasks;
#if UTAGE_INSTALLED
using Utage;
#endif
using HamTac;

public class UtageController : MonoBehaviour
{
#if UTAGE_INSTALLED
    static UtageController m_current;
    public static UtageController current
    {
        get
        {
            if (m_current == null)
                m_current = FindObjectOfType<UtageController>();
            return m_current;
        }
        set => m_current = value;
    }


    [SerializeField]
    AdvEngine m_engine;

    public event UnityAction<Callback> OnChange;

    [SerializeField]
    protected bool m_isPlaying;
    public bool IS_PLAYING => current.m_isPlaying;

    public bool IS_INITIALIZED => current.m_engine != null;

    [SerializeField]
    string m_startScenario;

    private void Start()
    {
        if (string.IsNullOrEmpty(m_startScenario) == false)
        {
            Initialize();
            StartDialog(m_startScenario, null);
        }
    }

    public void Initialize()
    {
        m_engine = FindObjectOfType<AdvEngine>();
        m_engine.ScenarioPlayer.OnEndScenario.AddListener((e) => OnDialogEnd());
        m_engine.ScenarioPlayer.OnBeginScenario.AddListener((e) => OnDialogBegin());
        RefreshGraphicManagerSortingLayer("UI");
    }

    public void OnDialogBegin()
    {
        JDebug.Log("Utage", $"OnDialogBegin", Extension.Color.zinc);
        RefreshGraphicManagerSortingLayer("UI");
        //!Freeze game
        OnChange?.Invoke(Callback.DialogBegin);
        m_isPlaying = true;
    }

    public void OnDialogEnd()
    {
        JDebug.Log("Utage", $"OnDialogEnd", Extension.Color.zinc);
        //!Unfreeze game
        OnChange?.Invoke(Callback.DialogEnd);
        m_isPlaying = false;
    }

    public async void StartDialog(string id, UnityAction onComplete)
    {
        JDebug.Log($"Try StartDialog:{id}");
        await StartDialogAsync(id, false, onComplete);
    }

    public async Task StartDialogAsync(string id, bool isForced, UnityAction onComplete)
    {
        if(string.IsNullOrEmpty(id))
        {
            onComplete?.Invoke();
            return;
        }

        if (isForced)
            m_engine.JumpScenario(id);
        else
            m_engine.StartGame(id);
        m_engine.Config.IsSkip = false;
        JDebug.Log($"Try StartDialogAsync:{id}");


        var tcs = new TaskCompletionSource<bool>();
        await Extension.Async.WaitUntil(() => !m_isPlaying);
        onComplete?.Invoke();

    }

    public void StopDialog()
    {
        m_engine.EndScenario();
        OnChange?.Invoke(Callback.DialogStop);
        m_isPlaying = false;
    }

    void RefreshGraphicManagerSortingLayer(string name)
    {
        var canvas = m_engine.GraphicManager.GetComponentsInChildren<Canvas>();
        if (canvas != null && canvas.Length > 0)
        {
            foreach (var c in canvas)
            {
                if (c.sortingLayerName.Equals(name) == false)
                    c.sortingLayerName = name;
            }
        }
    }

    public async void ForcedStartDialog(string id, UnityAction onComplete)
    {
        await StartDialogAsync(id, true, onComplete);
    }

    public enum Callback
    {
        DialogBegin, DialogEnd, DialogStop
    }

    public async void SetParameter(string key, object value)
    {
        await Extension.Async.WaitWhile(() => m_engine == null, 60);

        var success = m_engine.Param.TrySetParameter(key, value);
        if (success)
            JDebug.Log($"Success set utage param, key:{key} value:{value}");
        else
            JDebug.Log($"Try set utage param failed, key:{key} value:{value}");
    }

    public T GetParameter<T>(string key) where T : System.IEquatable<T>
    {
        //if (typeof(T) != typeof(bool) ||
        //    typeof(T) != typeof(string) ||
        //    typeof(T) != typeof(int) ||
        //    typeof(T) != typeof(float)) throw null;
        var obj = m_engine.Param.GetParameter<T>(key);
        if (obj != null)
            JDebug.Log($"Success get utage param, key:{key} value:{obj}");
        else
            JDebug.Log($"Try get utage param failed, key:{key}");
        return obj;
    }

    public async Task<T> GetParameterAsync<T>(string key) where T : System.IEquatable<T>
    {
        await Extension.Async.WaitUntil(() => m_engine != null);
        //if (typeof(T) != typeof(bool) ||
        //    typeof(T) != typeof(string) ||
        //    typeof(T) != typeof(int) ||
        //    typeof(T) != typeof(float)) throw null;
        try
        {
            var obj = m_engine.Param.GetParameter<T>(key);
            if (obj != null)
                JDebug.Log($"Success get utage param, key:{key} value:{obj}");
            else
                JDebug.Log($"Try get utage param failed, key:{key}");
            return obj;
        }
        catch(System.NullReferenceException)
        {
            JDebug.Log($"Try get utage param failed, key:{key}");
            return default(T);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tblKey">Example: ParamTbl{}</param>
    /// <returns></returns>
    public AdvParamStructTbl GetStructTable(string tblKey)
    {
        var tbl = m_engine.Param.StructTbl[tblKey];
        return tbl;
    }

    public void SetStructTableParameter(string tblKey, string rowKey, string colKey, object value)
    {
        //ParamTbl[E5S001].flag
        var path = $"{tblKey}[{rowKey}].{colKey}";
        SetParameter(path, value);
    }

    public T GetStructTableParameter<T>(string tblKey, string rowKey, string colKey) where T : System.IEquatable<T>
    {
        //ParamTbl[E5S001].flag
        var path = $"{tblKey}[{rowKey}].{colKey}";
        JDebug.Log($"Try get tbl:{path}");
        return GetParameter<T>(path);
    }

    public async Task<T> GetStructTableParameterAsync<T>(string tblKey, string rowKey, string colKey) where T : System.IEquatable<T>
    {
        //ParamTbl[E5S001].flag
        var path = $"{tblKey}[{rowKey}].{colKey}";
        return await GetParameterAsync<T>(path);
    }
#else
    public bool IS_PLAYING => false;
    public bool IS_INITIALIZED => true;
    public void Initialize() { }
    public async void StartDialog(string id, UnityAction onComplete) { }
#endif
}
