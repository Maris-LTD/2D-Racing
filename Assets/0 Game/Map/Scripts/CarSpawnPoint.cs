namespace Game.Map
{
    using UnityEngine;

    public class CarSpawnPoint : MonoBehaviour
    {
        [SerializeField] private int _spawnIndex;
        [SerializeField] private bool _isAISpawn = false;

        public int SpawnIndex => _spawnIndex;
        public bool IsAISpawn => _isAISpawn;
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = _isAISpawn ? Color.red : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 1f);
            
            Gizmos.color = _isAISpawn ? new Color(1f, 0f, 0f, 0.3f) : new Color(0f, 1f, 1f, 0.3f);
            Gizmos.DrawSphere(transform.position, 0.5f);
            
            Gizmos.color = Color.yellow;
            Vector3 forward = transform.forward * 3f;
            Gizmos.DrawRay(transform.position, forward);
        }

        private void OnDrawGizmosSelected()
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = _isAISpawn ? Color.red : Color.cyan;
            style.fontSize = 14;
            style.fontStyle = FontStyle.Bold;
            
            string label = _isAISpawn ? $"AI Spawn {_spawnIndex}" : $"Player Spawn {_spawnIndex}";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, label, style);
        }
#endif
    }
}

