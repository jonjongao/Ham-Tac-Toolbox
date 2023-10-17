using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Globalization;

namespace HamTac
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        static string saveFilePath => Application.persistentDataPath + Path.DirectorySeparatorChar + "game_config.json";
        static string buildSerialPath => Application.dataPath + "/BuildCount.txt";
        static string debugFilePath => Application.persistentDataPath + Path.DirectorySeparatorChar + "debug.txt";
        [SerializeField]
        Config m_runningConfig;
        public Config runningConfig => m_runningConfig;


        static GameConfig m_cache;
        public event UnityAction OnConfigChange
        {
            add => onConfigChange += value;
            remove=>onConfigChange -= value;
        }
        UnityAction onConfigChange;

        public static async void Initialize(UnityAction onChange, UnityAction onComplete)
        {
            var instance = LOAD();
            instance.onConfigChange = onChange;
            await instance.LoadConfigFile();
            onComplete?.Invoke();
        }
        public void RequestUpdate()
        {
            onConfigChange?.Invoke();
        }

        [Button("Load")]
        public async Task LoadConfigFile()
        {
            JDebug.W($"Try load config from:{saveFilePath}");
            Config config = null;
            if (File.Exists(saveFilePath))
            {
                try
                {
                    var text = await File.ReadAllTextAsync(saveFilePath);
                    config = JsonUtility.FromJson<Config>(text);
                    m_runningConfig = config;
                    onConfigChange?.Invoke();
                }
                catch (System.Exception err)
                {
                    Debug.LogError(err);
                }
            }
            if (config == null)
            {
                config = new Config().Default();
                await SaveConfigFile(config);
            }
        }

        [Button("Save")]
        public async Task SaveConfigFile()
        {
            await SaveConfigFile(m_runningConfig);
        }
        public async Task SaveConfigFile(Config config)
        {
            try
            {
                //var lang = I2.Loc.LocalizationManager.CurrentLanguageCode;
                //if (lang.Equals("zh-TW") ||
                //   lang.Equals("en"))
                //{

                //}
                //else
                //{
                //    lang = "en";
                //}
#if UNITY_EDITOR
                //var serial = 0;
                //if (File.Exists(buildSerialPath))
                //{
                //    var text = await File.ReadAllTextAsync(buildSerialPath);
                //    if (int.TryParse(text, out serial))
                //    {
                //    }
                //}
                //config.buildSerialNumber = serial;
#else
                if (m_runningConfig != null)
                    config.buildSerialNumber = m_runningConfig.buildSerialNumber;
#endif
                var version = Application.version;
                config.version = version;
                //config.language = lang;
                config.timestamp = System.DateTime.Now.ToString("yyMMddHHmmss");
                JDebug.W($"SaveConfigFile");
                var json = JsonUtility.ToJson(config, true);
                await File.WriteAllTextAsync(saveFilePath, json);
                m_runningConfig = config;
                onConfigChange?.Invoke();
            }
            catch (System.Exception err)
            {
                Debug.LogError(err);
            }
        }

        public static GameConfig LOAD()
        {
            try
            {
                if (m_cache == null)
                {
                    var res = Resources.Load<GameConfig>($"GameConfig");
                    m_cache = res;
                }
                return m_cache;
            }
            catch (System.Exception err)
            {
                Debug.LogError(err);
            }
            return null;
        }

        public static bool IS_DEBUG_MODE
        {
            get
            {
                var exist = File.Exists(debugFilePath);
                return Application.isEditor ? true : exist;
            }
        }
    }



    [System.Serializable]
    public class Config
    {
        public string version;
        public string timestamp;
        public int buildSerialNumber;
        public int width;
        public int height;
        public Vector2 resolution=>new Vector2Int(width, height);
        public int aspectRatioX;
        public int aspectRatioY;
        public int refreshRate;
        public int fullScreen;
        public bool isFullScreen
        {
            get { return fullScreen != 0; }
            set
            {
                fullScreen = value ? 1 : 0;
            }
        }
        public int masterVol;
        public int musicVol;
        public int soundVol;
        public int voiceVol;
        public int quality;
        public int dialogSpeed;
        public string language;

        public void ScalingScreenPosition(ref Vector2 screenPosition, int referenceWidth, int referenceHeight, bool capSize)
        {
            var ratio = new Vector2((float)referenceWidth / width, (float)referenceHeight / height);
            if (capSize && (ratio.x>=1f || ratio.y>=1f))
                screenPosition = screenPosition;
            else
                screenPosition *= ratio;
        }
        public void ScalingScreenPosition(ref Vector3 screenPosition, int referenceWidth, int referenceHeight, bool capSize)
        {
            var ratio = new Vector2((float)referenceWidth / width, (float)referenceHeight / height);
            if (capSize && (ratio.x >= 1f || ratio.y >= 1f))
                screenPosition = screenPosition;
            else
                screenPosition *= ratio;
        }

        public Config Default()
        {
            CultureInfo systemCulture = CultureInfo.CurrentCulture;
            string languageTag = systemCulture.Name;
            var lang = "en";
            if (languageTag.Equals("zh-TW"))
                lang = "zh-TW";
            else if (languageTag.Equals("zh-CN"))
                lang = "zh-CN";
            else if (languageTag.StartsWith("ja", StringComparison.OrdinalIgnoreCase))
                lang = "ja";
            else
                lang = "en";
            Debug.Log($"Initialize config file in language:{lang}");
            return new Config()
            {
                width = Mathf.Clamp(Screen.width, 1280, 1920),
                height = Mathf.Clamp(Screen.height, 720, 1080),
                aspectRatioX = 16,
                aspectRatioY = 9,
                refreshRate = 60,
                fullScreen = 1,
                masterVol = 100,
                musicVol = 50,
                soundVol = 50,
                voiceVol = 50,
                quality = 6,
                dialogSpeed = 40,
                language = lang,
            };
        }
    }
}