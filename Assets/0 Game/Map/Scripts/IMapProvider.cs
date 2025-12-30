namespace Game.Map
{
    using UnityEngine;
    using System.Collections.Generic;

    public interface ITrackData
    {
        Vector3 GetNearestPoint(Vector3 position);
        Vector3 GetDirectionAt(float t);
        float GetProgress(Vector3 pos);
        bool IsOffTrack(Vector3 pos);
    }

    public interface IMapProvider
    {
        ITrackData GetTrackData();
        int GetTotalCheckpoints();
        List<CarSpawnPoint> GetSpawnPoints();
        CarSpawnPoint GetSpawnPoint(int index);
    }
}