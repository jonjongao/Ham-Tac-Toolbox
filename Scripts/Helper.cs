using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HamTac
{
    public static class Extension
    {
        public static class StringHelper
        {
            /// <summary>
            /// 在字串中篩選出小括號「( )」間的值
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static string[] GetValueBetweenRoundBrackets(string input)
            {
                string pattern = @"\((-?\d+,-?\d+)\)";
                //string pattern = @"\{(-?\d+,-?\d+)\}";

                MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(input, pattern);
                string[] output = new string[matches.Count];
                for (int i = 0; i < matches.Count; i++)
                {
                    string value = matches[i].Groups[1].Value;
                    output[i] = value;
                }
                return output;
            }

            /// <summary>
            /// 在字串中篩選出大括號「{ }」間的值
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static string[] GetValueBetweenCurlyBrackets(string input)
            {
                string pattern = @"\{(-?\d+,-?\d+)\}";

                MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(input, pattern);
                string[] output = new string[matches.Count];
                for (int i = 0; i < matches.Count; i++)
                {
                    string value = matches[i].Groups[1].Value;
                    output[i] = value;
                }
                return output;
            }

            /// <summary>
            /// 將字串依拆分符分開，並取頭兩個值轉換為Vector2
            /// </summary>
            /// <param name="input"></param>
            /// <param name="separator"></param>
            /// <param name="output"></param>
            /// <returns></returns>
            public static bool SplitToV2(string input, char separator, out Vector2 output, bool allowEmptyInput, bool asMinMaxRange)
            {
                if (allowEmptyInput && string.IsNullOrEmpty(input))
                {
                    output = Vector2.zero;
                    return true;
                }
                string[] values = input.Split(separator);
                if (values.Length == 1)
                {
                    if (float.TryParse(values[0], out float x))
                    {
                        output = new Vector2(x, x);
                        return true;
                    }
                }
                try
                {
                    //todo如果分解後數量超過2，則強制只取前兩個值
                    if (values.Length > 2)
                        values = values.Take(2).ToArray();
                    if (float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y))
                    {
                        Vector2 vector = Vector2.zero;
                        if (asMinMaxRange)
                        {
                            var min = Math.Min(x, y);
                            var max = Math.Max(x, y);
                            vector = new Vector2(min, max);
                        }
                        else
                            vector = new Vector2(x, y);
                        output = vector;
                        return true;
                    }
                }
                catch (System.Exception err) { }
                Debug.LogError($"Fail convert to Vector2, input:{input}");
                output = Vector2.zero;
                return false;
            }

            public static bool TryGetRandomValueInRange(string input, char separator, out int output)
            {
                if (string.IsNullOrEmpty(input) == false)
                {
                    string[] values = input.Split(separator);
                    if (values.Length == 1)
                    {
                        if (int.TryParse(input, out int o))
                        {
                            output = o;
                            return true;
                        }
                    }
                    else
                    {
                        if (SplitToV2(input, separator, out Vector2 v2, false, true))
                        {
                            var rnd = new System.Random();
                            int min = (int)v2.x;
                            int max = (int)v2.y;
                            var o = rnd.Next(min, max + 1);
                            output = o;
                            return true;
                        }
                    }
                }
                output = 0;
                return false;
            }


        }

        public static int[] GroupOfRandomValueFromRange(Vector2Int[] range, Func<int,bool> sumChecker=null)
        {
            var result=new int[range.Length];
            if(sumChecker==null)
            {
                for (int i = 0; i < range.Length; i++)
                {
                    result[i] = RandomValueFromV2(range[i]);
                    Debug.Log($"Roll value:{result[i]}");
                }
                Debug.Log($"RNG table. success without checker");
                return result;
            }
            else
            {
                var times = 0;
                var sum = 0;
               while(times<100)
                {
                    sum = 0;
                    for (int i = 0; i < range.Length; i++)
                    {
                        var r= RandomValueFromV2(range[i]);
                        sum += r;
                        result[i] = r;
                        Debug.Log($"Roll value:{result[i]} sum:{sum}");
                    }
                    times++;
                    if(sumChecker(sum))
                    {
                        Debug.Log($"RNG table. success calc sum:{sum} times:{times}");
                        return result;
                    }
                }
            }
            Debug.LogError($"RNG table. failed to form");
            return result;
        }

        public static Vector3 GetCentralPosition(IEnumerable<Vector3> value)
        {
            if (value.Count() == 0)
                return Vector3.zero;
            var totalX = 0f;
            var totalY = 0f;
            var totalZ = 0f;
            foreach (var i in value)
            {
                totalX += i.x;
                totalY += i.y;
                totalZ += i.z;
            }
            var centerX = totalX / value.Count();
            var centerY = totalY / value.Count();
            var centerZ = totalZ / value.Count();
            return new Vector3(centerX, centerY, centerZ);
        }

        public static Transform FirstActive(this Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                    return transform.GetChild(i);
            }
            return null;
        }

        public static Transform LastActive(this Transform transform)
        {
            for (int i = transform.childCount - 1; i > 0; i--)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                    return transform.GetChild(i);
            }
            return null;
        }

        public static Vector2Int GetAbsMaxVector2Int(Vector2Int[] value)
        {
            if (value == null || value.Length == 0)
                return Vector2Int.zero;
            try
            {
                var max = value[0];
                float maxMagnitude = 0f;
                foreach (var i in value)
                {
                    float m = i.sqrMagnitude;
                    if (m > maxMagnitude)
                    {
                        max = i;
                        maxMagnitude = m;
                    }
                }
                return max;

            }
            catch (System.Exception err)
            {
                Debug.LogError(err);
                return Vector2Int.zero;
            }
        }

        public static Vector3 GetPositiveLocalScale(Transform transform)
        {
            var worldScale = transform.lossyScale;
            var n = new Vector3(Math.Sign(worldScale.x), Math.Sign(worldScale.y), Math.Sign(worldScale.z));
            var localScale = transform.localScale;
            return new Vector3(localScale.x * n.x, localScale.y * n.y, localScale.z * n.z);
        }

        public static float RandomRatio()
        {
            System.Random random = new System.Random();
            float randomValue = (float)random.NextDouble();
            return randomValue;
        }

        public static int RandomValueFromV2(Vector2Int input, bool includeMax = true)
        {
            var rnd = new System.Random();
            int min = (int)input.x;
            int max = includeMax ? (int)input.y + 1 : (int)input.y;
            var o = rnd.Next(min, max);
            return o;
        }

        public static void CreateInstanceInLength<T>(Transform container, GameObject prefab, ref List<T> list, int length) where T : MonoBehaviour
        {
            if (list.Count < length)
            {
                var patch = length - list.Count;
                for (int i = 0; i < patch; i++)
                {
                    var obj = UnityEngine.Object.Instantiate(prefab, container);
                    var script = obj.GetComponent<T>();
                    list.Add(script);
                }

            }
            for (int i = 0; i < list.Count; i++)
            {
                if (i < length)
                {
                    list[i].gameObject.SetActive(true);
                }
                else
                {
                    list[i].gameObject.SetActive(false);
                }
            }
        }

        public static int IndexOfCol(JToken indexer, JToken token, string key)
        {
            var index = indexer.First.Values<string>().IndexOf(key);
            //JDebug.Log($"Get index of {key}={index}");
            return index;
        }

        public static string StrOfCol(JToken indexer, JToken token, string key)
        {
            var index = IndexOfCol(indexer, token, key);
            if (index < 0) return null;
            var child = token.Children();
            //JDebug.Log($"token:{token} child:{JDebug.ListToLog(child.ToList())}");
            return (string)token.Children().ElementAtOrDefault(index);
        }

        public static HashSet<string> ListStringOfNumber(int from, int to)
        {
            var arr = new HashSet<string>();
            for (int i = from; i <= to; i++)
            {
                arr.Add(i.ToString());
            }
            return arr;
        }

        public static T[] SetArrayInSize<T>(T[] oldArray,int size)
        {
            var newArray=new T[size];
            for (int i = 0; i < Math.Min(oldArray.Length, size); i++)
            {
                newArray[i] = oldArray[i];
            }
            return newArray;
        }

        public static bool HasFormatSection(string text)
        {
            string pattern = @"\{\d+\}";
            return Regex.IsMatch(text, pattern);
        }

        public static int GetStringFormatSectionLength(string text)
        {
            var pattern = @"{(.*?)}";
            var matches = Regex.Matches(text, pattern);
            return matches.Count;
        }
        /// <summary>
        /// 將字串[x,y]轉成Vector2Int(x,y)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Vector2Int ParseV2I(this string value)
        {
            var x = -1;
            var y = -1;
            try
            {
                var arr = value.Split(',');
                x = int.Parse(arr[0]);
                y = int.Parse(arr[1]);
            }
            catch (System.Exception e)
            {
                //Debug.LogError($"Try Parse[{value}] with error, has been resolve");
                //Debug.LogWarning(e);
            }
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// (3, 2) -> 3,2
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ParseV2I(this Vector2Int value)
        {
            return $"{value.x},{value.y}";
        }

        public static bool HasParameter(this Animator animator, string paramName)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName)
                    return true;
            }
            return false;
        }

        public static void CenterOnChildred(this Transform parent)
        {
            var childs = parent.Cast<Transform>();
            var pos = Vector3.zero;
            foreach (var c in childs)
            {
                pos += c.position;
                c.parent = null;
            }
            pos /= childs.Count();
            parent.position = pos;
            foreach (var c in childs)
                c.parent = parent;
        }

        public static string GenerateSimpleGUID()
        {
            return System.Guid.NewGuid().ToString("N");
        }

        private static System.Random rng = new System.Random();

        public static int IndexOf<T>(this T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(value)) return i;
            }
            return -1;
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

        public static bool Contains(this Type[] a, Type b)
        {
            return a.Any(x => x.Equals(b));
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

        public static bool Contains<T>(this T[] array, T[] key)
        {
            for (int a = 0; a < array.Length; a++)
            {
                for (int b = 0; b < key.Length; b++)
                {
                    if (EqualityComparer<T>.Default.Equals(array[a], key[b]))
                        return true;
                }

            }
            return false;
        }

        public static bool Toggle(this CanvasGroup group)
        {
            var isOn = !group.interactable;
            JDebug.Q($"Toggle canvas:{group.name} to:{isOn}");
            group.alpha = isOn ? 1f : 0f;
            group.interactable = group.blocksRaycasts = !group.interactable;
            return group.interactable;
        }

        public static bool Toggle(this CanvasGroup canvasGroup, bool value)
        {
            JDebug.Q($"Toggle canvas:{canvasGroup.name} to:{value}");
            canvasGroup.alpha = value ? 1f : 0f;
            canvasGroup.interactable = canvasGroup.blocksRaycasts = value;
            return canvasGroup.interactable;
        }

        public static void SwitchToIndex(this IEnumerable<GameObject> targets, int index)
        {
            try
            {
                var arr = targets.ToArray();
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i] == null) continue;
                    if (i == index)
                        arr[i].SetActive(true);
                    else
                        arr[i].SetActive(false);
                }
            }
            catch (System.ArgumentOutOfRangeException)
            {
                JDebug.E($"Index:{index} is out of range");
            }
        }

        public static void SwitchToIndex(this List<CanvasGroup> canvasGroupArray, int index)
        {
            try
            {
                for (int i = 0; i < canvasGroupArray.Count; i++)
                {
                    if (canvasGroupArray[i] == null) continue;
                    if (i == index)
                        canvasGroupArray[i].Toggle(true);
                    else
                        canvasGroupArray[i].Toggle(false);
                }
            }
            catch (System.ArgumentOutOfRangeException)
            {
                JDebug.E($"Index:{index} is out of range");
            }
        }

        public static void SwitchToNext(this List<CanvasGroup> canvasGroupArray)
        {
            var index = canvasGroupArray.IndexOf(canvasGroupArray.First(x => x.alpha >= 0.9f));
            var next = Repeat(index + 1, canvasGroupArray.Count());
            canvasGroupArray.SwitchToIndex(next);
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
            System.Random rand = new System.Random();
            int randomIndex = rand.Next(0, list.Count());
            return list.ElementAt(randomIndex);
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

        public static Texture2D TakeScreenFromCamera(Camera captureCamera)
        {
            // Create a render texture with the same dimensions as the camera's viewport
            RenderTexture renderTexture = new RenderTexture(captureCamera.pixelWidth, captureCamera.pixelHeight, 24);

            // Set the render texture as the camera's target texture
            captureCamera.targetTexture = renderTexture;

            // Create a new texture with the same dimensions as the render texture
            Texture2D texture = new Texture2D(captureCamera.pixelWidth, captureCamera.pixelHeight, TextureFormat.RGB24, false);

            // Render the camera's output to the render texture
            captureCamera.Render();

            // Read the pixels from the render texture into the texture
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, captureCamera.pixelWidth, captureCamera.pixelHeight), 0, 0);
            texture.Apply();

            // Reset the camera's target texture and active render texture
            captureCamera.targetTexture = null;
            RenderTexture.active = null;

            // Destroy the render texture
            UnityEngine.Object.Destroy(renderTexture);
            return texture;
        }

        public static async Task OptimizedTakeScreenshotAsync(this MonoBehaviour component, TextureEncode encode, int quality, float scaling, string savePathIncludeExtension)
        {
            var tcs = new TaskCompletionSource<bool>();
            component.StartCoroutine(TakeScreenshot(encode, quality, scaling, savePathIncludeExtension, () => tcs.SetResult(true)));
            await tcs.Task;
        }

        public static Texture2D CopyTexture2D(Texture2D sourceTexture,int width,int height)
        {
            byte[] sourceData = sourceTexture.GetRawTextureData();

            // Create a new Texture2D with the same dimensions and format as the source texture
            Texture2D newTexture = new Texture2D(width, height, sourceTexture.format, false);
            // Load the pixel data into the new texture
            newTexture.LoadRawTextureData(sourceData);

            // Apply changes to the new texture
            newTexture.Apply();
            return newTexture;
        }
        public static Texture2D CopyTexture2D(Texture2D sourceTexture)
        {
            return CopyTexture2D(sourceTexture,sourceTexture.width,sourceTexture.height);
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
            texture2D.Reinitialize(targetX, targetY, texture2D.format, mipmap);
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

        public static float GetMaxDuration(this ParticleSystem value)
        {
            var d = value.main.duration;
            for (int i = 0; i < value.subEmitters.subEmittersCount; i++)
            {
                var sp = value.subEmitters.GetSubEmitterSystem(i);
                if (sp.main.duration > d)
                    d = sp.main.duration;
            }
            return d;
        }

        public static float EstimateMaxParticleDuration(GameObject target)
        {
            var p = target.GetComponent<ParticleSystem>();
            if (p)
                return p.GetMaxDuration();
            return 0f;
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

        public static int Repeat(this int value, int length)
        {
            return value >= length ? 0 : value;
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">ie: /Actor/Icon/Player.psd</param>
        /// <returns></returns>
        public static T FindResources<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                JDebug.E($"Invalid path:{path}", "System");
                return null;
            }
            if (Application.isPlaying)
            {
                path = System.IO.Path.ChangeExtension(path, null).Remove(0, 1);
                var obj = Resources.Load<T>(path) as UnityEngine.Object;
                if (obj == null)
                    JDebug.E($"Cant find resources:{path}", "System");
                return obj as T;
            }
            else
            {
#if UNITY_EDITOR
                path = $"Assets/Resources{path}";
                var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                if (obj == null)
                    JDebug.E($"Cant find asset:{path}", "System");
                return obj;
#endif
            }
            return null;
        }

        public static Vector2Int ToV2I(this Vector2 value)
        {
            return new Vector2Int(Mathf.RoundToInt(value.x), Mathf.RoundToInt(value.y));
        }

        public static Vector2 ToV2(this Vector2Int value)
        {
            return new Vector2(value.x, value.y);
        }

        public static int ToI(this float value)
        {
            return Mathf.FloorToInt(value);
        }

        public static float PerformCalculation(string calc, string arg1, string arg2)
        {
            var number1 = 0f;
            float.TryParse(arg1, out number1);
            var number2 = 0f;
            float.TryParse(arg2, out number2);

            if (calc.Equals("+"))
            {
                return number1 + number2;
            }
            else if (calc.Equals("-"))
            {
                return number1 - number2;
            }
            else if (calc.Equals("*"))
            {
                return number1 * number2;
            }
            else if (calc.Equals("/"))
            {
                // Warning: Integer division probably won't produce the result you're looking for.
                // Try using `double` instead of `int` for your numbers.
                return number1 / number2;
            }
            else
            {
                throw new ArgumentException("Unexpected operator string: " + calc);
            }
        }
        public static bool PerformCondition(string calc, string arg1, string arg2)
        {
            var number1 = 0f;
            float.TryParse(arg1, out number1);
            var number2 = 0f;
            float.TryParse(arg2, out number2);
            return PerformCondition(calc, number1, number2);
        }
        public static bool PerformCondition(string calc, float arg1, float arg2)
        {
            if (calc.Equals("<"))
            {
                return arg1 < arg2;
            }
            else if (calc.Equals("<="))
            {
                return (arg1 <= arg2);
            }
            else if (calc.Equals(">"))
            {
                return arg1 > arg2;
            }
            else if (calc.Equals(">="))
            {
                return (arg1 >= arg2);
            }
            else if (calc.Equals("=="))
            {
                return arg1 == arg2;
            }
            throw new ArgumentException("Unexpected operator string: " + calc);
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
#endif
        static Vector3[] GetBezierCurvePoints(Vector3 start, Vector3 end, int numberofPoints)
        {
            //Vector3 start = new Vector3(10, 10, 0);
            //Vector3 end = new Vector3(100, 50, 0);
            Vector3 control1 = Vector3.Lerp(start, end, 0.5f);
            Vector3 control2 = Vector3.Lerp(start, end, 0.5f);
            float maxY = Mathf.Max(start.y, end.y);
            control1.y = maxY + 5f;
            control2.y = maxY + 5f;

            int numberOfPoints = 10;
            Vector3[] arr = new Vector3[numberofPoints];

            for (int i = 0; i <= numberOfPoints; i++)
            {
                float t = i / (float)numberOfPoints;
                Vector3 pointOnCurve = (1 - t) * (1 - t) * (1 - t) * start + 3 * (1 - t) * (1 - t) * t * control1 + 3 * (1 - t) * t * t * control2 + t * t * t * end;
                //Debug.Log("Point on curve at t = " + t + ": " + pointOnCurve);
                arr[i] = pointOnCurve;
            }
            return arr;
        }

        public static class Color
        {
            public static UnityEngine.Color HexToColor(string hex)
            {
                UnityEngine.Color color = UnityEngine.Color.white;
                if (ColorUtility.TryParseHtmlString(hex, out color))
                {
                    return color;
                }
                else
                {
                    Debug.LogError("Invalid hex color: " + hex);
                    return UnityEngine.Color.white;
                }
            }

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
            public static async Task Yield(int num = 1)
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
                    await Yield(frequencyInFrame);
                }
            }

            public static async Task WaitUntil(Func<bool> condition, int frequencyInFrame = 1, float timeoutInSec = -1)
            {
                await WaitUntil(condition, () => false, frequencyInFrame, timeoutInSec, -1);
            }


            public static async Task WaitUntil(Func<bool> condition, Func<bool> breaker, int frequencyInFrame = 1, float timeoutInSec = -1, float minRequiredSec = -1)
            {
                if (minRequiredSec > 0f)
                    await Delay(minRequiredSec);
                var begin = Time.time;
                while (!condition())
                {
                    if (timeoutInSec > 0f && Time.time - begin > timeoutInSec)
                        break;
                    if (Application.isPlaying == false || breaker())
                        break;
                    await Yield(frequencyInFrame);
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

        public static class Asset
        {
            public static async void WebRequret(string url)
            {
                var w = new WWW(url);
                while (w.isDone == false)
                {
                    JDebug.Q($"Web request progress:{w.progress}");
                    await Task.Yield();
                }
                AssetBundle bundle = w.assetBundle;
                if (w.error == null)
                {

                }
                else
                {
                    JDebug.E($"Web request error:{w.error}");
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

        public KeyedObject(string key)
        {
            this.m_key = key;
        }

        public KeyedObject(string key, T value)
        {
            this.m_key = key;
            this.m_value = value;
        }
    }

    /// <summary>
    /// Inherit from dictionary, cannot have duplicate key
    /// 注意不能用在Editor中, 會因為不正常初始化導致找不到新加入的key, value
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
            //!Possible this dictionary was construct in script
            if (m_values == null)
            {
                List<KeyedObject<T>> list = new List<KeyedObject<T>>();
                foreach (var i in this)
                {
                    list.Add(new KeyedObject<T>(i.Key, i.Value));
                }
                m_values = list.ToArray();
            }
            else
            {
                for (int i = 0; i < m_values.Length; i++)
                    this.Add(m_values[i].key, m_values[i].value);
            }
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
    public class KeyedObject<T1, T2>
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
        protected KeyedObject<T1, T2>[] m_values;
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
    public static void Q(string context, string tag = null)
    {
        Log(0, tag == null ? string.Empty : tag, context, HamTac.Extension.Color.neutral);
    }
    public static void W(string context, string tag = null)
    {
        Log(1, tag == null ? string.Empty : tag, context, Color.yellow);
    }
    public static void E(string context, string tag = null)
    {
        Log(2, tag == null ? string.Empty : tag, context, Color.red);
    }

    public static void Log(string context)
    {
        Log(0, string.Empty, context, Color.gray);
    }
    public static void Log(string tag, string context) { Log(0, tag, context, HamTac.Extension.Color.neutral); }
    public static void Log(string tag, string context, Color color) { Log(0, tag, context, color); }
    public static void Log(int type, string tag, string context, Color color)
    {
        try
        {
#if UNITY_EDITOR && JDEBUG
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
#endif
        }
        catch (System.Exception error)
        {
            Debug.LogError($"Following error has been resolve");
            Debug.LogError(error);
        }
        //if(type == 2)
        //{
        //    Debug.LogError(context);
        //}

    }


    public static string ListToLog<T>(IList<T> list)
    {
        try
        {
#if UNITY_EDITOR && JDEBUG
            var str = string.Empty;
            if (list == null) return "Null";
            foreach (var i in list)
                str += i.ToString() + ", ";
            return str;
#else
        return string.Empty;
#endif
        }
        catch (System.Exception error)
        {
            Debug.LogError($"Following error has been resolve");
            Debug.LogError(error);
        }
        return string.Empty;
    }
}
