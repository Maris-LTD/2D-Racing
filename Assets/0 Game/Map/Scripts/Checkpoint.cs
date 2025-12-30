namespace Game.Map
{
    using UnityEngine;

    [RequireComponent(typeof(BoxCollider))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private int _checkpointIndex;
        [SerializeField] private bool _isStartFinish;

        private BoxCollider _collider;

        public int CheckpointIndex => _checkpointIndex;
        public bool IsStartFinish => _isStartFinish;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
            _collider.isTrigger = true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = _isStartFinish ? Color.green : Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            BoxCollider col = GetComponent<BoxCollider>();
            if (col != null)
            {
                Gizmos.DrawWireCube(col.center, col.size);
            }
        }
#endif
    }
}

