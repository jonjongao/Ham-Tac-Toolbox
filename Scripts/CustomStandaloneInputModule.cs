using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomStandaloneInputModule : StandaloneInputModule
{
    public GameObject GetHovered()
    {
        var mouseEvent = GetLastPointerEventData(-1);
        if (mouseEvent == null)
            return null;
        return mouseEvent.pointerCurrentRaycast.gameObject;
    }

    public List<GameObject> GetAllHovered()
    {
        var mouseEvent = GetLastPointerEventData(-1);
        if (mouseEvent == null)
            return null;
        return mouseEvent.hovered;
    }

    public bool HasHovered(GameObject value)
    {
        return GetHovered() == value;
    }
}