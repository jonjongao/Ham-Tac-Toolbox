using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpFrom : MonoBehaviour
{
    [SerializeField]
    Transform[] m_from;

    private void Update()
    {
        var totalX = 0f;
        var totalY = 0f;
        foreach (var i in m_from)
        {
            totalX += i.transform.position.x;
            totalY += i.transform.position.y;
        }
        var centerX = totalX / m_from.Length;
        var centerY = totalY / m_from.Length;

        transform.position = new Vector3(centerX, centerY, transform.position.z);
    }
}
