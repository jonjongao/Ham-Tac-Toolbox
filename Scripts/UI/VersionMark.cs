using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Text), typeof(TMPro.TextMeshProUGUI))]
public class VersionMark : UIBehaviour
{
    protected override void OnEnable()
    {
        var g = GetComponent<Graphic>();
        if (g == null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            if (g is Text)
                (g as Text).text = CurrentBundleVersion.version;
            else if (g is TMPro.TextMeshProUGUI)
                (g as TMPro.TextMeshProUGUI).text = CurrentBundleVersion.version;
            else
                gameObject.SetActive(false);
        }
        base.OnEnable();
    }
}
