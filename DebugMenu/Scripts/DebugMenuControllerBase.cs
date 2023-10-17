using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HamTac
{
    public class DebugMenuControllerBase : MonoBehaviour
    {
        protected Canvas m_canvas;
        public Canvas canvas => m_canvas;

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            DebugMenu.Update();
        }

        void Init()
        {
            m_canvas = GetComponentInChildren<Canvas>() ?? DebugMenu.BeginCanvas(transform).GetComponentInChildren<Canvas>();

            DebugMenu.Init(m_canvas);

            CreateMenu();
        }

        protected virtual void CreateMenu() { }

        private void OnDestroy()
        {
            DebugMenu.Destroy();
        }

        protected static (System.Type type, System.Action<object> callback) m_waitMouseDownCallback;

        public static void WAIT_MOUSE_DOWN<T>(System.Action<object> callback)
        {
            m_waitMouseDownCallback = (typeof(T), callback);
        }

        public static void DISPATCH_MOUSE_DOWN<T>(object content)
        {
            if (m_waitMouseDownCallback.type == null || m_waitMouseDownCallback.type != typeof(T))
                return;
            m_waitMouseDownCallback.callback?.Invoke(content);

            m_waitMouseDownCallback = (null, null);
        }
    }
}