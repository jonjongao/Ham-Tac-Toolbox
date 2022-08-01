using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class GameSettingView : MonoBehaviour
{
    public static GameSettingView current;

    [SerializeField]
    CanvasGroup m_group;
    [SerializeField]
    TMP_Dropdown m_resolutionDropdown;
    [SerializeField]
    TMP_Dropdown m_refreshRateDropdown;
    [SerializeField]
    Toggle m_fullScreenToggle;
    [SerializeField]
    Slider m_masterSlider;
    [SerializeField]
    Slider m_bgmSlider;
    [SerializeField]
    Slider m_sfxSlider;
    [SerializeField]
    Slider m_qualitySlider;
    [SerializeField]
    Slider m_dialogStopDurationSlider;
    List<Vector2Int> m_resolutionCache = new List<Vector2Int>();
    Dictionary<int, TMP_Dropdown.OptionData> m_targetFrameRateOptions = new Dictionary<int, TMP_Dropdown.OptionData>();

    protected virtual void Awake()
    {
        current = this;
        m_masterSlider.wholeNumbers = m_bgmSlider.wholeNumbers = m_sfxSlider.wholeNumbers = false;
        m_masterSlider.minValue = m_bgmSlider.minValue = m_sfxSlider.minValue = 0.0001f;
        m_masterSlider.maxValue = m_bgmSlider.maxValue = m_sfxSlider.maxValue = 1f;
        m_qualitySlider.wholeNumbers = true;
        m_qualitySlider.minValue = 0f;
        m_qualitySlider.maxValue = 6f;

        GameSettingModel.current.Initialize();
        GameSettingModel.current.OnPreferenceChange += OnPreferenceChange;

        m_refreshRateDropdown.onValueChanged.AddListener(OnTargetFrameRateChange);

        m_masterSlider.onValueChanged.AddListener(OnMasterVolChange);
        m_bgmSlider.onValueChanged.AddListener(OnBgmVolChange);
        m_sfxSlider.onValueChanged.AddListener(OnSfxVolChange);
    }

    void OnTargetFrameRateChange(int index)
    {
        var value = m_targetFrameRateOptions.Keys.ToList()[index];
        Application.targetFrameRate = value;
        JDebug.Log($"Set target frame rate to {value}");
    }

    private void OnPreferenceChange()
    {
        
    }

    private void OnDestroy()
    {
        m_masterSlider.onValueChanged.RemoveListener(OnMasterVolChange);
        m_bgmSlider.onValueChanged.RemoveListener(OnBgmVolChange);
        m_sfxSlider.onValueChanged.RemoveListener(OnSfxVolChange);
    }

    void OnMasterVolChange(float value)
    {
        AudioController.SetVolume(AudioChannel.Master, value);
    }

    void OnBgmVolChange(float value)
    {
        AudioController.SetVolume(AudioChannel.BGM, value);
    }

    void OnSfxVolChange(float value)
    {
        AudioController.SetVolume(AudioChannel.SFX, value);
    }


    private void OnEnable()
    {
        var preference = GameSettingModel.current.preference;
        var rs = Screen.resolutions;
        var r = Display.main;
        JDebug.Log($"All optional resolution:{JDebug.ListToLog(rs)}");
        JDebug.Log($"OnEnable Current system resolution:{r.systemWidth}x{r.systemWidth}");
        //Debug.LogFormat("<color=yellow>Current system resolution:{0}x{1}</color>", r.systemWidth, r.systemHeight);
        List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
        var r_index = 0;

        var r_log = string.Empty;

        foreach (var i in rs)
        {
            var this_r = new Vector2Int(i.width, i.height);
            if (m_resolutionCache.Contains(this_r) == false && this_r.x >= 1280)
            {
                m_resolutionCache.Add(this_r);
                list.Add(new TMP_Dropdown.OptionData(string.Format("{0}x{1}", this_r.x, this_r.y)));
                r_log += this_r + "\n";
            }
        }

        //Debug.LogFormat("List of avaliable resolution:{0}", r_log);
        JDebug.Log($"OnEnable List of avaliable resolution:{r_log}");

        for (int i = 0; i < m_resolutionCache.Count; i++)
        {
            if (m_resolutionCache[i].x == preference.width &&
                m_resolutionCache[i].y == preference.height)
            {
                r_index = i;
                JDebug.Log($"OnEnable Pick user index:[{i}]{ m_resolutionCache[i].x}x{m_resolutionCache[i].y}");
                //Debug.LogFormat("Pick user index:[{0}]{1}x{2}", i, m_resolutionCache[i].x, m_resolutionCache[i].y);
                break;
            }
        }

        m_resolutionDropdown.options = list;
        m_resolutionDropdown.value = r_index;

        m_targetFrameRateOptions = new Dictionary<int, TMP_Dropdown.OptionData>()
        {
            {30,new TMP_Dropdown.OptionData("30") },
            {60,new TMP_Dropdown.OptionData("60") },
            {144,new TMP_Dropdown.OptionData("144") },
            {-1,new TMP_Dropdown.OptionData("คฃณ]ญญ") }
        };

        m_refreshRateDropdown.options = m_targetFrameRateOptions.Values.ToList();
        m_refreshRateDropdown.value = m_targetFrameRateOptions.Keys.ToList().IndexOf(preference.refreshRate);

        m_fullScreenToggle.isOn = preference.isFullScreen;
        m_masterSlider.value = preference.masterVol / 100f;
        m_bgmSlider.value = preference.bgmVol / 100f;
        m_sfxSlider.value = preference.sfxVol / 100f;
        m_qualitySlider.value = preference.quality;
        m_dialogStopDurationSlider.value = preference.dialogStopDuration;
    }

    private void OnDisable()
    {
        m_resolutionCache.Clear();
    }

    public void ApplySetting()
    {
        var s = new GameSettingModel.Preference();
        s.width = m_resolutionCache[m_resolutionDropdown.value].x;
        s.height = m_resolutionCache[m_resolutionDropdown.value].y;
        s.isFullScreen = m_fullScreenToggle.isOn;
        s.masterVol = Mathf.RoundToInt(m_masterSlider.value * 100f);
        s.bgmVol = Mathf.RoundToInt(m_bgmSlider.value * 100f);
        s.sfxVol = Mathf.RoundToInt(m_sfxSlider.value * 100f);
        s.quality = Mathf.RoundToInt(m_qualitySlider.value);
        s.dialogStopDuration = Mathf.RoundToInt(m_dialogStopDurationSlider.value);
        GameSettingModel.current.Save(s);
    }
}
