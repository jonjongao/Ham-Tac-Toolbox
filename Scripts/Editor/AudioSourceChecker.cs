using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TbsFramework.EditorUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public class AudioSourceChecker : EditorWindow
{
    [MenuItem("Tools/HacTac/Audio Source Checker")]
    public static void ShowWindow()
    {
        var window = GetWindow(typeof(AudioSourceChecker));
        window.titleContent.text = "Audio Source Checker";
    }

    public DefaultAsset folder;
    public string folderPath;
    public GameObject[] targets;
    public AudioSource[] foundAudioSource;
    public AudioMixerGroup targetMixerGroup;

    private void OnGUI()
    {
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty folder_prop = serializedObject.FindProperty("folder");
        SerializedProperty folder_path_prop = serializedObject.FindProperty("folderPath");
        SerializedProperty targets_prop = serializedObject.FindProperty("targets");
        SerializedProperty audiosource_prop = serializedObject.FindProperty("foundAudioSource");
        SerializedProperty mixer_group_prop = serializedObject.FindProperty("targetMixerGroup");
        EditorGUILayout.PropertyField(folder_prop);
        EditorGUILayout.PropertyField(folder_path_prop);
        EditorGUILayout.PropertyField(mixer_group_prop);
        EditorGUILayout.PropertyField(targets_prop, true);
        EditorGUILayout.PropertyField(audiosource_prop, true);

        if (GUILayout.Button("Fetch all AudioSource"))
        {
            var path = AssetDatabase.GetAssetPath(folder);
            string[] subfolders = AssetDatabase.GetSubFolders(path);
            var allPath = subfolders.Concat(new[] { path }).ToArray();
            JDebug.Q($"Path:{JDebug.ListToLog(allPath.ToList())}");
            folderPath = path;
            string[] loadedGameObjects = AssetDatabase.FindAssets("t:prefab", allPath);
            var allObjPath = loadedGameObjects.Select(x => AssetDatabase.GUIDToAssetPath(x)).ToArray();
            foreach (string guid in loadedGameObjects)
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                Debug.Log($"Found object:{p}");
            }
            GameObject[] fetchObj = allObjPath.Select(x => AssetDatabase.LoadAssetAtPath<GameObject>(x)).ToArray();
            targets = fetchObj;
            //targets = loadedGameObjects.Where(x=>x is GameObject).Select(x=>x as GameObject).ToArray();
            List<AudioSource> list = new List<AudioSource>();
            foreach (var i in targets)
            {
                var a = i.GetComponentsInChildren<AudioSource>();
                foreach (var aa in a)
                    list.Add(aa);
            }
            foundAudioSource = list.ToArray();
        }
        if (GUILayout.Button("Patch all audio output"))
        {
            foreach (var i in foundAudioSource)
            {
                i.outputAudioMixerGroup = targetMixerGroup;
                Debug.Log($"Done patch:{i.name}");
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
