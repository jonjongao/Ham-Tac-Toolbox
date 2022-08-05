using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HamTac
{
    public class AssetLookupTable<T> : ScriptableObject
    {
        [SerializeField]
        string m_ID;
        public string ID => m_ID;

        public List<KeyedObject<T>> assets = new List<KeyedObject<T>>();

        public T FindByKey(string value)
        {
            try
            {
                var obj = assets.Find(x => x.key.Equals(value)).value;
                return obj;
            }
            catch (System.NullReferenceException)
            {
                return (T)new object();
            }
        }
    }
}