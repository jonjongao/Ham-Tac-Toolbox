using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HamTac
{
    [CreateAssetMenu(fileName = "AudioClipTable", menuName = "AudioClipLookupTable")]
    public class AudioClipLookupTable : AssetLookupTable<HamTac.AudioPointer>
    {
        public static AudioClipLookupTable FindByName(string value)
        {
            var path = $"Asset/{value}";
            var obj = Resources.Load<AudioClipLookupTable>(path);
            if (obj == null)
                JDebug.Log($"Cant find AssetTable:{path}");
            return obj;
        }
    }
}