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
        Canvas m_activeCameraCanvas;
        public Canvas activeCameraCanvas { get => m_activeCameraCanvas; set => m_activeCameraCanvas = value; }

        [SerializeField]
        Camera m_globalCamera;
        public Camera globalCamera => m_globalCamera;
        [SerializeField]
        Camera m_activeCamera;
        public Camera activeCamera
        {
            get => m_activeCamera ?? m_globalCamera;
            set
            {
                m_activeCamera = value;
                activeCameraCanvas.worldCamera = m_activeCamera;
            }
        }

        [SerializeField]
        protected bool m_isFreeze;
        /// <summary>
        /// 單純只有isFreeze
        /// </summary>
        public static bool IS_FREEZE => current.m_isFreeze;
        public static bool IS_LOCKED
        {
            get
            {
                return !m_current.CanInteract();
            }
        }
        protected virtual bool CanInteract()
        {
            var isBusy=current.m_isFreeze||current.m_isPause||current.m_isLoading;
            return !isBusy;
        }

        [SerializeField]
        protected bool m_isPause;
        public static bool IS_PAUSE => current.m_isPause;

        [SerializeField]
        protected bool m_isLoading;
        public static bool IS_LOADING => current.m_isLoading;
        public static bool IS_UNLOADING => current.m_isUnloading;

        [SerializeField]
        protected string m_currentScene;
        public string currentScene => m_currentScene;

        public static void SetFreeze(bool value) { current.m_isFreeze = value; JDebug.Log($"Freeze", $"Set freeze:{value}", Color.red); }

        public static bool HAS_LMB;

        [SerializeField]
        protected LoadingScreen m_loadingScreen;
        [SerializeField]
        protected GameObject m_bootScreen;

        protected virtual void Awake()
        {
            SetFreeze(false);
            activeCamera = m_globalCamera;
        }

        protected virtual void OnDestroy() { }


        protected virtual void Start()
        {
        }

        protected virtual void Update()
        {
            if (IS_FREEZE && Time.frameCount % 60 == 0)
                JDebug.Log($"Global freeze is on");
            HAS_LMB = Input.GetButtonDown("Fire1");
#if UNITY_EDITOR
            //if (Input.GetKeyDown(KeyCode.Tab))
            //{
            //    Cursor.visible = !Cursor.visible;
            //}
#endif
        }

        public void ExitGame()
        {
            JDebug.Log($"ExitGame");
            Application.Quit();
        }

        protected virtual async Task<bool> LoadSceneAsync(string id, bool showProgressBar, bool disableLoadingScreenOnComplete, float min = 0f, float max = 1f)
        {
            JDebug.W($"LoadSceneAsync id:{id}");
            var op = SceneManager.LoadSceneAsync(id, LoadSceneMode.Additive);
            op.allowSceneActivation = false;
            await OnSceneLoading(op, showProgressBar, disableLoadingScreenOnComplete, min, max);
            op.allowSceneActivation = true;
            //await Task.Yield();
            //!如果等待時間不夠，會有下一個場景要求的reference發生NullException問題
            await Task.Delay(1000);

            if (IS_FREEZE) SetFreeze(false);
            return true;
        }

        protected bool m_isUnloading;

        protected virtual async Task<bool> UnloadSceneAsync(string id, bool showProgressBar, bool disableLoadingScreenOnComplete, float min = 0f, float max = 1f)
        {
            if(m_isUnloading)
            {
                Debug.LogError($"Another unload task is already running");
                return false;
            }
            m_isUnloading = true;
            JDebug.W($"UnloadScene.0");
            Debug.Log($"Try unload scene:{id}");
            var op = SceneManager.UnloadSceneAsync(id);
            if(op!=null)
                op.allowSceneActivation = false;
            JDebug.W($"UnloadScene.1");
            await OnSceneLoading(op, showProgressBar, disableLoadingScreenOnComplete, min, max);
            if(op!=null)
                op.allowSceneActivation = true;
            JDebug.W($"UnloadScene.2");
            m_currentScene = string.Empty;
            await Task.Yield();
            m_isUnloading=false;
            return true;
        }

        async Task<bool> OnSceneLoading(AsyncOperation op, bool showProgressBar, bool disableLoadingScreenOnComplete, float min = 0f, float max = 1f)
        {
            if (m_loadingScreen)
                m_loadingScreen.gameObject.SetActive(true);
            float beginTime = Time.time;
            float minDuration = 1f;
#if UNITY_EDITOR
            minDuration = 0.5f;
            while (true)
            {
                var n = (Time.time - beginTime) / minDuration;
                if (showProgressBar && m_loadingScreen)
                    m_loadingScreen.SetProgress(min + (n * (max - min)));
                if (n >= 1f && (op!=null? op.progress >= 0.9f:true))
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
            if (m_loadingScreen)
                m_loadingScreen.gameObject.SetActive(!disableLoadingScreenOnComplete);
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