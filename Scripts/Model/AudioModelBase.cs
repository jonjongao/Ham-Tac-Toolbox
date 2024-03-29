﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HamTac
{
    public class AudioModelBase : MonoBehaviour
    {

    }

    [System.Serializable]
    public class AudioPointer
    {
        //public EventDispatcher.Type listenEvent;
        public bool useEnum;
        public string key;
        public bool enable;
        public AudioClip clip;
    }

    public enum AudioChannel
    {
        BGM, SFX, Master
    }


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(AudioPointer))]
    public class AudioPointerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var eventEnum = property.FindPropertyRelative("listenEvent");
            var strKeyProp = property.FindPropertyRelative("key");
            var strKeyString = strKeyProp.stringValue ?? "MISSING_KEY";
            var boolUseEnum = property.FindPropertyRelative("useEnum");
            var objClip = property.FindPropertyRelative("clip");

            if (boolUseEnum.boolValue)
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(eventEnum.enumDisplayNames[eventEnum.enumValueIndex]));
            else
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(strKeyString));

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var rectEnable = new Rect(position.x, position.y, 15, position.height);
            var rectMode = new Rect(position.x + 15, position.y, 50, position.height);
            var rectKey = new Rect(position.x + 65, position.y, 115, position.height);
            var rectClip = new Rect(position.x + 180, position.y, position.width - 180, position.height);

            EditorGUI.PropertyField(rectEnable, property.FindPropertyRelative("enable"), GUIContent.none);
            var mode = new string[] { "Key", "Enum" };
            var idxMode = boolUseEnum.boolValue ? 1 : 0;
            idxMode = EditorGUI.Popup(rectMode, idxMode, mode);
            boolUseEnum.boolValue = idxMode == 1 ? true : false;

            if (boolUseEnum.boolValue)
            {
                try
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.PropertyField(rectKey, eventEnum, GUIContent.none);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (string.IsNullOrEmpty(strKeyProp.stringValue))
                            strKeyProp.stringValue = eventEnum.enumNames[eventEnum.enumValueIndex];
                    }
                }
                catch (System.NullReferenceException)
                {
                    Debug.LogError($"EventEnum is null");
                    boolUseEnum.boolValue = false;
                    return;
                }
            }
            else
            {
                EditorGUI.PropertyField(rectKey, strKeyProp, GUIContent.none);
            }
            EditorGUI.PropertyField(rectClip, objClip, GUIContent.none);
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
#endif
}