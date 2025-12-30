namespace Game.Map
{
    using System.Collections;
    using Game.GameFlow.Events;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    public class TrackLoader
    {
        private const string TRACKS_ADDRESS_KEY = "Tracks/";

        public IEnumerator LoadTrackCoroutine(string trackName, System.Action<GameObject> onComplete)
        {
            string addressKey = TRACKS_ADDRESS_KEY + trackName;
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(addressKey);
            
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Addressables.Release(handle);
                onComplete?.Invoke(null);
                yield break;
            }

            GameObject trackPrefab = handle.Result;
            GameObject trackInstance = Object.Instantiate(trackPrefab);
            Addressables.Release(handle);
            trackInstance.name = trackName;

            Observer.Notify(new TrackLoadedEvent { TrackInstance = trackInstance });

            onComplete?.Invoke(trackInstance);
        }

        public GameObject LoadTrack(string trackName)
        {
            string addressKey = TRACKS_ADDRESS_KEY + trackName;
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(addressKey);
            GameObject trackPrefab = handle.WaitForCompletion();

            if (handle.Status != AsyncOperationStatus.Succeeded || trackPrefab == null)
            {
                Addressables.Release(handle);
                return null;
            }

            GameObject trackInstance = Object.Instantiate(trackPrefab);
            Addressables.Release(handle);
            trackInstance.name = trackName;

            Observer.Notify(new TrackLoadedEvent { TrackInstance = trackInstance });

            return trackInstance;
        }
    }
}

