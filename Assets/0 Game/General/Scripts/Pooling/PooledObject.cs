using UnityEngine;
using System;

namespace Game.Pooling
{
    public class PooledObject : MonoBehaviour
    {
        public GameObject OriginalPrefab { get; set; }
        public Action<PooledObject> OnReturnToPool { get; set; }

        public void ReturnToPool()
        {
            OnReturnToPool?.Invoke(this);
        }
    }
}
