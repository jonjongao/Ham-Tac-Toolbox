using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class BatchEventTrigger : MonoBehaviour
{
    [SerializeField]
    [TextArea]
    string m_notes;

    public UnityEvent OnTrigger;

    public void DoTrigger() { OnTrigger?.Invoke(); }
}
