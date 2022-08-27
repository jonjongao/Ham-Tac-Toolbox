using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Threading;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HamTac
{
    static class Extension
    {
        private static System.Random rng = new System.Random();

        public static int IndexOf<T>(this T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(value)) return i;
            }
            throw null;
        }

        public static int IndexOf<T>(this IEnumerable source, T value)
        {
            int index = 0;
            var comparer = EqualityComparer<T>.Default; // or pass in as a parameter
            foreach (T item in source)
            {
                if (comparer.Equals(item, value)) return index;
                index++;
            }
            return -1;
        }

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

        public static bool ToBool(this string a)
        {
            if (a.Equals("true") || a.Equals("1"))
                return true;
            else
                return false;
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

        public static T RandomTake<T>(this T[] array)
        {
            return array[UnityEngine.Random.Range(0, array.Length)];
        }

        public static T RandomTake<T>(this IEnumerable<T> list)
        {
            return list.ElementAt(UnityEngine.Random.Range(0, list.Count()));
        }

        public static Vector2[] GetPoints(this BoxCollider2D box, bool includeWorldPosition)
        {
            Vector3 worldPosition = includeWorldPosition ? box.bounds.center : Vector3.zero;
            Vector2 size = new Vector2(box.size.x * box.transform.localScale.x * 0.5f,
                box.size.y * box.transform.localScale.y * 0.5f);
            Vector3 corner1 = new Vector2(-size.x, -size.y);
            Vector3 corner2 = new Vector2(-size.x, size.y);
            Vector3 corner3 = new Vector2(size.x, -size.y);
            Vector3 corner4 = new Vector2(size.x, size.y);
            corner1 = worldPosition + corner1;
            corner2 = worldPosition + corner2;
            corner3 = worldPosition + corner3;
            corner4 = worldPosition + corner4;

            return new Vector2[]
            {
            corner1,corner3,corner4,corner2
            };
        }

        public static async Task OptimizedTakeScreenshotAsync(this MonoBehaviour component, TextureEncode encode, int quality, float scaling, string savePathIncludeExtension)
        {
            var tcs = new TaskCompletionSource<bool>();
            component.StartCoroutine(TakeScreenshot(encode, quality, scaling, savePathIncludeExtension, () => tcs.SetResult(true)));
            await tcs.Task;
        }

        public enum TextureEncode
        {
            JPG, PNG
        }

        static IEnumerator TakeScreenshot(TextureEncode encode, int quality, float scaling, string savePathIncludeExtension, System.Action callback)
        {
            yield return new WaitForEndOfFrame();
            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            var rendered = new Texture2D(texture.width, texture.height);
            rendered.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            rendered.Apply();

            if (scaling < 1f || scaling > 1f)
                ResizeTexture2D(rendered, Mathf.CeilToInt(texture.width * scaling), Mathf.CeilToInt(texture.height * scaling), false);

            byte[] byteArray = new byte[0];
            switch (encode)
            {
                case TextureEncode.PNG:
                    byteArray = rendered.EncodeToPNG();
                    break;
                case TextureEncode.JPG:
                default:
                    byteArray = rendered.EncodeToJPG(quality);
                    break;
            }
            System.IO.File.WriteAllBytes(savePathIncludeExtension, byteArray);
            Debug.Log($"SaveScrenshot:{savePathIncludeExtension}");
            callback?.Invoke();
        }

        public static void ResizeTexture2D(Texture2D texture2D, int targetX, int targetY, bool mipmap = true, FilterMode filter = FilterMode.Bilinear)
        {
            //create a temporary RenderTexture with the target size
            RenderTexture rt = RenderTexture.GetTemporary(targetX, targetY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);

            //set the active RenderTexture to the temporary texture so we can read from it
            RenderTexture.active = rt;

            //Copy the texture data on the GPU - this is where the magic happens [(;]
            Graphics.Blit(texture2D, rt);
            //resize the texture to the target values (this sets the pixel data as undefined)
#if UNITY_2021
            texture2D.Reinitialize(targetX, targetY, texture2D.format, mipmap);
#else
            texture2D.Resize(targetX, targetY, texture2D.format, mipmap);
#endif
            texture2D.filterMode = filter;

            try
            {
                //reads the pixel values from the temporary RenderTexture onto the resized texture
                texture2D.ReadPixels(new Rect(0.0f, 0.0f, targetX, targetY), 0, 0);
                //actually upload the changed pixels to the graphics card
                texture2D.Apply();
            }
            catch
            {
                Debug.LogError("Read/Write is not enabled on texture " + texture2D.name);
            }


            RenderTexture.ReleaseTemporary(rt);
        }

        public static bool IsInLayerMask(this GameObject obj, LayerMask layerMask)
        {
            return ((layerMask.value & (1 << obj.layer)) > 0);
        }

        public static Vector3 ToV3(this Vector2 value)
        {
            return new Vector3(value.x, value.y, 0f);
        }

        public static int ToInt(this string value)
        {
            var v = 0;
            int.TryParse(value, out v);
            return v;
        }

        public static int Repeat(this int value,int length)
        {
            return value > length ? 0 : value;
        }

        public static int PickInRatio(float[] ratios, bool includeNoPick)
        {
            //todo例如30%+60&+100%(皆無) = 190%
            var sum = ratios.Sum() + (includeNoPick ? 1f : 0f);
            for (int i = 0; i < ratios.Length; i++)
            {
                var d = UnityEngine.Random.Range(0f, sum);
                //todo0~190隨機數，若小於30%則中
                if (d < ratios[i])
                {
                    return i;
                }
                //todo沒中則從池中扣除30%，池會越來越小
                //todo即會傾向越來越容易中，如果在[includeNoPick]模式，最終機率最大為皆沒中
                else
                {
                    sum -= ratios[i];
                }
            }
            //todo-1=皆沒中
            return -1;
        }

#if UNITY_EDITOR
        [MenuItem("CONTEXT/BoxCollider2D/Use SpriteRenderer size", false, 3)]
        static void GetSpriteRendererSizeAsColliderSize(MenuCommand menuCommand)
        {
            var collider2d = (BoxCollider2D)menuCommand.context;

            var sprite = collider2d.GetComponent<SpriteRenderer>();
            if (sprite == null) return;
            Debug.Log($"pivot:{sprite.sprite.pivot} rect:{sprite.sprite.rect} " +
                $"toffset:{sprite.sprite.textureRectOffset} bounds:{sprite.sprite.bounds}");
            var pivot = new Vector2(Mathf.Clamp01(sprite.sprite.pivot.x / sprite.sprite.rect.width),
                                    Mathf.Clamp01(sprite.sprite.pivot.y / sprite.sprite.rect.height));

            var offsetX = (sprite.size.x * 0.5f) - (pivot.x * sprite.size.x);
            var offsetY = (sprite.size.y * 0.5f) - (pivot.y * sprite.size.y);
            Debug.Log($"pivot:{pivot} x:{offsetX} y:{offsetY}");
            collider2d.offset = new Vector2(offsetX, offsetY);
            collider2d.size = sprite.size;
        }

        [MenuItem("CONTEXT/BoxCollider2D/Use MeshFilter size", false, 3)]
        static void GetMeshFilterSizeAsColliderSize(MenuCommand menuCommand)
        {
            var collider2d = (BoxCollider2D)menuCommand.context;

            var sprite = collider2d.GetComponent<MeshRenderer>();
            if (sprite == null) return;
            collider2d.size = sprite.bounds.size;
            Debug.Log($"get mesh size:{sprite.bounds.size}");
        }

        [MenuItem("Tools/HacTac/Add Utage define symbol")]
        public static void AddUtageDefineSymbol()
        {
            AddDefineSymbols(new string[] { "UTAGE_INSTALLED" });
        }

        [MenuItem("Tools/HacTac/Remove Utage define symbol")]
        public static void RemoveUtageDefineSymbol()
        {
            RemoveDefineSymbols(new string[] { "UTAGE_INSTALLED" });
        }

        [MenuItem("Tools/HacTac/Add DoTween define symbol")]
        public static void AddDoTweenDefineSymbol()
        {
            AddDefineSymbols(new string[] { "DOTWEEN_INSTALLED" });
        }

        [MenuItem("Tools/HacTac/Remove DoTween define symbol")]
        public static void RemoveDoTweenDefineSymbol()
        {
            RemoveDefineSymbols(new string[] { "DOTWEEN_INSTALLED" });
        }

        static void AddDefineSymbols(string[] symbols)
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = definesString.Split(';').ToList();
            allDefines.AddRange(symbols.Except(allDefines));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray()));
        }

        static void RemoveDefineSymbols(string[] symbols)
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = definesString.Split(';').ToList();
            allDefines.RemoveAll(x => symbols.Contains(x));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray()));
        }
#endif

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
                var begin = Time.time;
                while (condition())
                {
                    if (timeoutInSec > 0f && Time.time - begin > timeoutInSec)
                        break;
                    await Task.Delay(frequencyInFrame);
                }
            }

            public static async Task WaitUntil(Func<bool> condition, int frequencyInFrame = 100, int timeoutInSec = -1)
            {
                var begin = Time.time;
                while (!condition())
                {
                    if (timeoutInSec > 0f && Time.time - begin > timeoutInSec)
                        break;
                    await Task.Delay(frequencyInFrame);
                }
            }

            public static async Task WaitUntil(Func<bool>condition,Func<bool> breaker, int frequencyInFrame = 100, int timeoutInSec = -1)
            {
                var begin = Time.time;
                while (!condition())
                {
                    if (timeoutInSec > 0f && Time.time - begin > timeoutInSec)
                        break;
                    if (Application.isPlaying == false || breaker())
                        break;
                    await Task.Delay(frequencyInFrame);
                }
            }

            public static async void WaitInTimeSkipable(float duration, Func<bool> condition, Action callback = null)
            {
                await WaitInTimeSkipableAsync(duration, condition);
                callback?.Invoke();
            }

            public static async Task WaitInTimeSkipableAsync(float duration, Func<bool> condition)
            {
                var timestamp = Time.time;
                while (Time.time - timestamp < duration)
                {
                    if (Time.time - timestamp > 0.25f && condition()) break;
                    await Task.Yield();
                }
                await Task.Yield();
            }



            public static async Task Delay(float seconds)
            {
                var begin = Time.time;
                while (Time.time - begin < seconds)
                {
                    if (Application.isPlaying == false)
                        break;
                    await Task.Yield();
                }
            }

            public static async Task Delay(float seconds, System.Func<bool> breaker)
            {
                var begin = Time.time;
                while (Time.time - begin < seconds)
                {
                    if (Application.isPlaying == false || breaker())
                        break;
                    await Task.Yield();
                }
            }
        }


    }



    [System.Serializable]
    public class KeyedElement
    {
        [SerializeField]
        protected string m_key;
        public string key => m_key;
    }

    /// <summary>
    /// Soft key relation
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

    /// <summary>
    /// Inherit from dictionary, cannot have duplicate key
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

    /// <summary>
    /// Soft key relation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class KeyedObject<T1,T2>
    {
        [SerializeField]
        protected T1 m_key;
        public T1 key => m_key;

        [SerializeField]
        protected T2 m_value;
        public T2 value => m_value;
    }

    /// <summary>
    /// Inherit from dictionary, cannot have duplicate key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class KeyedObjects<T1, T2> : ObservableDictionary<T1, T2>
    {
        [SerializeField]
        protected KeyedObject<T1,T2>[] m_values;
        protected bool m_isInit = false;
        void Init()
        {
            if (m_isInit) return;
            for (int i = 0; i < m_values.Length; i++)
                this.Add(m_values[i].key, m_values[i].value);
            m_isInit = true;
        }

        public new bool ContainsKey(T1 key)
        {
            Init();
            return base.ContainsKey(key);
        }

        public bool ContainsKey(params T1[] keys)
        {
            foreach (var k in keys)
            {
                if (this.ContainsKey(k) == false) return false;
            }
            return true;
        }

        public new bool ContainsValue(T2 value)
        {
            Init();
            return base.ContainsValue(value);
        }

        protected override T2 GetValue(T1 key)
        {
            Init();
            return base.GetValue(key);
        }

        protected override void SetValue(T1 key, T2 value)
        {
            Init();
            base.SetValue(key, value);
        }
    }

}

public static class JDebug
{
    public static void Q(string context)
    {
        Log(0, string.Empty, context, HamTac.Extension.Color.neutral);
    }
    public static void W(string context)
    {
        Log(1, string.Empty, context, Color.yellow);
    }
    public static void E(string context)
    {
        Log(2, string.Empty, context, Color.red);
    }

    public static void Log(string context)
    {
        Log(0, string.Empty, context, Color.gray);
    }
    public static void Log(string tag, string context, Color color) { Log(0, tag, context, color); }
    public static void Log(int type, string tag, string context, Color color)
    {
        var hex = ColorUtility.ToHtmlStringRGB(color);
        var b = new StringBuilder($"[{Time.frameCount}][<b><color=white>{tag.ToUpper()}</color></b>]<color=#{hex}>{context}</color>");
        switch (type)
        {
            case 1:
                Debug.LogWarning(b); break;
            case 2:
                Debug.LogError(b); break;
            default:
                Debug.Log(b); break;
        }
    }


    public static string ListToLog<T>(IList<T> list)
    {
        if (list == null) return "Null";
        var str = string.Empty;
        foreach (var i in list)
            str += i.ToString() + ", ";
        return str;
    }
}
