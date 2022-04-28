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
        var nav = DebugMenu.BeginVGroup("nav");
        var p1 = DebugMenu.BeginVScrollGroup("p1");
        var p3 = DebugMenu.BeginVScrollGroup("p3");
        var p4 = DebugMenu.BeginVScrollGroup("p4");
        var p11 = DebugMenu.BeginVScrollGroup("p11");
        var p12 = DebugMenu.BeginVScrollGroup("p12");
        var p13 = DebugMenu.BeginVScrollGroup("p13");
        var p14 = DebugMenu.BeginVScrollGroup("p14");


        nav.AddButton("Lv1", () => { p1.group.Toggle(); });
        nav.AddButton("Lv2", () => { p3.group.Toggle(); });
        nav.AddButton("Lv3", () => { p4.group.Toggle(); });
        nav.AddButton("Lv4", () => { p11.group.Toggle(); });
        nav.AddButton("功能性", () => { p12.group.Toggle(); });
        nav.AddButton("", () => { p13.group.Toggle(); });
        nav.AddButton("", () => { p14.group.Toggle(); });

        DebugMenu.OnUpdate(() =>
                {
                    if (Input.GetKeyDown(KeyCode.BackQuote))
                    {
#if !PROD
                        if (DebugMenu.Toggle("nav") == false)
                        {
                            DebugMenu.Toggle("p1", false);

                            DebugMenu.Toggle("p3", false);
                            DebugMenu.Toggle("p4", false);

                            DebugMenu.Toggle("p11", false);
                            DebugMenu.Toggle("p12", false);
                            DebugMenu.Toggle("p13", false);
                            DebugMenu.Toggle("p14", false);
                        }
#endif
                    }
                });

        nav.group.Toggle(false);
        p1.group.Toggle(false);
        p11.group.Toggle(false);
        p12.group.Toggle(false);
        p13.group.Toggle(false);
        p14.group.Toggle(false);
        p3.group.Toggle(false);
        p4.group.Toggle(false);
    }

    private void OnDestroy()
    {
        DebugMenu.Destroy();
    }
}