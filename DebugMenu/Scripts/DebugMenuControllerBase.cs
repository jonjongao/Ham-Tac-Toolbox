using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class DebugMenuControllerBase : MonoBehaviour
{
    protected Canvas m_canvas;
    public Canvas canvas => m_canvas;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        DebugMenu.Update();
    }

    void Init()
    {
        m_canvas = GetComponentInChildren<Canvas>() ?? DebugMenu.BeginCanvas(transform).GetComponentInChildren<Canvas>();

        DebugMenu.Init(m_canvas);

        CreateMenu();
    }

    protected virtual void CreateMenu() { }

    private void OnDestroy()
    {
        DebugMenu.Destroy();
    }
}