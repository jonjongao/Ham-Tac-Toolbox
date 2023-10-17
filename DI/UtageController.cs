using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Video;
#if UTAGE_INSTALLED
using Utage;
#endif
using HamTac;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEditor;
using AhnosArk;
using System.Linq;

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
    public AdvEngine engine => m_engine;
    public event UnityAction<Callback> OnChange;

    [SerializeField]
    protected bool m_isPlaying;
    public bool IS_PLAYING => current.m_isPlaying;

    public bool IS_INITIALIZED => current.m_engine != null;

    [SerializeField]
    string m_startScenario;

    public bool isSpeedUp = false;
    public bool isAutoPlay = false;

    public float normalPlaySpeed = 0.5f;
    public float speedUpPlaySpeed = 1f;

    public float normalDialogPause = 0.5f;
    public float speedUpDialogPause = 0.25f;

    [SerializeField]
    string[] m_characterNameParamKey;
    const string characterParamTag = "char?";

    [Sirenix.OdinInspector.Button("ParseStringToCharacterParamKeys")]
    public void ParseStringToCharacterParamKeys(string input)
    {
        string[] lines = input.Split(' ');
        m_characterNameParamKey = lines;
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    private void Awake()
    {
        m_current = this;
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(m_startScenario) == false)
        {
            Initialize();
            StartDialog(m_startScenario, null);
        }
        //Debug.LogError($"def language:{LanguageManagerBase.Instance.DefaultLanguage} all lang:{JDebug.ListToLog(LanguageManagerBase.Instance.Languages)}");
        AhnosEventDispatcher.On(GameEvent.PrepareUnloadScene, OnPrepareUnloadScene);
        LocalizationManager.OnLocalizeEvent += LocalizationManager_OnLocalizeEvent;
        GameManager.current.OnBootComplete += Current_OnBootComplete;
        AhnosEventDispatcher.On(GameEvent.PlayerDataInitialized, PlayerDataInitialized);
        AhnosEventDispatcher.On(GameEvent.PlayerDataClear, PlayerDataClear);
        GameConfig.LOAD().OnConfigChange += UtageController_OnConfigChange;
    }



    private void UtageController_OnConfigChange()
    {
        //JDebug.W($"On config update");
        if (m_engine)
        {
            var s = GameConfig.LOAD();
            m_engine.SoundManager.MasterVolume = s.runningConfig.masterVol * 0.01f;
            m_engine.SoundManager.SeVolume = s.runningConfig.soundVol * 0.01f;
            m_engine.SoundManager.VoiceVolume = s.runningConfig.voiceVol * 0.01f;
        }
    }

    private void Current_OnBootComplete(object sender, System.EventArgs e)
    {
        var keys = m_engine.Param.GetDefault().Tbl.Keys.ToArray();
        var charParam = keys.Where(x => x.Contains(characterParamTag)).Select(x => x.Split(characterParamTag).Last()).ToArray();
        //JDebug.W($"Check utage def table key:{JDebug.ListToLog(charParam.ToList())}");
        m_characterNameParamKey = charParam;
        ApplyCurrentLanguage();
        UtageController_OnConfigChange();
    }

    private void LocalizationManager_OnLocalizeEvent()
    {
        if (GameManager.IS_BOOTING)
            return;
        JDebug.W($"On language change:{LocalizationManager.CurrentLanguageCode}");
        ApplyCurrentLanguage();
    }

    void PlayerDataInitialized()
    {
        ApplyCurrentLanguage();
    }

    void ApplyCurrentLanguage()
    {
        if (LocalizationManager.CurrentLanguageCode.Equals("en"))
        {
            LanguageManagerBase.Instance.CurrentLanguage = "English";
        }
        else if (LocalizationManager.CurrentLanguageCode.Equals("zh-TW"))
        {
            LanguageManagerBase.Instance.CurrentLanguage = LanguageManagerBase.Instance.DefaultLanguage;
        }
        else if (LocalizationManager.CurrentLanguageCode.Equals("zh-CN"))
        {
            LanguageManagerBase.Instance.CurrentLanguage = "Simplified Chinese";
        }
        else if (LocalizationManager.CurrentLanguageCode.Equals("ja"))
        {
            if (GameModel.HAS_PLAYER_DATA)
            {
                Debug.Log($"Set as jp language.0 male:{GameModel.NowPlayerData.isMaleLead}");
                if (GameModel.NowPlayerData.isMaleLead)
                    LanguageManagerBase.Instance.CurrentLanguage = "Japanese(Male)";
                else
                    LanguageManagerBase.Instance.CurrentLanguage = "Japanese(Female)";
            }
            else
            {
                Debug.Log($"Set as jp language.1");
                LanguageManagerBase.Instance.CurrentLanguage = "Japanese(Male)";
            }
        }
        foreach (var i in m_characterNameParamKey)
        {
            if (TermModel.TryGet($"UnitName/{i}", out string value))
            {
                SetParameter(characterParamTag + i, value);
            }
        }
        Debug.Log($"Apply Utage language:{LanguageManagerBase.Instance.CurrentLanguage}");
    }

    private void OnDestroy()
    {
        AhnosEventDispatcher.Remove(GameEvent.PrepareUnloadScene, OnPrepareUnloadScene);
        AhnosEventDispatcher.Remove(GameEvent.PlayerDataInitialized, PlayerDataInitialized);
        AhnosEventDispatcher.Remove(GameEvent.PlayerDataClear, PlayerDataClear);
        if (m_engine)
        {
            m_engine.Page.OnBeginPage.RemoveAllListeners();
            m_engine.Page.OnEndPage.RemoveAllListeners();
            m_engine.ScenarioPlayer.OnEndScenario.RemoveAllListeners();
            m_engine.ScenarioPlayer.OnBeginScenario.RemoveAllListeners();
        }
        LocalizationManager.OnLocalizeEvent -= LocalizationManager_OnLocalizeEvent;
        GameManager.current.OnBootComplete -= Current_OnBootComplete;
        GameConfig.LOAD().OnConfigChange -= UtageController_OnConfigChange;
    }

    void OnPrepareUnloadScene()
    {
        if (m_isPlaying)
        {
            StopDialog();
        }
    }

    public void Initialize()
    {
        m_engine = FindObjectOfType<AdvEngine>();
        m_videoManager = m_engine.GraphicManager.VideoManager;
        m_engine.Page.OnBeginPage.AddListener(OnBeginPage);
        m_engine.Page.OnEndPage.AddListener(OnEndPage);
        m_engine.ScenarioPlayer.OnEndScenario.AddListener((e) => OnDialogEnd());
        m_engine.ScenarioPlayer.OnBeginScenario.AddListener((e) => OnDialogBegin());

        RefreshGraphicManagerSortingLayer("UI");
    }

    async void OnBeginPage(AdvPage advPage)
    {
        JDebug.W($"[Utage]OnBeginPage, data:{advPage.TextData.OriginalText}");
        //if (Cursor.visible != !m_videoIsPlaying)
        //    Cursor.visible = !m_videoIsPlaying;



        if (!isAutoPlay && !isSpeedUp) return;
        JDebug.W($"OnBeginPage.0");
        await Extension.Async.WaitUntil(() => advPage.IsWaitBrPage);
        //JDebug.W($"[Utage]Page play complete");
        JDebug.W($"OnBeginPage.1");
        await Extension.Async.Delay(isSpeedUp ? speedUpDialogPause : normalDialogPause);
        if (advPage == null) return;
        JDebug.W($"OnBeginPage.2");
        //todo模擬玩家點擊或跳過
        advPage.InputSendMessage();
        //todo刷新文字
        advPage.UpdateText();
        //if (m_videoManager == null)
        //    m_videoManager = m_engine.GetComponentInChildren<AdvVideoManager>();
        //if (m_videoManager)
        //    m_videoIsPlaying = m_videoManager.transform.childCount > 0;

        JDebug.W($"OnBeginPage.3");
        //if (AudioController.GetBGMSourceVolume(0, out float vol))
        //{
        //    if (vol > 0f)
        //        AudioController.SetBGMSourceVolume(0, 0f, 1f);
        //}

    }

    [SerializeField]
    bool m_videoIsPlaying;
    public bool videoIsPlaying => m_videoIsPlaying;
    [SerializeField]
    AdvVideoManager m_videoManager;

    [SerializeField]
    bool m_hasMusicPlaying;

    private void Update()
    {
        m_hasMusicPlaying = m_engine.SoundManager.IsPlayingBgm();
        if (AudioController.GetBGMSourceVolume(0, out float vol1))
        {
            //JDebug.W($"BGM index:{0} vol:{vol1} hasMusic:{m_hasMusicPlaying}");
            if (m_hasMusicPlaying && vol1 > 0f)
                AudioController.SetBGMSourceVolume(0, 0f, 0);
        }
        if (AudioController.GetBGMSourceVolume(1, out float vol2))
        {
            //JDebug.W($"BGM index:{1} vol:{vol2} hasMusic:{m_hasMusicPlaying}");
            if (m_hasMusicPlaying && vol2 > 0f)
                AudioController.SetBGMSourceVolume(1, 0f, 0);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            isSpeedUp = true;
            m_engine.Config.MessageSpeed = speedUpPlaySpeed;
            if (!isAutoPlay)
            {
                StartCoroutine(CoWaitUntilPageWaiting());
            }
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            isSpeedUp = false;
            m_engine.Config.MessageSpeed = normalPlaySpeed;
            if (!isAutoPlay)
            {
                StopAllCoroutines();
            }
        }

        if (Input.GetKey(KeyCode.Space))
        {
        }
        //if (m_videoManager == null)
        //{
        //    m_videoManager = m_engine.transform.parent.GetComponentInChildren<AdvVideoManager>();
        //}
        if (m_videoManager)
        {
            var hasPlay = m_videoManager.transform.childCount > 0;
            if (m_videoIsPlaying != hasPlay)
                m_videoIsPlaying = hasPlay;

            if (Cursor.visible != !m_videoIsPlaying)
                Cursor.visible = !m_videoIsPlaying;
        }
    }

    IEnumerator CoWaitUntilPageWaiting()
    {
        yield return new WaitUntil(() => m_engine.Page.IsWaitBrPage);
        if (!isSpeedUp) yield break;
        m_engine.Page.InputSendMessage();
        m_engine.Page.UpdateText();
    }


    void OnEndPage(AdvPage advPage)
    {
        //JDebug.W($"[Utage]OnEndPage, data:{advPage.TextData.OriginalText}");
        //if (m_videoManager == null)
        //    m_videoManager = m_engine.transform.parent.GetComponentInChildren<AdvVideoManager>();
        //if (m_videoManager)
        //    m_videoIsPlaying = m_videoManager.transform.childCount > 0;
        //if (Cursor.visible != !m_videoIsPlaying)
        //    Cursor.visible = !m_videoIsPlaying;
        //JDebug.W($"OnEndPage");
        //if (AudioController.GetBGMSourceVolume(0, out float vol))
        //{
        //    if (vol > 0f)
        //        AudioController.SetBGMSourceVolume(0, 0f, 1f);
        //}
    }



    public async void OnDialogBegin()
    {
        JDebug.Log("Utage", $"OnDialogBegin", Extension.Color.zinc);
        RefreshGraphicManagerSortingLayer("UI");
        //!Freeze game
        m_isPlaying = true;
        OnChange?.Invoke(Callback.DialogBegin);
        await Extension.Async.Delay(0.25f);
        //todo非戰鬥才降音樂音量
        //if (TBSFBattleController.current != null)
        //    AudioController.SetBGMSourceVolume(0, 0f, 1f);
    }

    public async void OnDialogEnd()
    {
        JDebug.Log("Utage", $"OnDialogEnd", Extension.Color.zinc);
        if (AudioController.GetBGMSourceVolume(0, out float vol1))
        {
            if (vol1 < 1f)
                AudioController.SetBGMSourceVolume(0, 1f, 0);
        }
        if (AudioController.GetBGMSourceVolume(1, out float vol2))
        {
            if (vol2 < 1f)
                AudioController.SetBGMSourceVolume(1, 1f, 0);
        }

        //!Unfreeze game
        m_isPlaying = false;
        OnChange?.Invoke(Callback.DialogEnd);
        await Extension.Async.Delay(0.25f);
        AudioController.SetBGMSourceVolume(0, 1f, 1f);
    }

    public async void StartDialog(string id, UnityAction onComplete)
    {
        JDebug.Log($"Try StartDialog:{id}");
        await StartDialogAsync(id, false, onComplete);
    }

    public async Task StartDialogAsync(string id, bool isForced, UnityAction onComplete)
    {
        if (string.IsNullOrEmpty(id))
        {
            onComplete?.Invoke();
            return;
        }

        if (m_engine == null)
        {
            JDebug.W($"UtageEngine is not initialized yet, wait until");
            await Extension.Async.WaitUntil(() => m_engine != null);
        }

        if (TBSFController.current != null)
        {
            if (TBSFController.current.turnOperateHelper.IsOperating())
            {
                JDebug.E($"Turn is operating, wait until its end");
                await Extension.Async.WaitUntil(() => TBSFController.current.turnOperateHelper.IsOperating() == false);
            }
        }


        if (isForced)
            m_engine.JumpScenario(id);
        else
            m_engine.StartGame(id);
        m_engine.Config.IsSkip = false;
        Debug.Log($"Try StartDialogAsync:{id}");


        //var tcs = new TaskCompletionSource<bool>();
        await Extension.Async.WaitUntil(() => !m_isPlaying);
        onComplete?.Invoke();
        PlayerOperateRespondQueue.EXECUTE_RESP($"UtageController/{id}");
    }

    public void StopDialog()
    {
        JDebug.Log($"Try StopDialog");
        m_engine.EndScenario();
        OnChange?.Invoke(Callback.DialogStop);
        m_isPlaying = false;
        if (AudioController.GetBGMSourceVolume(0, out float vol1))
        {
            if (vol1 < 1f)
                AudioController.SetBGMSourceVolume(0, 1f, 0);
        }
        if (AudioController.GetBGMSourceVolume(1, out float vol2))
        {
            if (vol2 < 1f)
                AudioController.SetBGMSourceVolume(1, 1f, 0);
        }
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

    public void PlayerDataClear()
    {
        //if (m_engine)
        //    m_engine.StartGame(string.Empty);
        m_engine.Param.InitDefaultAll(m_engine.Param.DefaultParameter);
    }

    public enum Callback
    {
        DialogBegin, DialogEnd, DialogStop
    }

    public async void SetParameter(string key, object value)
    {
        if (m_engine == null)
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
        catch (System.NullReferenceException)
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

    public void AllGraphicOff()
    {
        m_engine.GraphicManager.BgManager.FadeOutAll(m_engine.Page.ToSkippedTime(0.2f));
        m_engine.GraphicManager.SpriteManager.FadeOutAll(m_engine.Page.ToSkippedTime(0.2f));
        m_engine.GraphicManager.CharacterManager.FadeOutAll(m_engine.Page.ToSkippedTime(0.2f));
    }

    [Sirenix.OdinInspector.Button("Test")]
    public async void Test()
    {
        IS_TESTING = true;
        foreach (var page in m_engine.DataManager.ScenarioDataTbl)
        {
            foreach (var label in page.Value.ScenarioLabels)
            {
                Debug.Log($"Playing label:{label}");
                try
                {
                    await StartDialogAsync(label.Key, true, null);
                }
                catch (System.Exception error)
                {
                    Debug.LogError(error);
                }
            }
        }
    }

    public static bool IS_TESTING { get; protected set; }
}
