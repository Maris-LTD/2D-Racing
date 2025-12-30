namespace Game.Map{
    using UnityEngine;
    using UnityEngine.Splines;
    using System.Collections.Generic;
    using System.Linq;

    public class MapController : MonoBehaviour, IMapProvider
    {
        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private float _trackWidth = 8f;
        [SerializeField] private Checkpoint[] _checkpoints;
        [SerializeField] private Checkpoint _startFinishCheckpoint;
        [SerializeField] private CarSpawnPoint[] _spawnPoints;

        private ITrackData _trackData;

        private void Awake()
        {
            if (_splineContainer == null)
            {
                _splineContainer = GetComponentInChildren<SplineContainer>();
            }

            _trackData = new SplineTrackData(_splineContainer, _trackWidth);
            
            if (_spawnPoints == null || _spawnPoints.Length == 0)
            {
                _spawnPoints = GetComponentsInChildren<CarSpawnPoint>();
            }
        }
        
        public ITrackData GetTrackData()
        {
            return _trackData;
        }

        public int GetTotalCheckpoints()
        {
            return _checkpoints?.Length ?? 0;
        }

        public List<CarSpawnPoint> GetSpawnPoints()
        {
            return _spawnPoints != null ? _spawnPoints.ToList() : new List<CarSpawnPoint>();
        }

        public CarSpawnPoint GetSpawnPoint(int index)
        {
            if (_spawnPoints == null || index < 0 || index >= _spawnPoints.Length)
            {
                return null;
            }
            return _spawnPoints[index];
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_splineContainer != null)
            {
                Gizmos.color = new Color(1, 0, 0, 0.3f);
                foreach (var knot in _splineContainer.Spline.Knots)
                {
                    Vector3 worldPos = _splineContainer.transform.TransformPoint(knot.Position);
                    Gizmos.DrawWireSphere(worldPos, _trackWidth / 2f);
                }
            }

            if (_spawnPoints is { Length: > 0 })
            {
                for (int i = 0; i < _spawnPoints.Length; i++)
                {
                    if (_spawnPoints[i] != null)
                    {
                        if (i < _spawnPoints.Length - 1 && _spawnPoints[i + 1] != null)
                        {
                            Gizmos.color = Color.white;
                            Gizmos.DrawLine(_spawnPoints[i].Position, _spawnPoints[i + 1].Position);
                        }
                    }
                }
            }
        }
#endif
    }
}