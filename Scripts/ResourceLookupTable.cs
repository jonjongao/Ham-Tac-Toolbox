using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HamTac
{
    [CreateAssetMenu(fileName = "ResourceTable", menuName = "TBSF/LookupTable/ResourceLookupTable")]
    public class ResourceLookupTable : AssetLookupTable<Object>
    {
        static Dictionary<string, ResourceLookupTable> cache = new Dictionary<string, ResourceLookupTable>();

       
    }

}