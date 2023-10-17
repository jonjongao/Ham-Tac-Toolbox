using HamTac;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BootScreen : MonoBehaviour
{
    [SerializeField]
    SlidePopup m_slide;

    private async void OnEnable()
    {
        m_slide.Toggle(true);
        await Extension.Async.Delay(3f);
        m_slide.OnClickNext();
    }

    private void OnDisable()
    {
        m_slide.OnClickClose();
    }
}
