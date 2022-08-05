using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HamTac
{
    [CreateAssetMenu(fileName = "AudioClipTable", menuName = "LookupTable")]
    public class AudioClipLookupTable : AssetLookupTable<HamTac.AudioPointer> { }
}