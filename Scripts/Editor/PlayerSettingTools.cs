using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using HamTac;

public class PlayerSettingTools : EditorWindow
{
    [MenuItem("Tools/HacTac/Player Setting Tools")]
    public static void ShowWindow()
    {
        var window = GetWindow(typeof(PlayerSettingTools));
        window.titleContent.text = "Player Setting Tools";
    }

    public VersionSetting version;
    public string preview;
    string m_addSymbol;
    int m_selectedSymbol;
    public List<string> m_bulkDefineSymbols = new List<string>();

    [System.Serializable]
    public class VersionSetting
    {
        public string format = "Ver {0} Build {1}";
        public int major;
        public int minor;
        public int patch;
        //public string build;
        public string buildFormat = "yyMMddHHmm";

        public string[] customDefineSymbol;

        public string FormVersion()
        {
            var bui = System.DateTime.Now.ToString(buildFormat);
            //build= bui;
            var ver = $"{major}.{minor}.{patch}";
            var text = string.Format(format, ver, bui);
            return text;
        }
    }

    static VersionSetting ReadVersion()
    {
        var path = Application.dataPath + "/version.json";
        if (File.Exists(path))
        {
            var t = File.ReadAllText(path);
            var obj = JsonUtility.FromJson<VersionSetting>(t);
            return obj;
        }
        return new VersionSetting();
    }

    static void SaveVersion(VersionSetting version)
    {
        var path = Application.dataPath + "/version.json";
        var json = JsonUtility.ToJson(version, true);
        if (File.Exists(path))
        {
            File.WriteAllText(path, json);
            return;
        }
        var f = File.CreateText(path);
        f.Write(json);
        f.Close();
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    private void OnFocus()
    {
        if (version == null)
            version = ReadVersion();
        preview = version.FormVersion();
    }

    private void OnGUI()
    {
        SerializedObject obj = new SerializedObject(this);

        SerializedProperty versionProp = obj.FindProperty("version");
        EditorGUILayout.PropertyField(versionProp);

        #region BuildVersionizeTool


        //SerializedProperty buildMarkProp = obj.FindProperty("buildMark");
        //SerializedProperty buildMarkFormatProp = obj.FindProperty("buildMarkFormat");

        //SerializedProperty formatProp = obj.FindProperty("format");
        GUILayout.Label($"Build Versionize Tool", EditorStyles.whiteLargeLabel);
        SerializedProperty previewProp = obj.FindProperty("preview");
        //EditorGUILayout.PropertyField(buildMarkProp);
        //EditorGUILayout.PropertyField(buildMarkFormatProp);
        EditorGUILayout.PropertyField(previewProp, new GUIContent("Format Preview"));
        //EditorGUILayout.PropertyField(formatProp);

        EditorGUILayout.LabelField($"Version Marks");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("-Major"))
        {
            version.major--;
            SavePreview();
        }
        if (GUILayout.Button("+Major"))
        {
            version.major++;
            SavePreview();
        }
        if (GUILayout.Button("-Minor"))
        {
            version.minor--;
            SavePreview();
        }
        if (GUILayout.Button("+Minor"))
        {
            version.minor++;
            SavePreview();
        }
        if (GUILayout.Button("-Patch"))
        {
            version.patch--;
            SavePreview();
        }
        if (GUILayout.Button("+Patch"))
        {
            version.patch++;
            SavePreview();
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.LabelField($"Define Symbol");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+PROD"))
        {
            AddDefineSymbols("PROD");
        }
        if (GUILayout.Button("-PROD"))
        {
            RemoveDefineSymbols("PROD");
        }
        if (GUILayout.Button("+DEMO"))
        {
            AddDefineSymbols("DEMO");
        }
        if (GUILayout.Button("-DEMO"))
        {
            RemoveDefineSymbols("DEMO");
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.LabelField($"Current Version", PlayerSettings.bundleVersion);

        GUILayout.BeginHorizontal();
        //if (GUILayout.Button("Form"))
        //{
        //    var v = version.FormVersion();
        //    preview = v;
        //    SaveVersion(version);
        //}
        GUI.color = Color.green;
        if (GUILayout.Button("Apply to bundleVersion"))
        {
            PlayerSettings.bundleVersion = preview;
            SaveVersion(version);
        }
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
        #endregion

        EditorGUILayout.Space();

        #region DefineSymbolTool
        GUILayout.Label($"Define Symbol Tool", EditorStyles.whiteLargeLabel);

        //SerializedProperty bulkProp = obj.FindProperty("m_bulkDefineSymbols");
        //EditorGUILayout.PropertyField(bulkProp);

        EditorGUILayout.LabelField($"Add New Option");
        GUILayout.BeginHorizontal();
        m_addSymbol = GUILayout.TextField(m_addSymbol);
        if (GUILayout.Button("Add"))
        {
            if (version.customDefineSymbol == null)
                version.customDefineSymbol = new string[0];
            version.customDefineSymbol = version.customDefineSymbol.Append(m_addSymbol).ToArray();
            SaveVersion(version);
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.LabelField($"Add to Bulk Action List");
        GUILayout.BeginVertical();
        if (version.customDefineSymbol != null && version.customDefineSymbol.Length > 0)
        {
            var selected = EditorGUILayout.Popup("Custom Define Symbol", m_selectedSymbol, version.customDefineSymbol);
            if (selected != m_selectedSymbol)
            {
                m_bulkDefineSymbols.Add(version.customDefineSymbol[selected]);
            }
            m_selectedSymbol = selected;
        }

        if (m_bulkDefineSymbols.Count > 0)
        {
            foreach (var i in m_bulkDefineSymbols)
            {
                if (ExistingDefineSymbol(i))
                {
                    GUI.color = Color.yellow;
                }
                else
                {
                    GUI.color = Color.green;
                }
                GUILayout.Box(i);
            }
            GUI.color = Color.white;

            if (GUILayout.Button($"Clear Bulk List"))
            {
                m_bulkDefineSymbols.Clear();
            }
            GUILayout.BeginHorizontal();
            GUI.color = Color.red;
            if (GUILayout.Button($"Remove Defines"))
            {
                RemoveDefineSymbols(m_bulkDefineSymbols.ToArray());
                m_bulkDefineSymbols.Clear();
            }
            GUI.color = Color.green;
            if (GUILayout.Button($"Add Defines"))
            {
                AddDefineSymbols(m_bulkDefineSymbols.ToArray());
                m_bulkDefineSymbols.Clear();
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.Box($"No Bulk Action");
        }

        GUILayout.EndVertical();

        #endregion

        obj.ApplyModifiedProperties();
    }



    void SavePreview()
    {
        var v = version.FormVersion();
        preview = v;
        SaveVersion(version);
    }

    public static bool ExistingDefineSymbol(string symbol)
    {
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        List<string> allDefines = definesString.Split(';').ToList();
        return allDefines.Contains(symbol);
    }

    public static void AddDefineSymbols(params string[] symbols)
    {
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        List<string> allDefines = definesString.Split(';').ToList();
        allDefines.AddRange(symbols.Except(allDefines));
        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup,
            string.Join(";", allDefines.ToArray()));
    }

    public static void RemoveDefineSymbols(params string[] symbols)
    {
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        List<string> allDefines = definesString.Split(';').ToList();
        allDefines.RemoveAll(x => symbols.Contains(x));
        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup,
            string.Join(";", allDefines.ToArray()));
    }
}
