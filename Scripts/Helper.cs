using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Threading;
using System.Threading.Tasks;

static class Extension
{
    private static System.Random rng = new System.Random();

    public static bool HasIndex<T>(this IEnumerable<T> list, int index)
    {
        if (index < list.Count()) return true;
        return false;
    }

    public static T HasIndexElseNull<T>(this IEnumerable<T> list, int index)
    {
        if (index < list.Count()) return list.ElementAt(index);
        return default(T);
    }

    public static T Last<T>(this IList<T> list)
    {
        return list[list.Count - 1];
    }

    public static T First<T>(this IList<T> list)
    {
        return list[0];
    }


    public static IList<T> Shuffle<T>(this IList<T> list)
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
        return list;
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

    public static bool Contains(this string[] array, string key)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].Equals(key))
                return true;
        }
        return false;
    }

    public static bool Contains(this string[] array, string[] keys)
    {
        foreach (var i in keys)
        {
            if (array.Contains(i))
                return true;
        }
        return false;
    }

    public static bool Contains<T>(this T[] array, T key)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (EqualityComparer<T>.Default.Equals(array[i], key))
                return true;
        }
        return false;
    }

    public static bool Toggle(this CanvasGroup group)
    {
        group.alpha = group.interactable ? 0f : 1f;
        group.interactable = group.blocksRaycasts = !group.interactable;
        return group.interactable;
    }

    public static bool Toggle(this CanvasGroup canvasGroup, bool value)
    {
        canvasGroup.alpha = value ? 1f : 0f;
        canvasGroup.interactable = canvasGroup.blocksRaycasts = value;
        return canvasGroup.interactable;
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
        //Debug.LogFormat($"<color=red>{cells.Count}</Color>");
#endif
        return cells.GetRange(0, count);
    }

    public static class Color
    {
        public static UnityEngine.Color zinc => new UnityEngine.Color(161f / 255f, 161f / 255f, 170f / 255f);
        public static UnityEngine.Color neutral => new UnityEngine.Color(163f / 255f, 163f / 255f, 163f / 255f);
        public static UnityEngine.Color stone => new UnityEngine.Color(168f / 255f, 162f / 255f, 158f / 255f);
        public static UnityEngine.Color orange => new UnityEngine.Color(251f / 255f, 146f / 255f, 60f / 255f);
        public static UnityEngine.Color amber => new UnityEngine.Color(251f / 255f, 191f / 255f, 36f / 255f);
        public static UnityEngine.Color lime => new UnityEngine.Color(163f / 255f, 230f / 255f, 53f / 255f);
        public static UnityEngine.Color emerald => new UnityEngine.Color(52f / 255f, 211f / 255f, 153f / 255f);
        public static UnityEngine.Color teal => new UnityEngine.Color(45f / 255f, 212f / 255f, 191f / 255f);
        public static UnityEngine.Color sky => new UnityEngine.Color(56f / 255f, 189f / 255f, 248f / 255f);
        public static UnityEngine.Color indigo => new UnityEngine.Color(129f / 255f, 140f / 255f, 248f / 255f);
        public static UnityEngine.Color violet => new UnityEngine.Color(167f / 255f, 139f / 255f, 250f / 255f);
        public static UnityEngine.Color purple => new UnityEngine.Color(192f / 255f, 132f / 255f, 252f / 255f);
        public static UnityEngine.Color fuchsia => new UnityEngine.Color(232f / 255f, 121f / 255f, 249f / 255f);
        public static UnityEngine.Color pink => new UnityEngine.Color(244f / 255f, 114f / 255f, 182f / 255f);
        public static UnityEngine.Color rose => new UnityEngine.Color(251f / 255f, 113f / 255f, 133f / 255f);
    }

    public static class Async
    {
        public static async Task Yield(int num)
        {
            for (int i = 0; i < num; i++)
            {
                await Task.Yield();
            }
        }

        public static async Task WaitWhile(Func<bool> condition, int frequencyInFrame = 1, int timeoutInSec = -1)
        {
            var waitTask = Task.Run(async () =>
            {
                while (condition()) await Yield(frequencyInFrame);
            });

            if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeoutInSec)))
                throw new TimeoutException();
        }

        public static async Task WaitUntil(Func<bool> condition, int frequencyInFrame = 1, int timeoutInSec = -1)
        {
            var waitTask = Task.Run(async () =>
            {
                while (!condition()) await Task.Delay(frequencyInFrame);
            });

            if (waitTask != await Task.WhenAny(waitTask,
                    Task.Delay(timeoutInSec)))
                throw new TimeoutException();
        }
    }
}

public static class JDebug
{
    public static void Log(string context)
    {
        Log("DEF", context, Color.gray);
    }
    public static void Log(string tag, string context, Color color)
    {
        var hex = ColorUtility.ToHtmlStringRGB(color);
        var b = new StringBuilder($"[{Time.frameCount}][<b><color=white>{tag.ToUpper()}</color></b>]<color=#{hex}>{context}</color>");
        Debug.Log(b);
    }

    public static string ListToLog<T>(IList<T> list)
    {
        var str = string.Empty;
        foreach (var i in list)
            str += i.ToString() + ", ";
        return str;
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
public class KeyedObjects<T> : ObservableDictionary<string, T>
{
    [SerializeField]
    protected KeyedObject<T>[] m_values;
    protected bool m_isInit = false;
    void Init()
    {
        if (m_isInit) return;
        for (int i = 0; i < m_values.Length; i++)
            this.Add(m_values[i].key, m_values[i].value);
        m_isInit = true;
    }

    public new bool ContainsKey(string key)
    {
        Init();
        return base.ContainsKey(key);
    }

    public bool ContainsKey(params string[] keys)
    {
        foreach (var k in keys)
        {
            if (this.ContainsKey(k) == false) return false;
        }
        return true;
    }

    public new bool ContainsValue(T value)
    {
        Init();
        return base.ContainsValue(value);
    }

    protected override T GetValue(string key)
    {
        Init();
        return base.GetValue(key);
    }

    protected override void SetValue(string key, T value)
    {
        Init();
        base.SetValue(key, value);
    }
}

[System.Serializable]
public class KeyedUIButton : KeyedElement
{
    [SerializeField]
    protected Button m_button;
    public Button button => m_button;
}

public class ValueChangedEvent
{
    public ValueChangedEvent(bool isAdd, float prev, float now, float max)
    {
        this.isAdd = isAdd;
        this.prevValue = prev;
        this.nowValue = now;
        this.maxValue = max;
    }
    public bool isAdd { get; protected set; }
    public float prevValue { get; protected set; }
    public float nowValue { get; protected set; }
    public float maxValue { get; protected set; }
}

public interface IBodyParts
{
    KeyedObjects<Transform> bodyParts { get; }
}