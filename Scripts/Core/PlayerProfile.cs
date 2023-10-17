using HamTac;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HamTac
{
    [System.Serializable]
    public class PlayerProfile<T> : IProfileWrapper where T : ISerializeableProfile
    {
        [ES3NonSerializable]
        public T liveData = default(T);
        [ES3Serializable]
        Dictionary<string, byte[]> m_snapshots = new Dictionary<string, byte[]>();

        public const string defaultSnapshotKey = "default";

        public bool HasSnapshot()
        {
            var valid = m_snapshots != null && m_snapshots.Count > 0;
            return valid;
        }
        public bool HasSnapshot(string key)
        {
            return HasSnapshot() && m_snapshots.ContainsKey(key);
        }
        public void SetSnapshot(string key, byte[] source)
        {
            if (m_snapshots.ContainsKey(key))
                m_snapshots[key] = source;
            else
                m_snapshots.Add(key, source);
        }
        public bool GetSnapshotInBytes(string key, out byte[] source)
        {
            if (m_snapshots.ContainsKey(key))
            {
                source = m_snapshots[key];
                return true;
            }
            Debug.LogError($"Cant find snapshot:{key}");
            source = new byte[0];
            return false;
        }
        public void ClearSnapshot(string key)
        {
            if (m_snapshots.ContainsKey(key))
                m_snapshots.Remove(key);
        }

        public byte[] SerializeThenWrapToBytes()
        {
            SetSnapshot(defaultSnapshotKey, Serialize());
            return ES3.Serialize(this);
        }

        public byte[] Serialize()
        {
            liveData.OnSerialize();
            var bytes = ES3.Serialize<T>(liveData);
            return bytes;
        }

        public bool Deserialize(string key, bool clearAfterward, out PlayerProfile<T> result)
        {
            if (GetSnapshotInBytes(key, out var s))
            {
                liveData = ES3.Deserialize<T>(s);
                liveData.OnDeserialized();
                result = this;
                if (clearAfterward)
                    ClearSnapshot(key);
                return true;
            }
            result = this;
            return false;
        }
    }
}