using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TTSDK
{
    public class TTFileSystemManagerWebGL : TTFileSystemManager
    {
        // Inlined here (rather than calling TTSDKLog.Debug) because TTWebGL asmdef cannot
        // reference the runtime asmdef without creating a cycle. Define TTSDK_LOG_DEBUG to
        // enable; otherwise the entire call (including string-interpolation arguments) is
        // compiler-erased.
        [System.Diagnostics.Conditional("TTSDK_LOG_DEBUG")]
        private static void TraceLog(string message) => UnityEngine.Debug.Log(message);

#region 非流式读写
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkWriteStringFileSync(string filePath, string data, string encoding);
#else
        private static string StarkWriteStringFileSync(string filePath, string data, string encoding)
        {
            return TTFileSystemManagerDefault.Instance.WriteFileSync(filePath, data, encoding);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkWriteBinFileSync(string filePath, byte[] data, int dataLength);
#else
        private static string StarkWriteBinFileSync(string filePath, byte[] data, int dataLength)
        {
            return TTFileSystemManagerDefault.Instance.WriteFileSync(filePath, data);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkWriteBinFile(string filePath, byte[] data, int dataLength, string s, string f);
#else
        private static void StarkWriteBinFile(string filePath, byte[] data, int dataLength, string s, string f)
        {
            TTFileSystemManagerDefault.Instance.WriteFileSync(filePath, data);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkWriteStringFile(string filePath, string data, string encoding, string s,
            string f);
#else
        private static void StarkWriteStringFile(string filePath, string data, string encoding, string s,
            string f)
        {
            TTFileSystemManagerDefault.Instance.WriteFileSync(filePath, data, encoding);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkReadFile(string filePath, string encoding, string callbackId);
#else
        private static void StarkReadFile(string filePath, string encoding, string callbackId)
        {
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkReadStringFileSync(string filePath, string encoding);
#else
        private static string StarkReadStringFileSync(string filePath, string encoding)
        {
            return TTFileSystemManagerDefault.Instance.ReadFileSync(filePath, encoding);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int StarkReadBinFileSync(string filePath);
#else
        private static int StarkReadBinFileSync(string filePath)
        {
            return 0;
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkShareFileBuffer(byte[] data, string callbackId);
#else
        private static void StarkShareFileBuffer(byte[] data, string callbackId)
        {
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkDiscardFileBuffer(string callbackId);
#else
        private static void StarkDiscardFileBuffer(string callbackId)
        {
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern bool StarkAccessFileSync(string filePath);
#else
        private static bool StarkAccessFileSync(string filePath)
        {
            return TTFileSystemManagerDefault.Instance.AccessSync(filePath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkAccessFile(string filePath, string s, string f);
#else
        private static void StarkAccessFile(string filePath, string s, string f)
        {
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkCopyFileSync(string srcPath, string destPath);
#else
        private static string StarkCopyFileSync(string srcPath, string destPath)
        {
            return TTFileSystemManagerDefault.Instance.CopyFileSync(srcPath, destPath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkRenameFileSync(string srcPath, string destPath);
#else
        private static string StarkRenameFileSync(string srcPath, string destPath)
        {
            return TTFileSystemManagerDefault.Instance.RenameFileSync(srcPath, destPath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkCopyFile(string srcPath, string destPath, string s, string f);
#else
        private static void StarkCopyFile(string srcPath, string destPath, string s, string f)
        {
            TTFileSystemManagerDefault.Instance.CopyFileSync(srcPath, destPath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkRenameFile(string srcPath, string destPath, string s, string f);
#else
        private static void StarkRenameFile(string srcPath, string destPath, string s, string f)
        {
            TTFileSystemManagerDefault.Instance.RenameFileSync(srcPath, destPath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkUnlinkSync(string filePath);
#else
        private static string StarkUnlinkSync(string filePath)
        {
            return TTFileSystemManagerDefault.Instance.UnlinkSync(filePath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkUnlink(string filePath, string s, string f);
#else
        private static void StarkUnlink(string filePath, string s, string f)
        {
            TTFileSystemManagerDefault.Instance.UnlinkSync(filePath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkMkdir(string dirPath, bool recursive, string s, string f);
#else
        private static void StarkMkdir(string dirPath, bool recursive, string s, string f)
        {
            TTFileSystemManagerDefault.Instance.MkdirSync(dirPath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkMkdirSync(string dirPath, bool recursive);
#else
        private static string StarkMkdirSync(string dirPath, bool recursive)
        {
            return TTFileSystemManagerDefault.Instance.MkdirSync(dirPath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkRmdir(string dirPath, bool recursive, string s, string f);
#else
        private static void StarkRmdir(string dirPath, bool recursive, string s, string f)
        {
            TTFileSystemManagerDefault.Instance.RmdirSync(dirPath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkRmdirSync(string dirPath, bool recursive);
#else
        private static string StarkRmdirSync(string dirPath, bool recursive)
        {
            return TTFileSystemManagerDefault.Instance.RmdirSync(dirPath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkStatSync(string path);
#else
        private static string StarkStatSync(string path)
        {
            return "";
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkStat(string path, string callbackId);
#else
        private static void StarkStat(string path, string callbackId)
        {
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkGetSavedFileList(string callbackId);
#else
        private static void StarkGetSavedFileList(string callbackId)
        {
        }
#endif
        
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkGetCachedPathForUrl(string url);
#else
        private static string StarkGetCachedPathForUrl(string url)
        {
            return "";
        }
#endif

        #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkAppendStringFileSync(string filePath, string data, string encoding);
#else
        private static string StarkAppendStringFileSync(string filePath, string data, string encoding)
        {
            return TTFileSystemManagerDefault.Instance.AppendFileSync(filePath, data, encoding);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkAppendBinFileSync(string filePath, byte[] data, int dataLength);
#else
        private static string StarkAppendBinFileSync(string filePath, byte[] data, int dataLength)
        {
            return TTFileSystemManagerDefault.Instance.AppendFileSync(filePath, data);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkAppendStringFile(string filePath, string data, string encoding, string s, string f);
#else
        private static void StarkAppendStringFile(string filePath, string data, string encoding, string s, string f)
        {
            TTFileSystemManagerDefault.Instance.AppendFileSync(filePath, data, encoding);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkAppendBinFile(string filePath, byte[] data, int dataLength, string s, string f);
#else
        private static void StarkAppendBinFile(string filePath, byte[] data, int dataLength, string s, string f)
        {
            TTFileSystemManagerDefault.Instance.AppendFileSync(filePath, data);
        }
#endif
        
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkTruncate(string filePath, int length, string s, string f);
#else
        private static void StarkTruncate(string filePath, int length, string s, string f)
        {
            TTFileSystemManagerDefault.Instance.TruncateSync(filePath, length);
        }
#endif
        
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkTruncateSync(string filePath, int length);
#else
        private static string StarkTruncateSync(string filePath, int length)
        {
            return TTFileSystemManagerDefault.Instance.TruncateSync(filePath, length);
        }
#endif
        
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkReadDir(string filePath, string callbackId);
#else
        private static void StarkReadDir(string filePath, string callbackId)
        {
            TTFileSystemManagerDefault.Instance.ReadDirSync(filePath);
        }
#endif
        
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkReadDirSync(string filePath);
#else
        private static string StarkReadDirSync(string filePath)
        {
            return TTFileSystemManagerDefault.Instance.ReadDirSync(filePath)[0];
        }
#endif
#endregion

#region 流式读写
#if UNITY_WEBGL && !UNITY_EDITOR 
        //webgl
        [DllImport("__Internal")]
        private static extern void TT_FOpenFile(string filePath, string flag, string callbackId);
        [DllImport("__Internal")]
        private static extern string TT_FOpenFileSync(string filePath, string flag);
        [DllImport("__Internal")]
        private static extern void TT_FCloseFile(string fd, string s, string f);
        [DllImport("__Internal")]
        private static extern string TT_FCloseFileSync(string fd);
        [DllImport("__Internal")]
        private static extern void TT_FWriteBinFile(string fd, byte[] data, int dataLength, int offset, int length, string callbackId, int position);
        [DllImport("__Internal")]
        private static extern void TT_FWriteStringFile(string fd, string data, string encoding, string callbackId);
        [DllImport("__Internal")]
        private static extern string TT_FWriteBinFileSync(string fd, byte[] data, int dataLength, int offset, int length, int position);
        [DllImport("__Internal")]
        private static extern string TT_FWriteStringFileSync(string fd, string data, string encoding);
        [DllImport("__Internal")]
        private static extern void TT_FReadFile(string fd, int arrayBufferLength, int offset, int length, int position, string callbackId);
        [DllImport("__Internal")]
        private static extern string TT_FReadFileSync(string fd, int arrayBufferLength, int offset, int length, int position);
        [DllImport("__Internal")]
        private static extern void TT_FReadCompressedFile(string filePath, string compressionAlgorithm, string callbackId);
        [DllImport("__Internal")]
        private static extern string TT_FReadCompressedFileSync(string filePath, string compressionAlgorithm);
        [DllImport("__Internal")]
        private static extern void TT_Fstat(string fd, string callbackId);
        [DllImport("__Internal")]
        private static extern string TT_FstatSync(string fd);
        [DllImport("__Internal")]
        private static extern void TT_Ftruncate(string fd, int length, string s, string f);
        [DllImport("__Internal")]
        private static extern string TT_FtruncateSync(string fd, int length);
#else 
        //native
        private static void TT_FOpenFile(string filePath, string flag, string callbackId) {}
        private static string TT_FOpenFileSync(string filePath, string flag) { return null; }
        private static void TT_FCloseFile(string fd, string s, string f) {}
        private static string TT_FCloseFileSync(string fd) { return ""; }
        private static void TT_FWriteBinFile(string fd, byte[] data, int dataLength, int offset, int length, string callbackId, int position) {}
        private static void TT_FWriteStringFile(string fd, string data, string encoding, string callbackId) {}
        private static string TT_FWriteBinFileSync(string fd, byte[] data, int dataLength, int offset, int length, int position) {return ""; }
        private static string TT_FWriteStringFileSync(string fd, string data, string encoding) {return "";}
        private static void TT_FReadFile(string fd, int arrayBufferLength, int offset, int length, int position, string callbackId) {}
        private static string TT_FReadFileSync(string fd, int arrayBufferLength, int offset, int length, int position) { return ""; }
        private static void TT_FReadCompressedFile(string filePath, string compressionAlgorithm, string callbackId) {}
        private static string TT_FReadCompressedFileSync(string filePath, string compressionAlgorithm) { return ""; }
        private static void TT_Fstat(string fd, string callbackId) {}
        private static string TT_FstatSync(string fd) { return null; }
        private static void TT_Ftruncate(string fd, int length, string s, string f) {}
        private static string TT_FtruncateSync(string fd, int length) { return ""; }
#endif
#endregion
       
        
        public static readonly TTFileSystemManagerWebGL Instance = new TTFileSystemManagerWebGL();

        private static Dictionary<string, ReadFileParam> s_readFileParams = new Dictionary<string, ReadFileParam>();
        private static Dictionary<string, StatParam> s_statParams = new Dictionary<string, StatParam>();

        private static Dictionary<string, GetSavedFileListParam> s_getSavedFileListParams =
            new Dictionary<string, GetSavedFileListParam>();
        private static Dictionary<string, ReadDirParam> s_readDirParams = new Dictionary<string, ReadDirParam>();

        #region 流式读写
        private static Dictionary<string, OpenParam> s_openParams = new();
        private static Dictionary<string, WriteParam> s_writeParams = new();
        private static Dictionary<string, ReadParam> s_readParams = new();
        private static Dictionary<string, ReadCompressedFileParam> s_readCompressedFileParams = new();
        private static Dictionary<string, FstatParam> s_fstatParams = new();
        #endregion

        private sealed class PendingFileSystemCallback
        {
            internal long DeadlineMilliseconds;
            internal Func<bool> RemoveCallback;
            internal Action OnTimeout;
        }

        private static readonly object s_pendingCallbackLock = new object();
        private static readonly Dictionary<string, PendingFileSystemCallback>
            s_pendingCallbacks =
                new Dictionary<string, PendingFileSystemCallback>();
        private static readonly System.Diagnostics.Stopwatch s_callbackClock =
            System.Diagnostics.Stopwatch.StartNew();

        private static void RegisterPendingCallback<TParam>(
            Dictionary<string, TParam> callbacks,
            string callbackId,
            TParam callback,
            Action<TParam> onTimeout)
            where TParam : class
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            lock (s_pendingCallbackLock)
            {
                callbacks.Add(callbackId, callback);
                s_pendingCallbacks.Add(
                    callbackId,
                    new PendingFileSystemCallback
                    {
                        DeadlineMilliseconds = checked(
                            s_callbackClock.ElapsedMilliseconds +
                            TTCallbackHandler.DefaultTimeoutMilliseconds),
                        RemoveCallback = () => callbacks.Remove(callbackId),
                        OnTimeout = () => onTimeout?.Invoke(callback)
                    });
            }
        }

        private static bool TryTakePendingCallback<TParam>(
            Dictionary<string, TParam> callbacks,
            string callbackId,
            out TParam callback)
            where TParam : class
        {
            lock (s_pendingCallbackLock)
            {
                if (string.IsNullOrEmpty(callbackId) ||
                    !callbacks.TryGetValue(callbackId, out callback))
                {
                    callback = null;
                    return false;
                }

                callbacks.Remove(callbackId);
                s_pendingCallbacks.Remove(callbackId);
                return callback != null;
            }
        }

        private static void RemovePendingCallback<TParam>(
            Dictionary<string, TParam> callbacks,
            string callbackId)
            where TParam : class
        {
            lock (s_pendingCallbackLock)
            {
                callbacks.Remove(callbackId);
                s_pendingCallbacks.Remove(callbackId);
            }
        }

        internal static int PollPendingFileSystemCallbacks(
            long nowMilliseconds)
        {
            List<Action> timeouts = null;
            lock (s_pendingCallbackLock)
            {
                List<string> expiredIds = null;
                foreach (KeyValuePair<string, PendingFileSystemCallback> entry
                         in s_pendingCallbacks)
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

                timeouts = new List<Action>(expiredIds.Count);
                foreach (string callbackId in expiredIds)
                {
                    if (!s_pendingCallbacks.TryGetValue(
                            callbackId,
                            out PendingFileSystemCallback pending))
                    {
                        continue;
                    }

                    s_pendingCallbacks.Remove(callbackId);
                    if (pending.RemoveCallback())
                    {
                        timeouts.Add(pending.OnTimeout);
                    }
                }
            }

            foreach (Action timeout in timeouts)
            {
                try
                {
                    timeout?.Invoke();
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        "file-system timeout callback exception: " +
                        exception);
                }
            }
            return timeouts.Count;
        }

        private static void ClearPendingFileSystemCallbacks()
        {
            lock (s_pendingCallbackLock)
            {
                s_readFileParams.Clear();
                s_statParams.Clear();
                s_getSavedFileListParams.Clear();
                s_readDirParams.Clear();
                s_openParams.Clear();
                s_writeParams.Clear();
                s_readParams.Clear();
                s_readCompressedFileParams.Clear();
                s_fstatParams.Clear();
                s_pendingCallbacks.Clear();
            }
        }

        private static bool _initialized;

        public TTFileSystemManagerWebGL()
        {
            MigratingData();
            CreateStarkFileSystemHandler();
        }

        private void CreateStarkFileSystemHandler()
        {
            if (!_initialized)
            {
                _initialized = true;
                GameObject obj = new GameObject();
                Object.DontDestroyOnLoad(obj);
                obj.name = "StarkFileSystemManager";
                obj.AddComponent<TTFileSystemHandler>();
            }
        }

        public class TTFileSystemHandler : MonoBehaviour
        {
            private float _nextTimeoutPoll;

            private static bool TryTakeCallback<TParam>(
                Dictionary<string, TParam> callbacks,
                string callbackId,
                string handlerName,
                out TParam callback)
                where TParam : class
            {
                if (!TryTakePendingCallback(
                        callbacks,
                        callbackId,
                        out callback))
                {
                    Debug.LogWarning(
                        $"{handlerName} - no callback for callbackId: " +
                        callbackId);
                    return false;
                }

                return true;
            }

            private void Update()
            {
                if (Time.realtimeSinceStartup < _nextTimeoutPoll)
                {
                    return;
                }

                _nextTimeoutPoll = Time.realtimeSinceStartup + 1f;
                TTCallbackHandler.PollTimeouts();
                PollPendingFileSystemCallbacks(
                    s_callbackClock.ElapsedMilliseconds);
            }

            private void OnDestroy()
            {
                _initialized = false;
                TTCallbackHandler.ClearPendingCallbacks();
                ClearPendingFileSystemCallbacks();
            }

            private void SafeHandle(string handlerName, string msg, Action<string> action)
            {
                try
                {
                    TraceLog($"{handlerName} - {msg}");
                    action(msg);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{handlerName} exception: {e}");
                }
            }

            public void HandleNativeCallback(string msg) => SafeHandle(nameof(HandleNativeCallback), msg, m =>
            {
                TTCallbackHandler.InvokeResponseCallback<TTBaseResponse>(m);
            });

            public void HandleReadFileCallback(string msg) => SafeHandle(nameof(HandleReadFileCallback), msg, m =>
            {
                var res = JsonUtility.FromJson<TTReadFileCallback>(m);
                if (res == null)
                {
                    Debug.LogWarning("HandleReadFileCallback - empty response");
                    return;
                }

                if (!TryTakeCallback(
                        s_readFileParams,
                        res.callbackId,
                        nameof(HandleReadFileCallback),
                        out ReadFileParam conf))
                {
                    StarkDiscardFileBuffer(res.callbackId);
                    return;
                }

                if (res.errCode == 0)
                {
                    if (string.IsNullOrEmpty(conf.encoding) || conf.encoding.Equals("binary"))
                    {
                        if (res.byteLength < 0)
                        {
                            StarkDiscardFileBuffer(res.callbackId);
                            conf.fail?.Invoke(new TTReadFileResponse
                            {
                                errCode = -1,
                                errMsg = "invalid binary response length"
                            });
                            return;
                        }
                        var sharedBuffer = new byte[res.byteLength];
                        StarkShareFileBuffer(sharedBuffer, res.callbackId);
                        var obj = new TTReadFileResponse()
                        {
                            binData = sharedBuffer,
                        };
                        conf.success?.Invoke(obj);
                    }
                    else
                    {
                        var obj = new TTReadFileResponse()
                        {
                            stringData = res.data
                        };
                        conf.success?.Invoke(obj);
                    }
                }
                else
                {
                    var obj = new TTReadFileResponse()
                    {
                        errCode = res.errCode,
                        errMsg = res.errMsg
                    };
                    conf.fail?.Invoke(obj);
                }
            });

            public void HandleStatCallback(string msg) => SafeHandle(nameof(HandleStatCallback), msg, m =>
            {
                var res = JsonMapper.ToObject<TTStatResponse>(m);
                if (res == null)
                {
                    Debug.LogError("empty response");
                    return;
                }
                if (!TryTakeCallback(
                        s_statParams,
                        res.callbackId,
                        nameof(HandleStatCallback),
                        out StatParam conf))
                    return;

                if (res.errCode == 0)
                {
                    if (res.stat == null)
                    {
                        Debug.LogWarning("empty stat info");
                        res.stat = new TTStatInfo();
                    }
                    conf.success?.Invoke(res);
                }
                else
                {
                    res.stat = new TTStatInfo();
                    conf.fail?.Invoke(res);
                }
            });

            public void HandleGetSavedFileListCallback(string msg) => SafeHandle(nameof(HandleGetSavedFileListCallback), msg, m =>
            {
                var res = JsonMapper.ToObject<TTGetSavedFileListResponse>(m);
                if (res == null)
                {
                    Debug.LogError("empty response");
                    return;
                }
                if (!TryTakeCallback(
                        s_getSavedFileListParams,
                        res.callbackId,
                        nameof(HandleGetSavedFileListCallback),
                        out GetSavedFileListParam conf))
                    return;

                if (res.errCode == 0)
                {
                    if (res.fileList == null)
                    {
                        res.fileList = new TTFileInfo[0];
                    }

                    conf.success?.Invoke(res);
                }
                else
                {
                    res.fileList = new TTFileInfo[0];
                    conf.fail?.Invoke(res);
                }
            });
            
            public void HandleReadDirCallback(string msg) => SafeHandle(nameof(HandleReadDirCallback), msg, m =>
            {
                var res = JsonMapper.ToObject<TTReadDirResponse>(m);
                if (res == null)
                {
                    Debug.LogError("empty response");
                    return;
                }
                if (!TryTakeCallback(
                        s_readDirParams,
                        res.callbackId,
                        nameof(HandleReadDirCallback),
                        out ReadDirParam conf))
                    return;
                if (res.errCode == 0)
                {
                    if (res.files == null)
                    {
                        res.files = Array.Empty<string>();
                    }
                    conf.success?.Invoke(res);
                }
                else
                {
                    res.files = Array.Empty<string>();
                    conf.fail?.Invoke(res);
                }
            });

#region 流式读写
            public void HandleFOpenCallback(string msg) => SafeHandle(nameof(HandleFOpenCallback), msg, m =>
            {
                var res = JsonUtility.FromJson<TTOpenResponse>(m);
                if (res == null)
                {
                    Debug.LogWarning("HandleFOpenCallback - empty response");
                    return;
                }

                if (!TryTakeCallback(
                        s_openParams,
                        res.callbackId,
                        nameof(HandleFOpenCallback),
                        out OpenParam conf))
                    return;

                if (res.errCode == 0)
                {
                    conf.success?.Invoke(res);
                }
                else
                {
                    conf.fail?.Invoke(res);
                }
            });

            public void HandleFWriteCallback(string msg) => SafeHandle(nameof(HandleFWriteCallback), msg, m =>
            {
                var res = JsonUtility.FromJson<TTWriteResponse>(m);
                if (res == null)
                {
                    Debug.LogWarning("HandleFWriteCallback - empty response");
                    return;
                }

                if (!TryTakeCallback(
                        s_writeParams,
                        res.callbackId,
                        nameof(HandleFWriteCallback),
                        out WriteParam conf))
                    return;

                if (res.errCode == 0)
                {
                    conf.success?.Invoke(res);
                }
                else
                {
                    conf.fail?.Invoke(res);
                }
            });

            public void HandleFReadCallback(string msg) => SafeHandle(nameof(HandleFReadCallback), msg, m =>
            {
                var res = JsonUtility.FromJson<TTReadResponse>(m);
                if (res == null)
                {
                    Debug.LogWarning("HandleFReadCallback - empty response");
                    return;
                }

                if (!TryTakeCallback(
                        s_readParams,
                        res.callbackId,
                        nameof(HandleFReadCallback),
                        out ReadParam conf))
                {
                    StarkDiscardFileBuffer(res.callbackId);
                    return;
                }
                res.bytesRead = Math.Max(res.bytesRead, 0);

                if (res.errCode == 0)
                {
                    res.arrayBuffer = conf.arrayBuffer;
                    StarkShareFileBuffer(res.arrayBuffer, res.callbackId);
                    conf.success?.Invoke(res);
                }
                else
                {
                    conf.fail?.Invoke(res);
                }
            });

            public void HandleFReadCompressedFileCallback(string msg) => SafeHandle(nameof(HandleFReadCompressedFileCallback), msg, m =>
            {
                var res = JsonUtility.FromJson<TTReadCompressedCallback>(m);
                if (res == null)
                {
                    Debug.LogWarning(
                        "HandleFReadCompressedFileCallback - empty response");
                    return;
                }

                if (!TryTakeCallback(
                        s_readCompressedFileParams,
                        res.callbackId,
                        nameof(HandleFReadCompressedFileCallback),
                        out ReadCompressedFileParam conf))
                {
                    StarkDiscardFileBuffer(res.callbackId);
                    return;
                }

                if (res.errCode == 0)
                {
                    if (res.byteLength < 0)
                    {
                        StarkDiscardFileBuffer(res.callbackId);
                        conf.fail?.Invoke(new TTReadCompressedFileResponse
                        {
                            errCode = -1,
                            errMsg = "invalid compressed response length"
                        });
                        return;
                    }
                    var sharedBuffer = new byte[res.byteLength];
                    StarkShareFileBuffer(sharedBuffer, res.callbackId);
                    var obj = new TTReadCompressedFileResponse()
                    {
                        arrayBuffer = sharedBuffer,
                    };
                    conf.success?.Invoke(obj);
                }
                else
                {
                    var obj = new TTReadCompressedFileResponse()
                    {
                        errCode = res.errCode,
                        errMsg = res.errMsg
                    };
                    conf.fail?.Invoke(obj);
                }
            });

            public void HandleFStatCallback(string msg) => SafeHandle(nameof(HandleFStatCallback), msg, m =>
            {
                var res = JsonMapper.ToObject<FstatResponse>(m);
                if (res == null)
                {
                    Debug.LogWarning("HandleFStatCallback - empty response");
                    return;
                }

                if (!TryTakeCallback(
                        s_fstatParams,
                        res.callbackId,
                        nameof(HandleFStatCallback),
                        out FstatParam conf))
                    return;

                if (res.errCode == 0)
                {
                    conf.success?.Invoke(res);
                }
                else
                {
                    conf.fail?.Invoke(res);
                }
            });
#endregion

        }

        /// <summary>
        /// 将字符串写入文件（同步）
        /// </summary>
        /// <param name="filePath">要写入的文件路径</param>
        /// <param name="data">要写入的文本</param>
        /// <param name="encoding">指定写入文件的字符编码</param>
        /// <returns>成功返回空字符串，失败返回错误信息</returns>
        public override string WriteFileSync(string filePath, string data, string encoding = "utf8")
        {
            if (string.IsNullOrEmpty(encoding))
            {
                encoding = "utf8";
            }

            return StarkWriteStringFileSync(FixFilePath(filePath), data, encoding);
        }

        /// <summary>
        /// 将二进制写入文件（同步）
        /// </summary>
        /// <param name="filePath">要写入的文件路径</param>
        /// <param name="data">要写入的二进制数据</param>
        /// <returns>成功返回空字符串，失败返回错误信息</returns>
        public override string WriteFileSync(string filePath, byte[] data)
        {
            return StarkWriteBinFileSync(FixFilePath(filePath), data, data.Length);
        }

        /// <summary>
        /// 将二进制写入文件（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void WriteFile(WriteFileParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkWriteBinFile(
                FixFilePath(param.filePath),
                param.data,
                param.data.Length,
                pair.success,
                pair.fail
            );
        }

        /// <summary>
        /// 将字符串写入文件（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void WriteFile(WriteFileStringParam param)
        {
            if (string.IsNullOrEmpty(param.encoding))
            {
                param.encoding = "utf8";
            }

            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkWriteStringFile(
                FixFilePath(param.filePath),
                param.data,
                param.encoding,
                pair.success,
                pair.fail
            );
        }

        /// <summary>
        /// 读取本地文件内容（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void ReadFile(ReadFileParam param)
        {
            var key = TTCallbackHandler.MakeKey();
            RegisterPendingCallback(
                s_readFileParams,
                key,
                param,
                pending => pending.fail?.Invoke(
                    new TTReadFileResponse
                    {
                        errCode = -1,
                        errMsg = "callback timeout"
                    }));
            try
            {
                StarkReadFile(
                    FixFilePath(param.filePath),
                    param.encoding,
                    key);
            }
            catch
            {
                RemovePendingCallback(s_readFileParams, key);
                throw;
            }
        }

        /// <summary>
        /// 从本地文件读取二进制数据数据（同步）
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>字节数据，读取失败返回null</returns>
        public override byte[] ReadFileSync(string filePath)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer ||
                (Application.unityVersion.Contains("t") && (int)Application.platform == 0x00000032))
            {
                filePath = FixFilePath(filePath);
                var length = StarkReadBinFileSync(filePath);
                if (length == 0)
                {
                    return null;
                }

                var sharedBuffer = new byte[length];
                StarkShareFileBuffer(sharedBuffer, filePath);
                return sharedBuffer;
            }
            else
            {
                return System.IO.File.ReadAllBytes(filePath);
            }
        }

        /// <summary>
        /// 从本地文件读取字符串数据（同步）
        /// </summary>
        /// <param name="filePath">要读取的文件的路径</param>
        /// <param name="encoding">指定读取文件的字符编码, 不能为空</param>
        /// <returns>字符串数据，读取失败返回null</returns>
        public override string ReadFileSync(string filePath, string encoding)
        {
            return StarkReadStringFileSync(FixFilePath(filePath), encoding);
        }

        /// <summary>
        /// 判断文件/目录是否存在（同步）
        /// </summary>
        /// <param name="path">要判断是否存在的文件/目录路径</param>
        /// <returns>成功返回 true, 失败返回 false</returns>
        public override bool AccessSync(string path)
        {
            return StarkAccessFileSync(FixFilePath(path));
        }

        /// <summary>
        /// 判断文件/目录是否存在（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void Access(AccessParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkAccessFile(FixFilePath(param.path), pair.success, pair.fail);
        }

        /// <summary>
        /// 复制文件（同步） 
        /// </summary>
        /// <param name="srcPath">源文件路径</param>
        /// <param name="destPath">目标文件路径</param>
        /// <returns>成功返回空字符串，失败返回错误信息</returns>
        public override string CopyFileSync(string srcPath, string destPath)
        {
            return StarkCopyFileSync(FixFilePath(srcPath), FixFilePath(destPath));
        }

        /// <summary>
        /// 复制文件（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void CopyFile(CopyFileParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkCopyFile(FixFilePath(param.srcPath), FixFilePath(param.destPath), pair.success, pair.fail);
        }

        /// <summary>
        /// 重命名文件（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void RenameFile(RenameFileParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkRenameFile(FixFilePath(param.srcPath), FixFilePath(param.destPath), pair.success, pair.fail);
        }

        /// <summary>
        /// 重命名文件（同步）
        /// </summary>
        /// <param name="srcPath">源文件路径</param>
        /// <param name="destPath">目标文件路径</param>
        /// <returns>成功返回空字符串，失败返回错误信息</returns>
        public override string RenameFileSync(string srcPath, string destPath)
        {
            return StarkRenameFileSync(FixFilePath(srcPath), FixFilePath(destPath));
        }

        /// <summary>
        /// 删除文件（同步）
        /// </summary>
        /// <param name="filePath">源文件路径，支持本地路径</param>
        /// <returns>成功返回空字符串，失败返回错误信息</returns>
        public override string UnlinkSync(string filePath)
        {
            return StarkUnlinkSync(FixFilePath(filePath));
        }

        /// <summary>
        /// 删除文件（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void Unlink(UnlinkParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkUnlink(FixFilePath(param.filePath), pair.success, pair.fail);
        }

        /// <summary>
        /// 创建目录（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void Mkdir(MkdirParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkMkdir(FixFilePath(param.dirPath), param.recursive, pair.success, pair.fail);
        }

        /// <summary>
        /// 创建目录（同步）
        /// </summary>
        /// <param name="dirPath">创建的目录路径</param>
        /// <param name="recursive">是否在递归创建该目录的上级目录后再创建该目录。如果对应的上级目录已经存在，则不创建该上级目录。如 dirPath 为 a/b/c/d 且 recursive 为 true，将创建 a 目录，再在 a 目录下创建 b 目录，以此类推直至创建 a/b/c 目录下的 d 目录。</param>
        /// <returns>成功返回空字符串，失败返回错误信息</returns>
        public override string MkdirSync(string dirPath, bool recursive = false)
        {
            return StarkMkdirSync(FixFilePath(dirPath), recursive);
        }

        /// <summary>
        /// 删除目录（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void Rmdir(RmdirParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkRmdir(FixFilePath(param.dirPath), param.recursive, pair.success, pair.fail);
        }

        /// <summary>
        /// 删除目录（同步）
        /// </summary>
        /// <param name="dirPath">创建的目录路径</param>
        /// <param name="recursive">是否递归删除目录。如果为 true，则删除该目录和该目录下的所有子目录以及文件	。</param>
        /// <returns>成功返回空字符串，失败返回错误信息</returns>
        public override string RmdirSync(string dirPath, bool recursive = false)
        {
            return StarkRmdirSync(FixFilePath(dirPath), recursive);
        }

        /// <summary>
        /// 读取文件描述信息（同步）
        /// </summary>
        /// <param name="path">文件/目录路径</param>
        /// <param name="throwException">是否抛出错误信息，如果抛出错误信息，当文件不存在时则会抛出异常，错误信息从异常中获取。</param>
        /// <returns>返回文件信息，如果访问失败则返回null</returns>
        public override TTStatInfo StatSync(string path, bool throwException = false)
        {
            var info = StarkStatSync(FixFilePath(path));
            try
            {
                return JsonUtility.FromJson<TTStatInfo>(info);
            }
            catch (Exception)
            {
                if (throwException)
                {
                    if (string.IsNullOrEmpty(info))
                    {
                        info = "stat failed";
                    }

                    throw new Exception(info);
                }

                return null;
            }
        }

        /// <summary>
        /// 读取文件描述信息（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void Stat(StatParam param)
        {
            var key = TTCallbackHandler.MakeKey();
            RegisterPendingCallback(
                s_statParams,
                key,
                param,
                pending => pending.fail?.Invoke(
                    new TTStatResponse
                    {
                        errCode = -1,
                        errMsg = "callback timeout",
                        stat = new TTStatInfo()
                    }));
            try
            {
                StarkStat(FixFilePath(param.path), key);
            }
            catch
            {
                RemovePendingCallback(s_statParams, key);
                throw;
            }
        }

        /// <summary>
        /// 获取保存的用户目录文件列表
        /// </summary>
        /// <param name="param"></param>
        public override void GetSavedFileList(GetSavedFileListParam param)
        {
            var key = TTCallbackHandler.MakeKey();
            RegisterPendingCallback(
                s_getSavedFileListParams,
                key,
                param,
                pending => pending.fail?.Invoke(
                    new TTGetSavedFileListResponse
                    {
                        errCode = -1,
                        errMsg = "callback timeout",
                        fileList = Array.Empty<TTFileInfo>()
                    }));
            try
            {
                StarkGetSavedFileList(key);
            }
            catch
            {
                RemovePendingCallback(s_getSavedFileListParams, key);
                throw;
            }
        }
        
        /// <summary>
        /// 根据url链接获取本地缓存文件路径
        /// </summary>
        /// <param name="url">输入文件下载链接url</param>
        /// <returns>返回本地缓存文件路径，以scfile://user开头的路径，可以直接用这个路径访问该文件</returns>
        public override string GetLocalCachedPathForUrl(string url)
        {
            string path = StarkGetCachedPathForUrl(url);
            const string nativeUserDataPath = "ttfile://user";
            if (path == nativeUserDataPath)
            {
                return USER_DATA_PATH;
            }
            if (path != null && path.StartsWith(
                    nativeUserDataPath + "/",
                    StringComparison.Ordinal))
            {
                return USER_DATA_PATH + path.Substring(
                    nativeUserDataPath.Length);
            }
            return path;
        }

        /// <summary>
        /// 判断该url是否有本地缓存文件
        /// </summary>
        /// <param name="url">输入文件下载链接url</param>
        /// <returns>如果存在缓存文件则返回true，不存在缓存文件则返回false</returns>
        public override bool IsUrlCached(string url)
        {
            var path = GetLocalCachedPathForUrl(url);
            return !string.IsNullOrEmpty(path) && AccessSync(path);
        }
        
        public override string AppendFileSync(string filePath, string data, string encoding = "utf8")
        {
            TraceLog($"AppendFileSync filePath:{filePath},data:{data},encoding:{encoding}");
            if (string.IsNullOrEmpty(encoding))
            {
                encoding = "utf8";
            }

            return StarkAppendStringFileSync(FixFilePath(filePath), data, encoding);
        }

        public override string AppendFileSync(string filePath, byte[] data)
        {
            TraceLog($"AppendFileSync filePath:{filePath},data:{data}");
            return StarkAppendBinFileSync(FixFilePath(filePath), data, data.Length);
        }

        public override void AppendFile(AppendFileStringParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkAppendStringFile(
                FixFilePath(param.FilePath),
                param.Data,
                param.Encoding,
                pair.success,
                pair.fail);
        }

        public override void AppendFile(AppendFileParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkAppendBinFile(
                FixFilePath(param.FilePath),
                param.Data,
                param.Data.Length,
                pair.success,
                pair.fail);
        }
        
        
        public override void ReadDir(ReadDirParam param)
        {
            var key = TTCallbackHandler.MakeKey();
            RegisterPendingCallback(
                s_readDirParams,
                key,
                param,
                pending => pending.fail?.Invoke(
                    new TTReadDirResponse
                    {
                        errCode = -1,
                        errMsg = "callback timeout",
                        files = Array.Empty<string>()
                    }));
            try
            {
                StarkReadDir(
                    FixFilePath(param.DirPath),
                    key);
            }
            catch
            {
                RemovePendingCallback(s_readDirParams, key);
                throw;
            }
        }

        public override string[] ReadDirSync(string dirPath)
        {
            var msg = StarkReadDirSync(FixFilePath(dirPath));
            TraceLog($"ReadDirSync callback -->{msg}");
            string[] res;
            try
            {
                res =  JsonMapper.ToObject<string[]>(msg);
            }
            catch (Exception e)
            {
                Debug.LogError($"ReadDirSync ->{msg},{e}");
                res = Array.Empty<string>();
            }
            return res;
        }

        public override void Truncate(TruncateParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkTruncate(
                FixFilePath(param.FilePath),
                param.Length,
                pair.success,
                pair.fail);
        }   

        public override string TruncateSync(string filePath, int length)
        {
            return StarkTruncateSync(
                FixFilePath(filePath),
                length);
        }

        #region 流式读写
        public override void Open(OpenParam param)
        {
            var key = TTCallbackHandler.MakeKey();
            RegisterPendingCallback(
                s_openParams,
                key,
                param,
                pending => pending.fail?.Invoke(
                    new TTOpenResponse
                    {
                        errCode = -1,
                        errMsg = "callback timeout"
                    }));
            try
            {
                TT_FOpenFile(
                    FixFilePath(param.filePath),
                    param.flag,
                    key);
            }
            catch
            {
                RemovePendingCallback(s_openParams, key);
                throw;
            }
        }

        public override string OpenSync(OpenSyncParam param)
        {
            var fd = TT_FOpenFileSync(FixFilePath(param.filePath), param.flag);
            if (fd.Contains("ERROR"))
            {
                throw new Exception(fd);
            }
            return fd;
        }

        public override void Close(CloseParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            TT_FCloseFile(param.fd, pair.success, pair.fail);
        }

        public override void CloseSync(CloseSyncParam param)
        {
            string info = TT_FCloseFileSync(param.fd);
            if (!string.IsNullOrEmpty(info))
            {
                throw new Exception(info);
            }
        }

        public override void Write(WriteBinParam param)
        {
            var key = TTCallbackHandler.MakeKey();
            RegisterPendingCallback(
                s_writeParams,
                key,
                param,
                pending => pending.fail?.Invoke(
                    new TTWriteResponse
                    {
                        errCode = -1,
                        errMsg = "callback timeout"
                    }));
            int length = param.length == null ? -1 : Math.Max(param.length.Value, 0);
            int position = param.position == null ? -1 : Math.Max(param.position.Value, -1);
            try
            {
                TT_FWriteBinFile(
                    param.fd,
                    param.data,
                    param.data.Length,
                    param.offset,
                    length,
                    key,
                    position);
            }
            catch
            {
                RemovePendingCallback(s_writeParams, key);
                throw;
            }
        }

        public override void Write(WriteStringParam param)
        {
            var key = TTCallbackHandler.MakeKey();
            RegisterPendingCallback(
                s_writeParams,
                key,
                param,
                pending => pending.fail?.Invoke(
                    new TTWriteResponse
                    {
                        errCode = -1,
                        errMsg = "callback timeout"
                    }));
            try
            {
                TT_FWriteStringFile(
                    param.fd,
                    param.data,
                    param.encoding,
                    key);
            }
            catch
            {
                RemovePendingCallback(s_writeParams, key);
                throw;
            }
        }

        public override WriteResult WriteSync(WriteBinSyncParam param)
        {
            int length = param.length == null ? -1 : Math.Max(param.length.Value, 0);
            int position = param.position == null ? -1 : Math.Max(param.position.Value, -1);
            var result = TT_FWriteBinFileSync(param.fd, param.data, param.data.Length, param.offset, length, position);
            if (int.TryParse(result, out int bytesWritten))
            {
                return new()
                {
                    bytesWritten = bytesWritten
                };
            }
            else
            {
                throw new Exception(result);
            }
        }

        public override WriteResult WriteSync(WriteStringSyncParam param)
        {
            var result = TT_FWriteStringFileSync(param.fd, param.data, param.encoding);
            if (int.TryParse(result, out int bytesWritten))
            {
                return new()
                {
                    bytesWritten = bytesWritten
                };
            }
            else
            {
                throw new Exception(result);
            }
        }

        public override void Read(ReadParam param)
        {
            var key = TTCallbackHandler.MakeKey();
            RegisterPendingCallback(
                s_readParams,
                key,
                param,
                pending => pending.fail?.Invoke(
                    new TTReadResponse
                    {
                        errCode = -1,
                        errMsg = "callback timeout",
                        arrayBuffer = pending.arrayBuffer
                    }));
            int position = param.position == null ? -1 : Math.Max(param.position.Value, -1);
            try
            {
                TT_FReadFile(
                    param.fd,
                    param.arrayBuffer.Length,
                    param.offset,
                    param.length,
                    position,
                    key);
            }
            catch
            {
                RemovePendingCallback(s_readParams, key);
                throw;
            }
        }

        public override ReadResult ReadSync(ReadSyncParam param)
        {
            int position = param.position == null ? -1 : Math.Max(param.position.Value, -1);
            var result = TT_FReadFileSync(param.fd, param.arrayBuffer.Length, param.offset, param.length, position);
            if (int.TryParse(result, out int bytesRead))
            {
                bytesRead = Math.Max(bytesRead, 0);//根据平台不同，有的会返回-1有的是0
                var sharedBuffer = param.arrayBuffer;
                StarkShareFileBuffer(sharedBuffer, param.fd);
                return new()
                {
                    bytesRead = bytesRead,
                    arrayBuffer = sharedBuffer
                };
            }
            else
            {
                throw new Exception(result);
            }
        }

        public override void ReadCompressedFile(ReadCompressedFileParam param)
        {
            var key = TTCallbackHandler.MakeKey();
            RegisterPendingCallback(
                s_readCompressedFileParams,
                key,
                param,
                pending => pending.fail?.Invoke(
                    new TTReadCompressedFileResponse
                    {
                        errCode = -1,
                        errMsg = "callback timeout"
                    }));
            try
            {
                TT_FReadCompressedFile(
                    FixFilePath(param.filePath),
                    param.compressionAlgorithm,
                    key);
            }
            catch
            {
                RemovePendingCallback(
                    s_readCompressedFileParams,
                    key);
                throw;
            }
        }

        public override byte[] ReadCompressedFileSync(ReadCompressedFileSyncParam param)
        {
            var result = TT_FReadCompressedFileSync(FixFilePath(param.filePath), param.compressionAlgorithm);
            if (int.TryParse(result, out int length))
            {
                var sharedBuffer = new byte[length];
                StarkShareFileBuffer(sharedBuffer, FixFilePath(param.filePath));
                return sharedBuffer;
            }
            else
            {
                throw new Exception(result);
            }
        }

        public override void Fstat(FstatParam param)
        {
            var key = TTCallbackHandler.MakeKey();
            RegisterPendingCallback(
                s_fstatParams,
                key,
                param,
                pending => pending.fail?.Invoke(
                    new FstatResponse
                    {
                        errCode = -1,
                        errMsg = "callback timeout"
                    }));
            try
            {
                TT_Fstat(param.fd, key);
            }
            catch
            {
                RemovePendingCallback(s_fstatParams, key);
                throw;
            }
        }

        public override TTStatInfo FstatSync(FstatSyncParam param)
        {
            var info = TT_FstatSync(param.fd);
            try
            {
                return JsonUtility.FromJson<TTStatInfo>(info);
            }
            catch (Exception)
            {
                throw new Exception(info);
            }
        }

        public override void Ftruncate(FtruncateParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            TT_Ftruncate(param.fd, param.length, pair.success, pair.fail);
        }

        public override void FtruncateSync(FtruncateSyncParam param)
        {
            string info = TT_FtruncateSync(param.fd, param.length);
            if (!string.IsNullOrEmpty(info))
            {
                throw new Exception(info);
            }
        }
        #endregion

    }
}
