using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HamTac
{
    [CreateAssetMenu(fileName = "GameObjectTable", menuName = "GameObjectLookupTable")]
    public class GameObjectLookupTable : AssetLookupTable<GameObject>
    {
        public static GameObjectLookupTable FindByName(string value)
        {
            var path = $"Asset/{value}";
            var obj = Resources.Load<GameObjectLookupTable>(path);
            if (obj == null)
                JDebug.Log($"Cant find AssetTable:{path}");
            return obj;
        }
    }

}