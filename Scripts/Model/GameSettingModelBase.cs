using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class GameSettingModelBase
{
    static GameSettingModelBase m_current;
    //public static GameSettingModelBase current
    //{
    //    get
    //    {
    //        if (m_current == null)
    //        {
    //            m_current = new GameSettingModelBase();
    //        }
    //        return m_current;
    //    }
    //}

    [System.Serializable]
    public class Preference
    {
        public int width;
        public int height;
        public Vector2Int aspectRatio;
        public int refreshRate;
        public bool isFullScreen;
        public int masterVol;
        public int bgmVol;
        public int sfxVol;
        public int quality;
        public int dialogStopDuration;
    }

    Preference m_preference;
    public Preference preference => m_preference;

    static bool m_isDebugMode;
    public static bool IS_DEBUG_MODE
    {
        get
        {
            return Application.isEditor ? true : m_isDebugMode;
        }
    }

    public static string settingFilePath => Application.streamingAssetsPath + Path.DirectorySeparatorChar + "setting.json";

    protected virtual Preference CreateDefaultPreference()
    {
        JDebug.Log("CreateDefaultOption Cant find setting file, create default setting");
        var data = new Preference
        {
            width = Screen.width,
            height = Screen.height,
            refreshRate = Screen.currentResolution.refreshRate,
            isFullScreen = true,
            masterVol = 100,
            bgmVol = 100,
            sfxVol = 100,
            quality = 6,
            dialogStopDuration = 4
        };

        if (data.width < 1920)
        {
            var res = Screen.currentResolution;
            JDebug.Log($"CreateDefaultOption Game window init in smaller resolution, User resolution: {res}");
            var ratio = (float)data.width / (float)data.height;

            //16:10
            if (Mathf.Approximately(ratio, 1.6f))
            {
                JDebug.Log("CreateDefaultOption User is 16:10 aspect ratio");
                if (res.width >= 1920)
                {
                    data.width = 1920;
                    data.height = 1200;
                }
                else if (res.width >= 1680)
                {
                    data.width = 1680;
                    data.height = 1050;
                }
                else if (res.width >= 1440)
                {
                    data.width = 1440;
                    data.height = 900;
                }
                else if (res.width >= 1280)
                {
                    data.width = 1280;
                    data.height = 800;
                }
            }
            //16:9
            else if (Mathf.Approximately(ratio, 1.777777777777778f))
            {
                JDebug.Log("CreateDefaultOption User is 16:9 aspect ratio");
                if (res.width >= 1920)
                {
                    data.width = 1920;
                    data.height = 1080;
                }
                else if (res.width >= 1600)
                {
                    data.width = 1600;
                    data.height = 900;
                }
                else if (res.width >= 1366)
                {
                    data.width = 1366;
                    data.height = 768;
                }
                else if (res.width >= 1280)
                {
                    data.width = 1280;
                    data.height = 720;
                }
            }
            //4:3
            else if (Mathf.Approximately(ratio, 1.347368421052632f))
            {
                JDebug.Log("CreateDefaultOption User is 4:3 aspect ratio");
                if (res.width >= 1920)
                {
                    data.width = 1920;
                    data.height = 1440;
                }
                else if (res.width >= 1600)
                {
                    data.width = 1600;
                    data.height = 1200;
                }
                else if (res.width >= 1440)
                {
                    data.width = 1440;
                    data.height = 1080;
                }
                else if (res.width >= 1280)
                {
                    data.width = 1280;
                    data.height = 960;
                }
            }
            else
            {
                JDebug.Log($"CreateDefaultOption User has other aspect ratio: {ratio}, forced to 16:9");
                if (res.width >= 1920)
                {
                    data.width = 1920;
                    data.height = 1080;
                }
                else if (res.width >= 1600)
                {
                    data.width = 1600;
                    data.height = 900;
                }
                else if (res.width >= 1366)
                {
                    data.width = 1366;
                    data.height = 768;
                }
                else if (res.width >= 1280)
                {
                    data.width = 1280;
                    data.height = 720;
                }
            }
        }

        var str = JsonUtility.ToJson(data, true);
        //Debug.LogFormat("產生預設設定檔案");

        JDebug.Log($"CreateDefaultOption Default setting: {str}");
        return data;
    }

    public static bool HasDebugFile()
    {
        if (File.Exists(Application.dataPath + System.IO.Path.DirectorySeparatorChar + "debug.txt"))
            return true;
        return false;
    }

    public event UnityAction OnPreferenceChange;

    public virtual void Initialize()
    {
        m_preference = Load();
        m_isDebugMode = HasDebugFile();
    }

    public virtual Preference Load()
    {
        JDebug.Log($"Load game setting");
        /*
		 * 如果遊戲資料夾沒有DtreamingAssets資料夾，自動創建一個
		 */
        if (Directory.Exists(Application.streamingAssetsPath) == false)
        {
            JDebug.Log($"Create streaming assets folder");
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }

        Preference data = null;
        if (File.Exists(settingFilePath))
        {
            try
            {
                var d = File.ReadAllText(settingFilePath);
                data = JsonUtility.FromJson<Preference>(d);
                JDebug.Log($"GameSetting Init Success load setting file, setting: {JsonUtility.ToJson(data, true)}");
            }
            catch (System.UnauthorizedAccessException)
            {
                data = CreateDefaultPreference();
                Save(data);
                JDebug.Log($"GameSetting Init Load setting failed, error:UnauthorizedAccessException");
            }
        }
        else
        {
            data = CreateDefaultPreference();
            Save(data);
        }
        OnPreferenceChange?.Invoke();
        return data;
    }

    public virtual void Save(Preference data)
    {
        JDebug.Log($"Save game setting");
        try
        {
            var j = JsonUtility.ToJson(data, true);
            File.WriteAllText(settingFilePath, j);
            JDebug.Log($"GameSetting Save Save setting success, setting:{j}");
//#if UNITY_EDITOR
//            UnityEditor.AssetDatabase.Refresh();
//#endif
        }
        catch (System.UnauthorizedAccessException)
        {
            JDebug.Log($"GameSetting Save Save setting failed, error: UnauthorizedAccessException");
        }
        m_preference = data;
        OnPreferenceChange?.Invoke();
    }
}
