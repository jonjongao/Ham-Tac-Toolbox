using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
static class ExtensionsClass
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static KeyedElement Find(this IList<KeyedElement> list, string key)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].key.Equals(key))
                return list[i];
        }
        return null;
    }

    public static bool Contains(this IList<KeyedElement> list, string key)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].key.Equals(key))
                return true;
        }
        return false;
    }

    public static bool Toggle(this CanvasGroup canvasGroup, bool value)
    {
        canvasGroup.alpha = value ? 1f : 0f;
        canvasGroup.interactable = canvasGroup.blocksRaycasts = value;
        return value;
    }

    public static bool IsOn(this CanvasGroup canvasGroup)
    {
        return canvasGroup.alpha > 0.5f;
    }

    public static string ToString(this Array arr)
    {
        if (arr == null)
            return "null";
        else
            return "{" + string.Join(", ", arr.Cast<object>().Select(o => o.ToString()).ToArray()) + "}";
    }

    public static string ToString<TKey, TValue>(this Dictionary<TKey, TValue> dict)
    {
        if (dict == null)
            return "null";
        else
            return "{" + string.Join(", ", dict.Select(kvp => kvp.Key.ToString() + ":" + kvp.Value.ToString()).ToArray()) + "}";
    }

    public static Bounds EncapsulateAll(this Bounds[] array)
    {
        var b = new Bounds(array[0].center, array[0].size);
        foreach (var a in array)
            b.Encapsulate(a);
        return b;
    }

    public static Mesh GenerateMeshFromBounds(this Bounds bounds)
    {
        var b = bounds;
        b.center = Vector3.zero;
        var mesh = new Mesh();
        var vert = new Vector3[]
        {
            b.min,
            b.max,
            new Vector3(b.min.x, b.min.y, b.max.z),
            new Vector3(b.min.x, b.max.y, b.min.z),
            new Vector3(b.max.x, b.min.y, b.min.z),
            new Vector3(b.min.x, b.max.y, b.max.z),
            new Vector3(b.max.x, b.min.y, b.max.z),
            new Vector3(b.max.x, b.max.y, b.min.z),
        };
        mesh.vertices = vert;
        mesh.triangles = new[]
         {
             0,7,4,
             0,3,7,
             5,1,3,
             3,1,7,
             7,1,4,
             4,1,6,
             5,3,2,
             2,3,0,
             0,4,2,
             2,4,6,
             1,5,2,
             6,1,2
         };
        Vector2[] uvs = new Vector2[vert.Length];
        for (int i = 0; i < uvs.Length; i++)
            uvs[i] = new Vector2(vert[i].x, vert[i].z);
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }

    public static List<T> ShuffleThenGetRange<T>(this List<T> cells, int count)
    {
        cells.Shuffle();
#if UNITY_EDITOR
        Debug.LogFormat($"<color=red>{cells.Count}</Color>");
#endif
        return cells.GetRange(0, count);
    }
}

public static class JDebug
{
    public static void Log(string context)
    {
        Log("Default", context, Color.gray);
    }
    public static void Log(string tag, string context, Color color)
    {
        var hex = ColorUtility.ToHtmlStringRGB(color);
        var b = new StringBuilder($"[{Time.frameCount}][{tag}]<color={hex}>{context}</color>");
        Debug.Log(b);
    }
}

[System.Serializable]
public class KeyedElement
{
    [SerializeField]
    protected string m_key;
    public string key => m_key;
}

[System.Serializable]
public class KeyedObject<T>
{
    [SerializeField]
    protected string m_key;
    public string key => m_key;

    [SerializeField]
    protected T m_value;
    public T value => m_value;
}

[System.Serializable]
public class KeyedObjects<T>
{
    [SerializeField]
    protected List<KeyedObject<T>> m_values = new List<KeyedObject<T>>();
    public List<KeyedObject<T>> values => m_values;

    public bool HasKey(string key)
    {
        foreach (var i in m_values)
            if (i.key.Equals(key)) return true;
        return false;
    }

    public KeyedObject<T> Find(string key)
    {
        foreach (var i in m_values)
            if (i.key.Equals(key)) return i;
        return null;
    }
}

[System.Serializable]
public class KeyedUIButton : KeyedElement
{
    [SerializeField]
    protected Button m_button;
    public Button button => m_button;
}