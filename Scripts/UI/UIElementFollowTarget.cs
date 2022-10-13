using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIElementFollowTarget : MonoBehaviour
{
    [SerializeField]
    Transform m_target;
    public Transform target => m_target;

    [SerializeField]
    Camera m_camera;

    [SerializeField]
    Vector2 m_offset;

    [SerializeField]
    Vector2 m_lastPosition;

    [SerializeField]
    RectTransform m_pointerContainer;

    private void Awake()
    {
        var rt = transform as RectTransform;
        rt.anchorMin = rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    private void OnEnable()
    {
        Refresh(true);
    }

    public void SetTarget(Transform target, Camera camera, float clippingSize = 1f)
    {
        JDebug.W($"Try set target:{target} {camera} {clippingSize}");
        m_target = target;
        m_camera = camera;
        transform.localScale = Vector3.one * clippingSize;
        transform.DOPunchScale(new Vector3(-0.3f, -0.3f, 0.3f), 0.25f, 1);
        Refresh(true);
    }
    public void ClearTarget()
    {
        m_target = null;
    }

    private void Update()
    {
        if (m_target == null) return;

        Refresh();
    }

    void Refresh(bool isForced = false)
    {
        if (m_target != null)
        {
            var p = RectTransformUtility.WorldToScreenPoint(
                m_camera,
                m_target.transform.position);

            if (isForced == true || (p - m_lastPosition).magnitude > 5f)
            {
                (transform as RectTransform).anchoredPosition = p + m_offset;
                m_lastPosition = p;
            }
        }
    }

    public void SetPointerScale(float value)
    {
        m_pointerContainer.localScale = Vector3.one * value;
    }
}
