using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HamTac
{
    [CreateAssetMenu(fileName = "AssetTable", menuName = "AssetTable")]
    public class AssetTableObject : ScriptableObject
    {
        [SerializeField]
        KeyedObjects<GameObject> m_assets = new KeyedObjects<GameObject>();
        public KeyedObjects<GameObject> assets => m_assets;
    }

    public class AssetTable
    {
        public static AssetTableObject FindByName(string value)
        {
            var path = $"Asset/{value}";
            var obj = Resources.Load<AssetTableObject>(path);
            if (obj == null)
                JDebug.Log($"Cant find AssetTable:{path}");
            return obj;
        }
    }
}