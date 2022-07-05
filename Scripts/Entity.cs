using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EntityState : KeyedObject<bool>
{
    protected string m_guid;
    protected Vector3 m_worldPosition;
    public string guid => m_guid;
    public Vector3 worldPosition => m_worldPosition;

    public EntityState(string guid, Vector3 pos)
    {
        m_guid = guid;
        m_worldPosition = pos;
    }

    public EntityState ShallowCopy()
    {
        return (EntityState)this.MemberwiseClone();
    }
}

public class Entity : MonoBehaviour, IEntity
{
    [SerializeField]
    bool m_startAsDisable = false;
    public bool startAsDisable => m_startAsDisable;

    [SerializeField]
    bool m_isInit = false;
    public bool isInit => m_isInit;



    [SerializeField]
    protected string m_name;
    public new string name => string.IsNullOrEmpty(m_name) ? gameObject.name : m_name;

    [SerializeField]
    string m_category;
    public string[] category { get; protected set; }

    protected Vector3 m_originPosition;

    [SerializeField]
    protected string m_GUID;
    public string GUID => m_GUID;

    private void Awake()
    {
        m_originPosition = transform.position;
        category = m_category.Split(',');
    }

    protected virtual void Start()
    {
        var id = string.IsNullOrEmpty(m_GUID) ? m_name : m_GUID;
        //if (AllEntity.ContainsKey(id) && id.Equals("Player") == false)
        //    ApplyEntityState(AllEntity[id]);
        //else
        Toggle(!m_startAsDisable);

        m_isInit = true;
    }

    protected virtual void OnEnable()
    {
        if (m_isInit == false) return;
    }

    protected virtual void OnDisable()
    {
        if (m_isInit == false) return;
    }

    public virtual void Toggle(bool value)
    {
        gameObject.SetActive(value);
        var id = string.IsNullOrEmpty(m_GUID) ? m_name : m_GUID;

        //if (AllEntity.ContainsKey(id))
        //{
        //    AllEntity[id] = value;
        //}
        //else
        //{
        //    Util.Log("Entity", "Toggle", string.Format("Track guid:{0} entity:{1}", m_GUID, name));
        //    AllEntity.Add(id, new EntityState()
        //    {
        //        key = name,
        //        value = value,
        //        guid = id,
        //        worldPosition = transform.position
        //    });
        //}
    }

    public virtual void ApplyGUID(string arg0)
    {
        m_GUID = arg0;
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    public bool ContainCategory(string value)
    {
        try
        {
            return category.Contains(value);
        }
        catch (System.NullReferenceException)
        {
            return false;
        }
    }

    public bool ContainCategory(string[] value)
    {
        try
        {
            return category.Contains(value);
        }
        catch (System.NullReferenceException)
        {
            return false;
        }
    }
}


public interface IEntity
{
    GameObject gameObject { get; }

    bool ContainCategory(string value);
    bool ContainCategory(string[] value);
}