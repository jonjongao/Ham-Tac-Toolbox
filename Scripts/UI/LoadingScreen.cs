using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HamTac
{
    public class LoadingScreen : MonoBehaviour
    {
        CanvasGroup m_group;
        [SerializeField]
        ProgressBar m_progressBar;

        private void Awake()
        {
            m_group = GetComponent<CanvasGroup>();
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            //JDebug.W($"Enable {gameObject.name}");
            SetProgress(0f);
            m_group.Toggle(true);
        }

        private void OnDisable()
        {
            //JDebug.W($"Disable {gameObject.name}");
            m_group.Toggle(false);
        }

        public void SetProgress(float value)
        {
            if (gameObject.activeSelf == false)
                gameObject.SetActive(true);
            //JDebug.W($"Set loading progress:{value}");
            m_progressBar.SetNormalize(value);
        }
    }
}