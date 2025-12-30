using UnityEngine;
using System.Collections.Generic;

namespace Game.Pooling
{
    public class GlobalObjectPoolManager : MonoBehaviour
    {
        private static GlobalObjectPoolManager _instance;
        public static GlobalObjectPoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GlobalObjectPoolManager");
                    _instance = go.AddComponent<GlobalObjectPoolManager>();
                }
                return _instance;
            }
        }

        private Dictionary<GameObject, ObjectPool> _pools;
        private Transform _poolContainer;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            
            _pools = new Dictionary<GameObject, ObjectPool>();
            _poolContainer = new GameObject("PoolContainer").transform;
            _poolContainer.SetParent(transform);
        }

        private ObjectPool GetOrCreatePool(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out ObjectPool pool))
            {
                Transform poolParent = new GameObject($"Pool_{prefab.name}").transform;
                poolParent.SetParent(_poolContainer);

                pool = new ObjectPool(prefab, poolParent);
                _pools.Add(prefab, pool);
            }

            return pool;
        }

        public PooledObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                return null;
            }

            ObjectPool pool = GetOrCreatePool(prefab);
            PooledObject pooledObject = pool.Get(position, rotation);
            if (pooledObject != null)
            {
                pooledObject.OriginalPrefab = prefab;
            }
            return pooledObject;
        }

        public GameObject GetGameObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            PooledObject pooledObject = Get(prefab, position, rotation);
            return pooledObject?.gameObject;
        }

        public T GetComponent<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            PooledObject pooledObject = Get(prefab, position, rotation);
            return pooledObject?.GetComponent<T>();
        }

        public void Return(PooledObject pooledObject)
        {
            if (pooledObject == null || pooledObject.OriginalPrefab == null)
            {
                return;
            }

            if (_pools.TryGetValue(pooledObject.OriginalPrefab, out ObjectPool pool))
            {
                pool.ReturnObject(pooledObject);
            }
        }

        public void ReturnAll(GameObject prefab)
        {
            if (_pools.TryGetValue(prefab, out ObjectPool pool))
            {
                pool.ReturnAll();
            }
        }

        public void ReturnAllPools()
        {
            foreach (var pool in _pools.Values)
            {
                pool.ReturnAll();
            }
        }

        public void ClearPool(GameObject prefab)
        {
            if (_pools.TryGetValue(prefab, out ObjectPool pool))
            {
                pool.Clear();
                _pools.Remove(prefab);
            }
        }

        public void ClearAllPools()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
            _pools.Clear();
        }

        public ObjectPool GetPool(GameObject prefab)
        {
            _pools.TryGetValue(prefab, out ObjectPool pool);
            return pool;
        }

        public bool HasPool(GameObject prefab)
        {
            return _pools.ContainsKey(prefab);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                ClearAllPools();
                _instance = null;
            }
        }
    }
}
