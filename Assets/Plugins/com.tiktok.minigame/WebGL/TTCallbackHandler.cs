using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UnityEngine;

namespace TTSDK
{
    internal struct TTCallbackKeyPair
    {
        public string success;
        public string fail;
    }

    internal sealed class TTCallbackKeyPairDictionary<T>
    {
        private readonly Dictionary<T, T> _keyToValue =
            new Dictionary<T, T>();
        private readonly Dictionary<T, T> _valueToKey =
            new Dictionary<T, T>();

        internal void Add(T key, T value)
        {
            if (_keyToValue.ContainsKey(key) ||
                _valueToKey.ContainsKey(key) ||
                _keyToValue.ContainsKey(value) ||
                _valueToKey.ContainsKey(value))
            {
                throw new ArgumentException("Duplicate key or value.");
            }

            _keyToValue.Add(key, value);
            _valueToKey.Add(value, key);
        }

        internal bool Remove(T key)
        {
            if (_keyToValue.TryGetValue(key, out T value))
            {
                _keyToValue.Remove(key);
                _valueToKey.Remove(value);
                return true;
            }

            if (_valueToKey.TryGetValue(key, out value))
            {
                _valueToKey.Remove(key);
                _keyToValue.Remove(value);
                return true;
            }

            return false;
        }

        internal bool TryGetValue(T key, out T value)
        {
            return _keyToValue.TryGetValue(key, out value) ||
                   _valueToKey.TryGetValue(key, out value);
        }

        internal void Clear()
        {
            _keyToValue.Clear();
            _valueToKey.Clear();
        }
    }

    /// <summary>
    /// Owns direct jslib callbacks. IDs are process-monotonic, terminal
    /// responses claim state before user code, and missing host responses are
    /// evicted after a bounded timeout.
    /// </summary>
    internal static class TTCallbackHandler
    {
        internal const long DefaultTimeoutMilliseconds = 120000;

        private sealed class PendingCallback
        {
            internal Delegate Callback;
            internal long DeadlineMilliseconds;
            internal Action OnTimeout;
        }

        private static readonly object CallbackLock = new object();
        private static readonly Dictionary<string, PendingCallback>
            responseCallbacks =
                new Dictionary<string, PendingCallback>();
        private static readonly TTCallbackKeyPairDictionary<string> KeyPairs =
            new TTCallbackKeyPairDictionary<string>();
        private static readonly System.Diagnostics.Stopwatch CallbackClock =
            System.Diagnostics.Stopwatch.StartNew();

        private static long _nextCallbackId;
        private static long _timeoutMilliseconds =
            DefaultTimeoutMilliseconds;

        internal static int Count
        {
            get
            {
                lock (CallbackLock)
                {
                    return responseCallbacks.Count;
                }
            }
        }

        internal static TTCallbackKeyPair AddPair<T>(
            Action<T> success,
            Action<T> fail)
            where T : TTBaseResponse
        {
            lock (CallbackLock)
            {
                Action timeout = fail == null
                    ? null
                    : () => InvokeTimeout(fail);
                var result = new TTCallbackKeyPair
                {
                    success = AddLocked(success, timeout),
                    fail = AddLocked(fail, timeout)
                };

                // Empty IDs represent missing optional callbacks and must not
                // enter the bidirectional pair map.
                if (!string.IsNullOrEmpty(result.success) &&
                    !string.IsNullOrEmpty(result.fail))
                {
                    KeyPairs.Add(result.success, result.fail);
                }
                return result;
            }
        }

        internal static string Add<T>(Action<T> callback)
            where T : TTBaseResponse
        {
            lock (CallbackLock)
            {
                return AddLocked(callback, null);
            }
        }

        private static string AddLocked(Delegate callback, Action onTimeout)
        {
            if (callback == null)
            {
                return string.Empty;
            }

            string key = MakeKey();
            responseCallbacks.Add(
                key,
                new PendingCallback
                {
                    Callback = callback,
                    DeadlineMilliseconds = checked(
                        CallbackClock.ElapsedMilliseconds +
                        _timeoutMilliseconds),
                    OnTimeout = onTimeout
                });
            return key;
        }

        internal static string MakeKey()
        {
            long id = Interlocked.Increment(ref _nextCallbackId);
            return "tt-" + id.ToString(CultureInfo.InvariantCulture);
        }

        internal static void InvokeResponseCallback<T>(string json)
            where T : TTBaseResponse
        {
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            T response = JsonUtility.FromJson<T>(json);
            Callback(response.callbackId, response);
        }

        internal static void Callback<T>(string id, T response)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("callback id is empty");
                return;
            }

            PendingCallback pending;
            lock (CallbackLock)
            {
                if (!responseCallbacks.TryGetValue(id, out pending))
                {
                    Debug.LogError($"callback id not found, id: {id}");
                    return;
                }

                // Claim both arms before invoking user code so response,
                // timeout and re-entry have exactly one winner.
                responseCallbacks.Remove(id);
                if (KeyPairs.TryGetValue(id, out string pairId))
                {
                    KeyPairs.Remove(id);
                    responseCallbacks.Remove(pairId);
                }
            }

            if (pending.Callback is Action<T> callback)
            {
                callback(response);
                return;
            }

            Debug.LogError(
                $"callback type mismatch, id: {id}, expected: {typeof(T)}");
        }

        internal static int PollTimeouts()
        {
            return PollTimeouts(CallbackClock.ElapsedMilliseconds);
        }

        internal static int PollTimeouts(long nowMilliseconds)
        {
            List<PendingCallback> expired = null;
            lock (CallbackLock)
            {
                List<string> expiredIds = null;
                foreach (KeyValuePair<string, PendingCallback> entry in
                         responseCallbacks)
                {
                    if (entry.Value.DeadlineMilliseconds > nowMilliseconds)
                    {
                        continue;
                    }

                    expiredIds ??= new List<string>();
                    expiredIds.Add(entry.Key);
                }

                if (expiredIds == null)
                {
                    return 0;
                }

                expired = new List<PendingCallback>(expiredIds.Count);
                foreach (string id in expiredIds)
                {
                    if (!responseCallbacks.TryGetValue(
                            id,
                            out PendingCallback pending))
                    {
                        continue;
                    }

                    responseCallbacks.Remove(id);
                    if (KeyPairs.TryGetValue(id, out string pairId))
                    {
                        KeyPairs.Remove(id);
                        responseCallbacks.Remove(pairId);
                    }
                    expired.Add(pending);
                }
            }

            foreach (PendingCallback pending in expired)
            {
                try
                {
                    pending.OnTimeout?.Invoke();
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        "direct jslib timeout callback exception: " +
                        exception);
                }
            }
            return expired.Count;
        }

        internal static void ClearPendingCallbacks()
        {
            lock (CallbackLock)
            {
                responseCallbacks.Clear();
                KeyPairs.Clear();
            }
        }

        internal static void SetTimeoutForTests(long timeoutMilliseconds)
        {
            if (timeoutMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(timeoutMilliseconds));
            }

            lock (CallbackLock)
            {
                _timeoutMilliseconds = timeoutMilliseconds;
            }
        }

        internal static void ResetForTests()
        {
            lock (CallbackLock)
            {
                responseCallbacks.Clear();
                KeyPairs.Clear();
                _timeoutMilliseconds = DefaultTimeoutMilliseconds;
            }
        }

        private static void InvokeTimeout<T>(Action<T> fail)
            where T : TTBaseResponse
        {
            T response;
            try
            {
                response = (T)Activator.CreateInstance(typeof(T));
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Cannot create timeout response {typeof(T)}: " +
                    exception);
                return;
            }

            response.errCode = -1;
            response.errMsg = "callback timeout";
            fail(response);
        }
    }
}
