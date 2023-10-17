using HamTac;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InputControlModule : MonoBehaviour
{
    public static UnityAction OnCameraMoveStart;
    public static UnityAction OnCameraMoveEnd;
    public static UnityAction OnCameraZoomStart;
    public static UnityAction OnCameraZoomEnd;
}

public class StandaloneControlModule : InputControlModule
{
    bool rmbDown => Input.GetMouseButtonDown(1);
    bool rmbUp => Input.GetMouseButtonUp(1);
    bool rmbHeld => Input.GetMouseButton(1);
    [SerializeField]
    Vector3 lastDownPosition;
    Vector3 dragDelta;
    [SerializeField]
    Vector3 dragDirection;
    [SerializeField]
    float dragLength = 50f;
    [SerializeField]
    float sensitivity = 0.5f;
    [SerializeField]
    float dragPower;

    [SerializeField]
    float m_scrollSpeed = 5f;
    float m_scrollMultiply = -1f;
    bool m_isScrolling;

    private void Update()
    {
        if (CutSceneController.current)
        {
            if (CutSceneController.current.isPlaying)
                return;
        }

        if (rmbDown)
        {
            lastDownPosition = Input.mousePosition;
            OnCameraMoveStart?.Invoke();
        }

        if (rmbUp)
        {
            lastDownPosition = Vector3.zero;
            dragDirection = Vector3.zero;
            OnCameraMoveEnd?.Invoke();
        }

        if (rmbHeld)
        {
            var d = lastDownPosition - Input.mousePosition;
            dragDirection = d / dragLength;
            dragPower = dragDirection.magnitude / dragLength;

            if (CameraController.current)
            {
                CameraController.current.MoveToward(dragDirection, sensitivity * dragPower);
            }
        }

        if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.001f)
        {
            if (CameraController.current)
            {
                CameraController.current.ShiftDistance((Input.mouseScrollDelta.y * m_scrollMultiply) * m_scrollSpeed);
            }
            OnCameraZoomStart?.Invoke();
            m_isScrolling = true;
        }
        else if (m_isScrolling)
        {
            OnCameraZoomEnd?.Invoke();
            m_isScrolling = false;
        }
    }
}
