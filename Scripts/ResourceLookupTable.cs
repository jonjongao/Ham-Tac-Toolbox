using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HamTac
{
    [CreateAssetMenu(fileName = "ResourceTable", menuName = "ResourceLookupTable")]
    public class ResourceLookupTable : AssetLookupTable<Object>
    {
        public static ResourceLookupTable FindByName(string value)
        {
            var path = $"Asset/{value}";
            var obj = Resources.Load<ResourceLookupTable>(path);
            if (obj == null)
                JDebug.Log($"Cant find AssetTable:{path}");
            return obj;
        }
    }

}