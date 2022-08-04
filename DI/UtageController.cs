using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading;
using System.Threading.Tasks;
using Utage;
using HamTac;

public class UtageController : MonoBehaviour, IDialogController
{
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

    public event UnityAction<IDialogController.Callback> OnChange;

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
        OnChange?.Invoke(IDialogController.Callback.DialogBegin);
        m_isPlaying = true;
    }

    public void OnDialogEnd()
    {
        JDebug.Log("Utage", $"OnDialogEnd", Extension.Color.zinc);
        //!Unfreeze game
        OnChange?.Invoke(IDialogController.Callback.DialogEnd);
        m_isPlaying = false;
    }

    public async void StartDialog(string id, UnityAction onComplete)
    {
        await StartDialogAsync(id, false, onComplete);
    }

    public async Task StartDialogAsync(string id, bool isForced, UnityAction onComplete)
    {
        if (isForced)
            m_engine.JumpScenario(id);
        else
            m_engine.StartGame(id);
        m_engine.Config.IsSkip = false;
        JDebug.Log($"Try start dialog:{id}");


        var tcs = new TaskCompletionSource<bool>();
        await Extension.Async.WaitUntil(() => !m_isPlaying);
        onComplete?.Invoke();

    }

    public void StopDialog()
    {
        m_engine.EndScenario();
        OnChange?.Invoke(IDialogController.Callback.DialogStop);
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
}
