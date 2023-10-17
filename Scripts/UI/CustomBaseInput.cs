using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine.UI;

namespace UnityEngine.EventSystems
{
    /// <summary>
    /// Interface to the Input system used by the BaseInputModule. With this it is possible to bypass the Input system with your own but still use the same InputModule. For example this can be used to feed fake input into the UI or interface with a different input system.
    /// </summary>
    public class CustomBaseInput : BaseInput
    {
        [SerializeField]
        GameObject m_currentSelected;

        [Button]
        public void PingCurrentSelected()
        {
#if UNITY_EDITOR
            var obj = EventSystem.current.currentSelectedGameObject;
            if(obj)
            {
                EditorGUIUtility.PingObject(obj);
            }
#endif
        }

        /// <summary>
        /// Interface to Input.mousePosition. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public override Vector2 mousePosition
        {
            get { return m_altInputMousePositon; }
        }

        [SerializeField]
        private Vector2 m_altInputMousePositon;
        public void SetMousePosition(Vector2 position)
        {
            m_altInputMousePositon = position;
        }

        [SerializeField]
        string m_altLeftMouseButton = "Fire1";
        [SerializeField]
        string m_altRightMouseButton = "Fire2";

        public override bool GetMouseButton(int button)
        {
            //return base.GetMouseButton(button);
            if (button == 0)
                return Input.GetButton(m_altLeftMouseButton);
            else if (button == 1)
                return Input.GetButton(m_altRightMouseButton);
            return false;
        }

        public override bool GetMouseButtonDown(int button)
        {
            if (button == 0)
                return Input.GetButtonDown(m_altLeftMouseButton);
            else if (button == 1)
                return Input.GetButtonDown(m_altRightMouseButton);
            return false;
        }

        public override bool GetMouseButtonUp(int button)
        {
            if (button == 0)
                return Input.GetButtonUp(m_altLeftMouseButton);
            else if (button == 1)
                return Input.GetButtonUp(m_altRightMouseButton);
            return false;
        }
    }
}
