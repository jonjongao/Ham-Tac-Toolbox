using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using HamTac;
using UnityEngine.UI;

public class VirtualPointer : MonoBehaviour
{
    private static List<VirtualPointer> m_virtualPointer = new List<VirtualPointer>();

    public static VirtualPointer current
    {
        get { return m_virtualPointer.Count > 0 ? m_virtualPointer[0] : null; }
        set
        {
            int index = m_virtualPointer.IndexOf(value);

            if (index > 0)
            {
                m_virtualPointer.RemoveAt(index);
                m_virtualPointer.Insert(0, value);
            }
            else if (index < 0)
            {
                Debug.LogError("Failed setting EventSystem.current to unknown EventSystem " + value);
            }
        }
    }

    [SerializeField]
    float m_actualMouseMagnitude;
    [SerializeField]
    Vector3 m_actualMousePositionLastFrame;
    public bool mouseIsUsing => m_actualMouseMagnitude > 0.1f;

    [SerializeField]
    float m_axisMultiplier = 1000f;
    [SerializeField]
    bool m_clampPosition = true;

    [SerializeField]
    Vector2 m_virtualPosition;
    public Vector3 mousePosition => m_virtualPosition;

    [SerializeField]
    Image m_highlightImage;
    [SerializeField]
    Animation m_pointerAnimation;
    [SerializeField]
    RectTransform m_pivot;
    [SerializeField]
    string m_altLeftMouseButton = "Fire1";
    [SerializeField]
    string m_altRightMouseButton = "Fire2";

    private void OnEnable()
    {
        m_virtualPointer.Add(this);
        //JDebug.W($"resolution:{Screen.currentResolution} screen:{Screen.width} area:{Screen.mainWindowDisplayInfo.width}");
        var p = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        m_pivot.anchoredPosition = p;
        m_actualMousePositionLastFrame = Input.mousePosition;
        m_actualMouseMagnitude = 0f;
        m_virtualPosition = p;
    }

    private void OnDisable()
    {
        m_virtualPointer.Remove(this);
    }

    void Update()
    {
        if (EventSystem.current && EventSystem.current.currentInputModule.inputOverride is not CustomBaseInput)
            EventSystem.current.currentInputModule.inputOverride = EventSystem.current.GetComponent<CustomBaseInput>();
        m_actualMouseMagnitude = (Input.mousePosition - m_actualMousePositionLastFrame).sqrMagnitude;

        if (mouseIsUsing)
        {
            m_virtualPosition = Input.mousePosition;
        }
        else
        {
            var axis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (axis.magnitude > 0.1f)
                m_virtualPosition = m_pivot.anchoredPosition + (axis * m_axisMultiplier) * Time.deltaTime;
        }

        if (m_clampPosition)
        {
            m_virtualPosition.x = Mathf.Clamp(m_virtualPosition.x, 0f, Screen.width);
            m_virtualPosition.y = Mathf.Clamp(m_virtualPosition.y, 0f, Screen.height);
        }

        if (Input.GetButtonDown(m_altLeftMouseButton))
        {
            m_highlightImage.rectTransform.anchoredPosition = m_virtualPosition;
            if (m_pointerAnimation.isPlaying)
                m_pointerAnimation.Stop();
            m_pointerAnimation.Play("PointerDown");
        }

        m_pivot.anchoredPosition = m_virtualPosition;
        if (EventSystem.current && EventSystem.current.currentInputModule.inputOverride is CustomBaseInput)
        {
            (EventSystem.current.currentInputModule.inputOverride as CustomBaseInput).SetMousePosition(m_virtualPosition);
        }

        m_actualMousePositionLastFrame = Input.mousePosition;
    }
}