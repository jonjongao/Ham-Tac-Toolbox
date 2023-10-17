using HamTac;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasSizeHandler : MonoBehaviour
{
    [SerializeField]
    CanvasScaler m_scaler;
    [SerializeField]
    GameConfig m_config;
    [SerializeField]
    bool m_updateOnResolutionChange;

    private void Awake()
    {
        m_scaler = GetComponent<CanvasScaler>();
    }

    private void Start()
    {
        m_config.OnConfigChange += M_config_OnConfigChange;
    }

    private void OnDestroy()
    {
        m_config.OnConfigChange -= M_config_OnConfigChange;
    }

    private void M_config_OnConfigChange()
    {
        var c = m_config.runningConfig;
        if(m_updateOnResolutionChange)
        {
            if(m_scaler)
            {
                m_scaler.referenceResolution = c.resolution;
            }
        }
    }
}
