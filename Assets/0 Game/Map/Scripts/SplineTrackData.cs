namespace Game.Map
{
    using UnityEngine;
    using UnityEngine.Splines;
    using Unity.Mathematics;
    using Unity.Collections;

    public class SplineTrackData : ITrackData
    {
        private SplineContainer _splineContainer;
        private float _trackWidth;
        private NativeSpline _cachedNativeSpline;
        private float4x4 _cachedTransformMatrix;
        private bool _isNativeSplineValid;

        public SplineTrackData(SplineContainer splineContainer, float trackWidth = 10f)
        {
            _splineContainer = splineContainer;
            _trackWidth = trackWidth;
            _isNativeSplineValid = false;
        }

        private void EnsureNativeSpline()
        {
            if (_splineContainer == null)
            {
                DisposeNativeSpline();
                return;
            }

            float4x4 currentTransform = _splineContainer.transform.localToWorldMatrix;
            
            if (!_isNativeSplineValid || !AreMatricesEqual(_cachedTransformMatrix, currentTransform))
            {
                DisposeNativeSpline();
                
                _cachedTransformMatrix = currentTransform;
                _cachedNativeSpline = new NativeSpline(_splineContainer.Spline, _cachedTransformMatrix, Allocator.Persistent);
                _isNativeSplineValid = true;
            }
        }

        private bool AreMatricesEqual(float4x4 a, float4x4 b)
        {
            return math.all(a.c0 == b.c0) && 
                   math.all(a.c1 == b.c1) && 
                   math.all(a.c2 == b.c2) && 
                   math.all(a.c3 == b.c3);
        }

        private void DisposeNativeSpline()
        {
            if (_isNativeSplineValid)
            {
                _cachedNativeSpline.Dispose();
                _isNativeSplineValid = false;
            }
        }

        public Vector3 GetNearestPoint(Vector3 position)
        {
            if (_splineContainer == null) return position;

            EnsureNativeSpline();
            
            float3 pos = new float3(position.x, position.y, position.z);
            SplineUtility.GetNearestPoint(_cachedNativeSpline, pos, out float3 nearest, out float t);
            
            return new Vector3(nearest.x, nearest.y, nearest.z);
        }

        public Vector3 GetDirectionAt(float t)
        {
             if (_splineContainer == null) return Vector3.forward;

             t = Mathf.Clamp01(t);
             
             Vector3 worldTangent = _splineContainer.EvaluateTangent(t);
             return worldTangent.normalized;
        }

        public float GetProgress(Vector3 pos)
        {
            if (_splineContainer == null) return 0f;

            EnsureNativeSpline();
            
            float3 position = new float3(pos.x, pos.y, pos.z);
            SplineUtility.GetNearestPoint(_cachedNativeSpline, position, out float3 nearest, out float t);
            
            return t;
        }

        public bool IsOffTrack(Vector3 pos)
        {
            if (_splineContainer == null) return false;

            Vector3 nearestPoint = GetNearestPoint(pos);
            float distance = Vector3.Distance(pos, nearestPoint);
            
            return distance > (_trackWidth / 2f);
        }

        ~SplineTrackData()
        {
            DisposeNativeSpline();
        }
    }
}