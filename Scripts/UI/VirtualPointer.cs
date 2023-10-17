using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using HamTac;
using UnityEngine.UI;
using System.Linq;

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
    [SerializeField]
    bool isMouseMode;
    [SerializeField]
    Vector2 m_screenAxis;
    public Vector2 screenAxis => m_screenAxis;

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
        if (Input.GetKey(KeyCode.Joystick1Button0) ||
            Input.GetKey(KeyCode.Joystick1Button1) ||
            Input.GetKey(KeyCode.Joystick1Button2) ||
            Input.GetKey(KeyCode.Joystick1Button3))
        {
            isMouseMode = false;
        }

        if (EventSystem.current && EventSystem.current.currentInputModule.inputOverride is not CustomBaseInput)
            EventSystem.current.currentInputModule.inputOverride = EventSystem.current.GetComponent<CustomBaseInput>();

        m_actualMouseMagnitude = (Input.mousePosition - m_actualMousePositionLastFrame).sqrMagnitude;

        if (mouseIsUsing)
        {
            m_virtualPosition = Input.mousePosition;
            isMouseMode = true;
        }
        else
        {
            var axis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (axis.magnitude > 0.1f)
                m_virtualPosition = m_pivot.anchoredPosition + (axis * m_axisMultiplier) * Time.deltaTime;
        }
        m_screenAxis = new Vector2(((Screen.width * -0.5f) + m_virtualPosition.x) / (Screen.width * 0.5f),
                       ((Screen.height * -0.5f) + m_virtualPosition.y) / (Screen.height * 0.5f));

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

    private void FixedUpdate()
    {
        if (isMouseMode) return;
        SimulateClick();
    }

    RaycastHit[] m_rayHits = new RaycastHit[5];
    RaycastHit[] m_nearest = new RaycastHit[1];

    List<Transform> m_rayHitTransforms = new List<Transform>();

    void SimulateClick()
    {
        if (GameManager.current.activeCamera == null) return;
        var cam = GameManager.current.activeCamera;
        var ray = cam.ScreenPointToRay(m_virtualPosition);
        Debug.DrawRay(ray.origin, ray.direction * 500f, Color.magenta);
        Physics.RaycastNonAlloc(ray, m_rayHits, 500);
        m_rayHits.OrderBy(x => Vector3.Distance(cam.transform.position, x.transform.position));
        m_nearest[0] = m_rayHits[0];


        for (int i = 0; i < m_rayHitTransforms.Count; i++)
        {
            if (m_rayHitTransforms[i] == null || !m_nearest.Any(x => x.transform == m_rayHitTransforms[i].transform))
            {
                if (m_rayHitTransforms[i] != null)
                    m_rayHitTransforms[i].transform.SendMessage("OnMouseExit", SendMessageOptions.DontRequireReceiver);
                m_rayHitTransforms.RemoveAt(i);
            }
        }

        foreach (var i in m_nearest)
        {
            if (i.transform == null) continue;
            if (m_rayHitTransforms.Contains(i.transform) == false)
            {
                i.transform.SendMessage("OnMouseEnter", SendMessageOptions.DontRequireReceiver);
                m_rayHitTransforms.Add(i.transform);
            }
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            foreach (var i in m_nearest)
            {
                if (i.collider == null || i.transform == null) continue;
                i.transform.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);
            }
        }
        if (Input.GetKeyUp(KeyCode.Joystick1Button0))
        {
            foreach (var i in m_nearest)
            {
                if (i.collider == null || i.transform == null) continue;
                i.transform.SendMessage("OnMouseUp", SendMessageOptions.DontRequireReceiver);
            }
        }

    }
}