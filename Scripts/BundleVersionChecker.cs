using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System.Linq;

#if UNITY_EDITOR
[InitializeOnLoad]
public class BundleVersionChecker
{
    /// <summary>
    /// Class name to use when referencing from code.
    /// </summary>
    const string ClassName = "CurrentBundleVersion";

    const string TargetCodeFile = "Assets/" + ClassName + ".cs";

    static BundleVersionChecker()
    {
        string lastVersion = CurrentBundleVersion.version;
        var timeMark = System.DateTime.Now.ToString("yyMMddHH");
        /*
         * 大版號
         * 小版號 （當天的build次數）
         * 日期
         */
        var mainVersion = PlayerSettings.bundleVersion;
        var newVersion = $"version {mainVersion} build {timeMark}";

        //PlayerSettings.bundleVersion = newVersion;

        //string bundleVersion = PlayerSettings.bundleVersion;
        Debug.Log($"Last version:{lastVersion} new version:{newVersion}");

        if (lastVersion != newVersion)
        {
            Debug.Log("Found new bundle version " + newVersion + " replacing code from previous version " + lastVersion + " in file \"" + TargetCodeFile + "\"");
            CreateNewBuildVersionClassFile(newVersion);
        }
    }

    static string CreateNewBuildVersionClassFile(string bundleVersion)
    {
        using (StreamWriter writer = new StreamWriter(TargetCodeFile, false))
        {
            try
            {
                string code = GenerateCode(bundleVersion);
                writer.WriteLine("{0}", code);
            }
            catch (System.Exception ex)
            {
                string msg = " threw:\n" + ex.ToString();
                Debug.LogError(msg);
                EditorUtility.DisplayDialog("Error when trying to regenerate class", msg, "OK");
            }
        }
        return TargetCodeFile;
    }

    /// <summary>
    /// Regenerates (and replaces) the code for ClassName with new bundle version id.
    /// </summary>
    /// <returns>
    /// Code to write to file.
    /// </returns>
    /// <param name='bundleVersion'>
    /// New bundle version.
    /// </param>
    static string GenerateCode(string bundleVersion)
    {
        string code = "public static class " + ClassName + "\n{\n";
        code += $"\tpublic static readonly string version = \"{bundleVersion}\";";
        code += "\n}\n";
        return code;
    }
}
#endif