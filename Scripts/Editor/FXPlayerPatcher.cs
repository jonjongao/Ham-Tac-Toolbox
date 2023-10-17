using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class FXPlayerPatcher : EditorWindow
{
    [MenuItem("Tools/HacTac/FXPlayer Patcher")]
    public static void ShowWindow()
    {
        var window = GetWindow(typeof(FXPlayerPatcher));
        window.titleContent.text = "FXPlayer Patcher";
    }

    public GameObject[] m_skills;
    public GameObject[] m_players;

    public GameObject[] m_outputs;

    private void OnGUI()
    {
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty skills = serializedObject.FindProperty("m_skills");
        SerializedProperty players = serializedObject.FindProperty("m_players");
        SerializedProperty outputs = serializedObject.FindProperty("m_outputs");
        EditorGUILayout.PropertyField(skills);
        EditorGUILayout.PropertyField(players);
        EditorGUILayout.PropertyField(outputs);

        if (GUILayout.Button("Clear"))
        {
            skills.ClearArray();
            players.ClearArray();
        }

        if (GUILayout.Button("Find Missing AudioClip"))
        {
            m_players = m_skills.Select(x => x.GetComponentInChildren<AbilityFxPlayer>()).Distinct()
                .Where(x =>
                {
                    return AudioClipNeedPatch(x);
                }).Select(x=>x.gameObject).ToArray();
        }

        if (GUILayout.Button("Find Incorrect Duration"))
        {
            m_players = m_skills.Select(x => x.GetComponentInChildren<AbilityFxPlayer>()).Distinct()
                .Where(x =>
                {
                    return DurationNeedPatch(x);
                }).Select(x => x.gameObject).ToArray();
        }

        if (GUILayout.Button("Find incorrect AudioClip"))
        {
            m_players = m_skills.Select(x => x.GetComponentInChildren<AbilityFxPlayer>()).Distinct()
                .Where(x =>
                {
                    return ParticleContainAudioClip(x);
                }).Select(x => x.gameObject).ToArray();
        }

        if (GUILayout.Button("Find Text Need Localize"))
        {
            m_players = m_skills.SelectMany(x => x.GetComponentsInChildren<TextMeshProUGUI>()).Distinct()
                .Where(x =>
                {
                    return TextNeedPatch(x);
                }).Select(x => x.gameObject).ToArray();
        }

        if (GUILayout.Button("Patch Text"))
        {
            m_players = m_skills.SelectMany(x => x.GetComponentsInChildren<TextMeshProUGUI>()).Distinct()
                .Where(x =>
                {
                    return PatchText(x);
                }).Select(x => x.gameObject).ToArray();
        }

        if (GUILayout.Button("More Then 1 Localize"))
        {
            m_players = m_skills.SelectMany(x => x.GetComponentsInChildren<TextMeshProUGUI>()).Distinct()
                .Where(x =>
                {
                    return MoreLoc(x);
                }).Select(x => x.gameObject).ToArray();
        }

        if (GUILayout.Button("TestAllBattleSkirmish"))
        {
            var allBattle=SkirmishTableCluster.GET_ALL_DATA();
            foreach(var i in allBattle)
            {
                var pass = i.TestInSafe();
                if(!pass)
                {
                    Debug.LogError($"Skirmish:{i.id} not pass test");
                }
            }
        }

        if (GUILayout.Button("Check all skill prefab reference"))
        {
            var allSkill = SkillTableCluster.LIST_ALL().Where(x => SkillPrefabIsIncorrect(x));
            foreach (var i in allSkill)
            {
                Debug.Log($"Fail to check:{i.name}");
            }
        }

        if(GUILayout.Button("Check Localize font asset"))
        {
            var localize = GameObject.FindObjectsOfType<I2.Loc.Localize>(true)
                .Where(x=>x.mLocalizeTargetName.Equals("I2.Loc.LocalizeTarget_TextMeshPro_UGUI") ||
                          x.mLocalizeTargetName.Equals("I2.Loc.LocalizeTarget_UnityUI_Text"))
                .Where(x=>x.mTermSecondary.Equals("DynamicFont")==false);
            m_outputs=localize.Select(x=>x.gameObject).ToArray();
        }

        serializedObject.ApplyModifiedProperties();
    }


    bool SkillPrefabIsIncorrect(SkillTable tbl)
    {
        if (tbl.based.controller == null)
            return true;
        var nameDiff = false;
        if (tbl.based.controller && tbl.based.controller.name != tbl.name)
            nameDiff = true;
        return nameDiff;
    }

    bool AudioClipNeedPatch(AbilityFxPlayer player)
    {
        if (player == null)
            return false;
        if (string.IsNullOrEmpty(player.fxOnStart.sfxKey) == false && player.fxOnStart.sfxClip == null)
            return true;
        if (string.IsNullOrEmpty(player.mainFx.sfxKey) == false && player.mainFx.sfxClip == null)
            return true;
        if (string.IsNullOrEmpty(player.fxOnStop.sfxKey) == false && player.fxOnStop.sfxClip == null)
            return true;
        return false;
    }

    bool DurationNeedPatch(AbilityFxPlayer player)
    {
        if (player == null)
            return false;
        if (player.fxOnStart.fx != null && player.fxOnStart.fixedDuration <= 0f)
            return true;
        if (player.mainFx.fx != null && player.mainFx.fixedDuration <= 0f)
            return true;
        if (player.fxOnStop.fx != null && player.fxOnStop.fixedDuration <= 0f)
            return true;
        return false;
    }

    bool ParticleContainAudioClip(AbilityFxPlayer player)
    {
        if (player == null)
            return false;
        if (player.fxOnStart.fx != null && player.fxOnStart.fx.GetComponentsInChildren<AudioSource>().Length>0)
            return true;
        if (player.mainFx.fx != null && player.mainFx.fx.GetComponentsInChildren<AudioSource>().Length > 0)
            return true;
        if (player.fxOnStop.fx != null && player.fxOnStop.fx.GetComponentsInChildren<AudioSource>().Length > 0)
            return true;
        return false;
    }

    bool TextNeedPatch(TextMeshProUGUI text)
    {
        var loc = text.GetComponent<I2.Loc.Localize>();
        if(loc!=null && loc.SecondaryTerm.Equals("DynamicFont")==false)
        {
            return true;
        }
        return loc == null;
    }

    bool PatchText(TextMeshProUGUI text)
    {
        var loc = text.GetComponent<I2.Loc.Localize>();
        if (loc != null)
        {
            if (loc.SecondaryTerm.Equals("DynamicFont") == false)
            {
                loc.SecondaryTerm = "DynamicFont";
                EditorUtility.SetDirty(loc);
            }
        }
        else
        {
            loc=text.gameObject.AddComponent<I2.Loc.Localize>();
            loc.SecondaryTerm = "DynamicFont";
            EditorUtility.SetDirty(loc);
        }
        
        return true;
    }

    bool MoreLoc(TextMeshProUGUI text)
    {
        var loc = text.GetComponents<I2.Loc.Localize>();
        return loc.Length>1;
    }
}
