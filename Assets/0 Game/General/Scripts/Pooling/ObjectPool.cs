using UnityEngine;
using System.Collections.Generic;

namespace Game.Pooling
{
    public class ObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly Queue<PooledObject> _availableObjects;
        private readonly HashSet<PooledObject> _activeObjects;

        public GameObject Prefab => _prefab;
        public int AvailableCount => _availableObjects.Count;
        public int ActiveCount => _activeObjects.Count;
        public int TotalCount => AvailableCount + ActiveCount;

        public ObjectPool(GameObject prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
            _availableObjects = new Queue<PooledObject>();
            _activeObjects = new HashSet<PooledObject>();
        }

        private PooledObject CreateNewObject(Vector3 position, Quaternion rotation)
        {
            GameObject obj = Object.Instantiate(_prefab, _parent);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(false);

            PooledObject pooledObject = obj.GetComponent<PooledObject>();
            if (pooledObject == null)
            {
                pooledObject = obj.AddComponent<PooledObject>();
            }

            pooledObject.OnReturnToPool = ReturnObject;

            return pooledObject;
        }

        public PooledObject Get(Vector3 position, Quaternion rotation)
        {
            PooledObject pooledObject;

            if (_availableObjects.Count > 0)
            {
                pooledObject = _availableObjects.Dequeue();
            }
            else
            {
                pooledObject = CreateNewObject(position, rotation);
            }

            _activeObjects.Add(pooledObject);
            pooledObject.transform.SetParent(null);
            pooledObject.transform.position = position;
            pooledObject.transform.rotation = rotation;
            pooledObject.gameObject.SetActive(true);

            return pooledObject;
        }

        public GameObject GetGameObject(Vector3 position, Quaternion rotation)
        {
            return Get(position, rotation)?.gameObject;
        }

        public T GetComponent<T>(Vector3 position, Quaternion rotation) where T : Component
        {
            return Get(position, rotation)?.GetComponent<T>();
        }

        public void ReturnObject(PooledObject pooledObject)
        {
            if (pooledObject == null || !_activeObjects.Contains(pooledObject))
            {
                return;
            }

            _activeObjects.Remove(pooledObject);
            pooledObject.gameObject.SetActive(false);
            pooledObject.transform.SetParent(_parent);
            pooledObject.transform.localPosition = Vector3.zero;
            pooledObject.transform.localRotation = Quaternion.identity;
            _availableObjects.Enqueue(pooledObject);
        }

        public void ReturnAll()
        {
            var activeList = new List<PooledObject>(_activeObjects);
            foreach (var obj in activeList)
            {
                ReturnObject(obj);
            }
        }

        public void Clear()
        {
            ReturnAll();

            while (_availableObjects.Count > 0)
            {
                var obj = _availableObjects.Dequeue();
                if (obj != null)
                {
                    Object.Destroy(obj.gameObject);
                }
            }

            _activeObjects.Clear();
        }
    }
}
