using System;
using System.Collections.Generic;

namespace Game
{
    public static class Observer
    {
        private static readonly Dictionary<Type, Delegate> _observers = new Dictionary<Type, Delegate>();

        public static void AddObserver<T>(Action<T> callback)
        {
            var type = typeof(T);
            if (!_observers.ContainsKey(type))
            {
                _observers[type] = null;
            }
            _observers[type] = Delegate.Combine(_observers[type], callback);
        }

        public static void RemoveObserver<T>(Action<T> callback)
        {
            var type = typeof(T);
            if (_observers.TryGetValue(type, out var currentDelegate))
            {
                _observers[type] = Delegate.Remove(currentDelegate, callback);
                if (_observers[type] == null)
                {
                    _observers.Remove(type);
                }
            }
        }

        public static void Notify<T>(T data)
        {
            var type = typeof(T);
            if (_observers.TryGetValue(type, out var currentDelegate))
            {
                var callback = currentDelegate as Action<T>;
                callback?.Invoke(data);
            }
        }

        public static void ClearAll()
        {
            _observers.Clear();
        }
    }
}

