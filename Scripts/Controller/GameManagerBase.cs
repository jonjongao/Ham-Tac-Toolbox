using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HamTac
{
    [DefaultExecutionOrder(-9000)]
    public class GameManagerBase : MonoBehaviour
    {
        protected static GameManagerBase m_current;
        public static GameManagerBase current
        {
            get
            {
                if (m_current == null)
                    m_current = FindObjectOfType<GameManagerBase>();
                return m_current;
            }
            set => m_current = value;
        }

        [SerializeField]
        Canvas m_globalOverlayCanvas;
        public Canvas globalOverlayCanvas { get => m_globalOverlayCanvas; set => m_globalOverlayCanvas = value; }

        [SerializeField]
        Camera m_globalCamera;
        public Camera globalCamera => m_globalCamera;

        [SerializeField]
        protected bool m_isFreeze;
        public static bool IS_FREEZE => current.m_isFreeze;
        public static bool IS_LOCKED
        {
            get
            {
                return current.m_isFreeze;
            }
        }

        [SerializeField]
        bool m_isPause;
        public static bool IS_PAUSE => current.m_isPause;

        [SerializeField]
        public string m_currentScene;

        public static void SetFreeze(bool value) { current.m_isFreeze = value; JDebug.Log($"Freeze", $"Set freeze:{value}", Color.red); }

        public static bool HAS_LMB;

        [SerializeField]
        protected LoadingScreen m_loadingScreen;
        [SerializeField]
        protected GameObject m_bootScreen;

        protected virtual void Awake()
        {
            SetFreeze(false);
        }

        protected virtual void OnDestroy() { }


        protected virtual void Start() { }

        protected virtual void Update()
        {
            if (IS_FREEZE && Time.frameCount % 60 == 0)
                JDebug.Log($"Global freeze is on");
            HAS_LMB = Input.GetButtonDown("Fire1");
        }

        public void ExitGame()
        {
            JDebug.Log($"ExitGame");
            Application.Quit();
        }

        protected virtual async Task<bool> LoadSceneAsync(string id, bool showProgressBar, float min = 0f, float max = 1f)
        {
            var op = SceneManager.LoadSceneAsync(id, LoadSceneMode.Additive);
            op.allowSceneActivation = false;
            await OnSceneLoading(op, showProgressBar, min, max);
            op.allowSceneActivation = true;
            //await Task.Yield();
            //!如果等待時間不夠，會有下一個場景要求的reference發生NullException問題
            await Task.Delay(1000);

            if (IS_FREEZE) SetFreeze(false);
            return true;
        }

        protected virtual async Task<bool> UnloadSceneAsync(string id, bool showProgressBar, float min = 0f, float max = 1f)
        {
            var op = SceneManager.UnloadSceneAsync(id);
            op.allowSceneActivation = false;
            await OnSceneLoading(op, showProgressBar, min, max);
            op.allowSceneActivation = true;
            m_currentScene = string.Empty;
            await Task.Yield();
            return true;
        }

        async Task<bool> OnSceneLoading(AsyncOperation op, bool showProgressBar, float min = 0f, float max = 1f)
        {
            if(m_loadingScreen)
                m_loadingScreen.gameObject.SetActive(true);
            float beginTime = Time.time;
            float minDuration = 1f;
#if UNITY_EDITOR
            minDuration = 0.25f;
            while (true)
            {
                var n = (Time.time - beginTime) / minDuration;
                if (showProgressBar && m_loadingScreen)
                    m_loadingScreen.SetProgress(min + (n * (max - min)));
                if (n >= 1f && op.progress >= 0.9f)
                    break;
                await Task.Yield();
            }
#else
        while(true)
        {
            JDebug.Log($"Progress:{op.progress}");
            if (showProgressBar && m_loadingScreen)
                m_loadingScreen.SetProgress(min + (op.progress * (max - min)));
            if (op.progress >= 0.9f)
                break;
            await Task.Yield();
        }
        await Task.Delay(Mathf.CeilToInt(minDuration * 1000));
#endif
            if(m_loadingScreen)
                m_loadingScreen.gameObject.SetActive(false);
            return true;
        }

        public static bool SKIP_DOWN
        {
            get { return (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)); }
        }

        public static void SetPause(bool value)
        {
            Time.timeScale = value ? 0f : 1f;
            m_current.m_isPause = value;
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GameManagerBase), true)]
    public class GameManagerBaseEditor : Editor
    {
        GameManagerBase current;

        private void OnEnable()
        {
            current = (GameManagerBase)target;
        }
    }
#endif
}