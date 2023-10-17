using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
using AhnosArk;
using System.Threading.Tasks;
using I2.Loc;

namespace HamTac
{
    public class GameSettingViewBase : MonoBehaviour
    {
        public static GameSettingViewBase current;

        [SerializeField]
        CanvasGroup m_group;
        [SerializeField]
        TMP_Dropdown m_languageDropdown;
        [SerializeField]
        Image m_languageBlocker;
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
        [SerializeField]
        List<Vector2Int> m_resolutionCache = new List<Vector2Int>();
        Dictionary<int, TMP_Dropdown.OptionData> m_targetFrameRateOptions = new Dictionary<int, TMP_Dropdown.OptionData>();

        protected virtual void Awake()
        {
            current = this;
            m_masterSlider.wholeNumbers = m_bgmSlider.wholeNumbers = m_sfxSlider.wholeNumbers = m_dialogStopDurationSlider.wholeNumbers= false;
            m_masterSlider.minValue = m_bgmSlider.minValue = m_sfxSlider.minValue =m_dialogStopDurationSlider.minValue= 0.0001f;
            m_masterSlider.maxValue = m_bgmSlider.maxValue = m_sfxSlider.maxValue =m_dialogStopDurationSlider.maxValue= 1f;
            m_qualitySlider.wholeNumbers = true;
            m_qualitySlider.minValue = 0f;
            m_qualitySlider.maxValue = 6f;

            m_refreshRateDropdown.onValueChanged.AddListener(OnTargetFrameRateChange);

            m_masterSlider.onValueChanged.AddListener(OnMasterVolChange);
            m_bgmSlider.onValueChanged.AddListener(OnBgmVolChange);
            m_sfxSlider.onValueChanged.AddListener(OnSfxVolChange);

            GameConfig.Initialize(OnPreferenceChange,()=>
            {
                FetchFromSetting();
            });
        }

        void OnTargetFrameRateChange(int index)
        {
            var value = m_targetFrameRateOptions.Keys.ToList()[index];
            JDebug.Log($"Set target frame rate to {value}");
        }

        private void OnPreferenceChange()
        {
            //var s = GameSettingModelBase.current.preference;
            try
            {
                var c = GameConfig.LOAD().runningConfig;
                var rr = new RefreshRate();
                rr.numerator = (uint)c.refreshRate;
                var fm = FullScreenMode.Windowed;
                if(c.isFullScreen)
                {
                    fm = FullScreenMode.FullScreenWindow;
//#if UNITY_STANDALONE_WIN
//                    fm = FullScreenMode.ExclusiveFullScreen;
//#else
//                    fm = FullScreenMode.FullScreenWindow;
//#endif
                }

                Screen.SetResolution(c.width, c.height, fm ,rr);
                Debug.Log($"Setting preference change");
                //Debug.Log($"Setting fullscreen:{c.isFullScreen}");
                //Application.targetFrameRate = c.refreshRate;
                //Screen.SetResolution(s.width, s.height, s.isFullScreen, s.refreshRate);
                //Application.targetFrameRate = s.refreshRate;
                QualitySettings.SetQualityLevel(c.quality);

                if(LocalizationManager.CurrentLanguageCode.Equals(c.language)==false)
                    I2.Loc.LocalizationManager.CurrentLanguageCode = c.language;

                OnMasterVolChange(c.masterVol * 0.01f);
                OnBgmVolChange(c.musicVol * 0.01f);
                OnSfxVolChange(c.soundVol * 0.01f);
            }
            catch(System.Exception err)
            {
                Debug.LogError(err);
                Debug.LogError($"Cant find preference file");
            }
        }

        private void OnDestroy()
        {
            m_masterSlider.onValueChanged.RemoveListener(OnMasterVolChange);
            m_bgmSlider.onValueChanged.RemoveListener(OnBgmVolChange);
            m_sfxSlider.onValueChanged.RemoveListener(OnSfxVolChange);
        }

        void OnMasterVolChange(float value)
        {
            AudioControllerBase.SetVolume(AudioChannel.Master, value);
        }

        void OnBgmVolChange(float value)
        {
            AudioControllerBase.SetVolume(AudioChannel.BGM, value);
        }

        void OnSfxVolChange(float value)
        {
            AudioControllerBase.SetVolume(AudioChannel.SFX, value);
        }

        private void OnEnable()
        {
            FetchFromSetting();
        }

        void FetchFromSetting()
        {
            if(GameModel.HAS_PLAYER_DATA)
            {
                m_languageDropdown.interactable = false;
                m_languageBlocker.gameObject.SetActive(true);
            }
            else
            {
                m_languageDropdown.interactable = true;
                m_languageBlocker.gameObject.SetActive(false);
            }

            var c = GameConfig.LOAD().runningConfig;
            //var preference = GameSettingModelBase.current.preference;
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
                if (m_resolutionCache[i].x == c.width &&
                    m_resolutionCache[i].y == c.height)
                {
                    r_index = i;
                    JDebug.Log($"OnEnable Pick user index:[{i}]{m_resolutionCache[i].x}x{m_resolutionCache[i].y}");
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
                    {-1,new TMP_Dropdown.OptionData(TermModel.Get(Term.UNLIMIT)) }
                };

            m_refreshRateDropdown.options = m_targetFrameRateOptions.Values.ToList();
            m_refreshRateDropdown.value = m_targetFrameRateOptions.Keys.ToList().IndexOf(c.refreshRate);

            m_fullScreenToggle.isOn = c.isFullScreen;
            m_masterSlider.value = c.masterVol / 100f;
            m_bgmSlider.value = c.musicVol / 100f;
            m_sfxSlider.value = c.soundVol / 100f;
            m_qualitySlider.value = c.quality;
            m_dialogStopDurationSlider.value = c.dialogSpeed / 100f;
        }

        private void OnDisable()
        {
            m_resolutionCache.Clear();
        }

        public async Task ApplySetting()
        {
            JDebug.Q($"ApplySetting");
            try
            {
                var s = GameConfig.LOAD().runningConfig;

                s.width = m_resolutionCache[m_resolutionDropdown.value].x;
                s.height = m_resolutionCache[m_resolutionDropdown.value].y;

                var code = I2.Loc.LocalizationManager.GetLanguageCode(m_languageDropdown.options[m_languageDropdown.value].text);
                JDebug.Q($"ApplySetting code:{code}");
                s.language = code;
                s.isFullScreen = m_fullScreenToggle.isOn;
                s.refreshRate = m_targetFrameRateOptions.Keys.ToList()[m_refreshRateDropdown.value];
                s.masterVol = Mathf.RoundToInt(m_masterSlider.value * 100f);
                s.musicVol = Mathf.RoundToInt(m_bgmSlider.value * 100f);
                s.soundVol = Mathf.RoundToInt(m_sfxSlider.value * 100f);
                s.quality = Mathf.RoundToInt(m_qualitySlider.value);
                s.dialogSpeed = Mathf.RoundToInt(m_dialogStopDurationSlider.value*100f);
                await GameConfig.LOAD().SaveConfigFile();
            }
            catch(System.Exception err)
            {
                Debug.LogError(err);
            }
        }
    }
}