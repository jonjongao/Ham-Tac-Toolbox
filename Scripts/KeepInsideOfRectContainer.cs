using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepInsideOfRectContainer : MonoBehaviour
{
    [SerializeField]
    RectTransform container;
    [SerializeField]
    Vector2 m_pivotOffset;
    
    Vector3[] cornersCache = new Vector3[4];
    RectTransform movable => transform as RectTransform;

    private void Update()
    {
        Vector2 pos = VirtualPointer.current.mousePosition;
        movable.anchoredPosition = pos + m_pivotOffset;

        // RectTransform container, movable;
        // Vector3[] cornersCache = new Vector3[4];
        container.GetWorldCorners(cornersCache);
        // BL = Bottom Left, TR = Top Right (corners)
        Vector3 containerBL = cornersCache[0], containerTR = cornersCache[2];
        var containerSize = containerTR - containerBL; // NEW
        movable.GetWorldCorners(cornersCache);
        Vector3 movableBL = cornersCache[0], movableTR = cornersCache[2];
        var movableSize = movableTR - movableBL; // NEW

        var position = movable.position;
        Vector3 deltaBL = position - movableBL, deltaTR = movableTR - position;
        position.x = movableSize.x < containerSize.x // NEW
          ? Mathf.Clamp(position.x, containerBL.x + deltaBL.x, containerTR.x - deltaTR.x)
          : Mathf.Clamp(position.x, containerTR.x - deltaTR.x, containerBL.x + deltaBL.x); // NEW
        position.y = movableSize.y < containerSize.y // NEW
          ? Mathf.Clamp(position.y, containerBL.y + deltaBL.y, containerTR.y - deltaTR.y)
          : Mathf.Clamp(position.y, containerTR.y - deltaTR.y, containerBL.y + deltaBL.y); // NEW
        movable.position = position;
    }
}
