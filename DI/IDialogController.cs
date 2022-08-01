using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public interface IDialogController
{
    public enum Callback
    {
        DialogBegin, DialogEnd, DialogStop
    }
    public event UnityAction<Callback> OnChange;

    void Initialize();
    void OnDialogEnd();
    void OnDialogBegin();
    void StartDialog(string id, UnityAction onComplete);

    void ForcedStartDialog(string id, UnityAction onComplete);
}
