using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using HamTac;
using System.Linq;
using UnityEngine.Events;
using System.Threading.Tasks;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectExtended : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField]
    Vector2 m_contentInitializePos;
    [SerializeField]
    ScrollRect m_scrollRect;
    public ScrollRect scrollRect => m_scrollRect;
    [SerializeField]
    int m_autoSnapWidth = -1;
    [SerializeField]
    int m_autoSnapHeight = -1;
    [SerializeField]
    int m_snapColCount = -1;
    [SerializeField]
    int m_snapRowCount = -1;
    [SerializeField]
    int m_snapRowOffset = 0;
    public int snapRowOffset => m_snapRowOffset;
    [SerializeField]
    bool m_isDown;
    bool m_isRmbDown;

    public UnityEvent<Vector2> OnSnap;

    [SerializeField]
    Vector3 m_dragDelta;
    [SerializeField]
    Vector3 m_downPosition;

    public UnityEvent OriginalPointerRelease;

    private void Awake()
    {
        //m_scrollRect = GetComponent<ScrollRect>();
    }

    private void OnEnable()
    {
        m_scrollRect.content.anchoredPosition = m_contentInitializePos;
        m_snapColCount = GetComponentsInChildren<HorizontalLayoutGroup>().Length;
        var allCol = m_scrollRect.content.GetComponentsInChildren<VerticalLayoutGroup>();
        if (allCol.Length > 0)
        {
            m_snapRowCount = allCol.OrderByDescending(x=>x.transform.childCount).FirstOrDefault().transform.childCount;
        }
    }

    private void Update()
    {
        if(m_isRmbDown)
        {
            m_dragDelta = m_downPosition - Input.mousePosition;
        }
    }

    public void SetAutoSnap(int width, int height)
    {
        m_autoSnapWidth = width;
        m_autoSnapHeight = height;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
       
        if(m_dragDelta.magnitude<100f)
        {
            OriginalPointerRelease?.Invoke();
        }
        m_isDown = false;
        m_isRmbDown = false;
        if (m_autoSnapWidth > 0)
        {
            var side = Mathf.Sign(m_scrollRect.content.anchoredPosition.x);
            var diff = Mathf.Abs(m_scrollRect.content.anchoredPosition.x % m_autoSnapWidth);
            var num = Mathf.RoundToInt(Mathf.Abs(m_scrollRect.content.anchoredPosition.x / m_autoSnapWidth));
            //Debug.Log($"diff:{diff} num:{num} side:{side}");
            if (diff >= 0)
            {
                var newPos = new Vector2(m_autoSnapWidth * num, 0) * new Vector2(side, 1);
                //m_scrollRect.content.anchoredPosition = new Vector2(m_autoSnapWidth * num, 0) * new Vector2(side, 1);

                var ySide = Mathf.Sign(m_scrollRect.content.anchoredPosition.y);
                var yDiff = Mathf.Abs(m_scrollRect.content.anchoredPosition.y % m_autoSnapHeight);
                var yNum = Mathf.RoundToInt(m_scrollRect.content.anchoredPosition.y / m_autoSnapHeight);
                //Debug.Log($"ydiff:{yDiff} ynum:{yNum} yside:{ySide}");
                yNum = Mathf.Clamp(yNum, m_snapRowOffset, m_snapRowCount + m_snapRowOffset - 1);
                newPos.y = m_autoSnapHeight * yNum;

                m_scrollRect.content.DOAnchorPos(newPos, 0.25f).OnComplete(() =>
                {
                    OnSnap?.Invoke(newPos);
                });
                //OnSnap?.Invoke(newPos);
            }
        }
    }

    public Vector3 CalcSnapPosition(Vector2 orgPos)
    {
        var side = Mathf.Sign(orgPos.x);
        var diff = Mathf.Abs(orgPos.x % m_autoSnapWidth);
        var num = Mathf.RoundToInt(Mathf.Abs(orgPos.x / m_autoSnapWidth));
        var newPos = new Vector2(m_autoSnapWidth * num, 0) * new Vector2(side, 1);
        var ySide = Mathf.Sign(orgPos.y);
        var yDiff = Mathf.Abs(orgPos.y % m_autoSnapHeight);
        var yNum = Mathf.RoundToInt(orgPos.y / m_autoSnapHeight);
        //Debug.Log($"ydiff:{yDiff} ynum:{yNum} yside:{ySide}");
        yNum = Mathf.Clamp(yNum, m_snapRowOffset, m_snapRowCount + m_snapRowOffset - 1);
        newPos.y = m_autoSnapHeight * yNum;
        return newPos;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        m_downPosition = Input.mousePosition;
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            m_isDown = true;
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            m_isRmbDown = true;
        }
    }
}
