using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SEA.DependencyInjection.Utility
{
    internal class Cache<TKey, TValue> : IDisposable
    {
        private Dictionary<TKey, TValue> _cache = new();
        private List<TKey> _loadingTypes = new();
        private bool _isDisposed;

        internal TValue GetOrLoad(TKey key, Func<TValue> loader)
        {
            // for resilience during poorly-timed disposes
            var cache = _cache;
            var loadingTypes = _loadingTypes;

            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(Cache<TKey, TValue>));
            }

            TValue instance;

            lock (cache)
            {
                cache.TryGetValue(key, out instance);

                if (instance == null)
                {
                    if (loadingTypes.Contains(key))
                    {
                        do
                        {
                            if (_isDisposed)
                            {
                                throw new ObjectDisposedException(nameof(Cache<TKey, TValue>));
                            }

                            Monitor.Wait(cache);
                        }
                        while (loadingTypes.Contains(key));

                        return cache[key];
                    }
                    else
                    {
                        loadingTypes.Add(key);
                    }
                }
            }

            if (instance == null)
            {
                instance = loader.Invoke();
            }

            lock (cache)
            {
                loadingTypes.Remove(key);
                cache[key] = instance;
                Monitor.PulseAll(cache);
            }

            return instance;
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                var cache = _cache;

                _cache = null;
                _loadingTypes = null;

                if (disposing && typeof(IDisposable).IsAssignableFrom(typeof(TValue)))
                {
                    foreach (var values in cache.Values.ToList())
                    {
                        ((IDisposable)values).Dispose();
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
