using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utage;
using UnityEngine.UI;
using System.Linq;

public class GraphicManagerHelper : MonoBehaviour
{
    AdvGraphicManager m_manager;
    [SerializeField]
    Material m_rawImageMaterial;
    [SerializeField]
    List<MaterialModifer> m_materialModifer=new List<MaterialModifer>();

    [System.Serializable]
    public struct MaterialModifer
    {
        public string layerName;
        public Material material;
    }

    private void OnEnable()
    {
        m_manager = GetComponent<AdvGraphicManager>();

        if (m_manager == null) return;
        m_manager.OnDrawGraphicObject.AddListener(OnDrawGraphicObject);
    }

    private void OnDisable()
    {
        if (m_manager == null) return;
        m_manager.OnDrawGraphicObject.RemoveListener(OnDrawGraphicObject);
    }

    void OnDrawGraphicObject(AdvGraphicObject obj, AdvGraphicInfo info)
    {
        var graphic = obj.RenderObject.GetComponent<MaskableGraphic>();
        JDebug.Log($"**********On draw graphic object:{obj.name} layer:{obj.Layer.name} renderObject:{obj.RenderObject.name} dataType:{info.DataType} fileType:{info.FileType}");
        //if (graphic is RawImage)
        //{
        //    (graphic as RawImage).material = m_rawImageMaterial;
        //}



        if(m_materialModifer.Any(x=>x.layerName.Equals(obj.Layer.name)))
        {
            graphic.material = m_materialModifer.Find(x => x.layerName.Equals(obj.Layer.name)).material;
        }
    }


}
