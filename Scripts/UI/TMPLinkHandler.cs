using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class TMPLinkHandler : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
{
    [SerializeField]
    private TMP_Text m_text;
    [SerializeField]
    private Canvas m_canvasToCheck;
    [SerializeField]
    private Camera m_cameraToUse;

    public static event UnityAction<TMP_LinkInfo> OnAnyLinkHover;
    public static event UnityAction<TMP_LinkInfo> OnAnyLinkExit;

    TMP_LinkInfo m_lastHoverLink;
    int m_lastTaggedIndex = -1;

    private void OnDisable()
    {
        ExitLink();
    }

    private void Awake()
    {
        m_text = GetComponent<TMP_Text>();
        m_canvasToCheck = GetComponentInParent<Canvas>();

        if (m_canvasToCheck.renderMode == RenderMode.ScreenSpaceOverlay)
            m_cameraToUse = null;
        else
            m_cameraToUse = m_canvasToCheck.worldCamera;
    }


    public void OnPointerMove(PointerEventData eventData)
    {
        Vector3 mousePosition = new Vector3(eventData.position.x, eventData.position.y, 0);

        var linkTaggedText = TMP_TextUtilities.FindIntersectingLink(m_text, mousePosition, m_cameraToUse);
        if (linkTaggedText == -1)
        {
            if(m_lastTaggedIndex != linkTaggedText)
            {
                ExitLink();
            }
           
            return;
        }
        m_lastTaggedIndex = linkTaggedText;

        TMP_LinkInfo linkInfo = m_text.textInfo.linkInfo[linkTaggedText];

        string linkID = linkInfo.GetLinkID();
        //Debug.Log($"TMPLinkHandler hover link id:{linkID} hash:{linkInfo.hashCode}");
        if (linkInfo.hashCode != 0 &&
            m_lastHoverLink.hashCode != linkInfo.hashCode)
        {
            if (m_lastHoverLink.hashCode != 0)
            {
                ExitLink();
            }
            //Debug.Log($"TMPLinkHandler hover a new link id:{linkID} hash:{linkInfo.hashCode}");
            m_lastHoverLink= linkInfo;
        }

        OnAnyLinkHover?.Invoke(m_lastHoverLink);
        if (linkID.StartsWith("http://") || linkID.StartsWith("https://"))
        {
            //Application.OpenURL(linkID);
            return;
        }

    }

    void ExitLink()
    {
        if (m_lastHoverLink.hashCode == 0)
            return;
        //Debug.Log($"TMPLinkHandler exit link id:{m_lastHoverLink.GetLinkID()} hash:{m_lastHoverLink.hashCode}");
        OnAnyLinkExit?.Invoke(m_lastHoverLink);
        m_lastHoverLink = new TMP_LinkInfo();
        m_lastTaggedIndex = -1;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
       
    }
}
