using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace TTSDK
{
    /**
     * 只是用来承载 asset bundle 的获取，
     * TTAssetBundle.load 触发下载后，这里并不会获取到真正的 Asset Bundle 数据
     */
    public class DownloadHandlerTTAssetBundle : DownloadHandlerScript
    {
        // 刻意以本类自有完成状态遮蔽基类 DownloadHandler.isDone（异步下载语义不同）
        public new bool isDone;
        
        private string _uri;
        private uint _crc;
        private AssetBundle _assetBundle;
        private MemoryStream _contentStream;
        private int _contentLength;
        // [V2 B.5] GetData() 的 ToArray 结果缓存，避免业务多次访问 .data 重复 ToArray 复制
        private byte[] _cachedData;
        // Unity can keep reading an uncompressed/LZ4 AssetBundle stream while
        // assets are loaded. Once LoadFromStream succeeds, the stream must
        // therefore outlive both this handler and its UnityWebRequest. Unity
        // retains the managed stream until the AssetBundle is unloaded; do
        // not invalidate it from Dispose(). MemoryStream owns no unmanaged
        // resource and its buffer becomes collectible when Unity releases it.
        private bool _streamLifetimeTransferred;
        private readonly bool _useAbfs;
        private bool _ownsRegistration;
        
#if !(UNITY_WEBGL && !UNITY_EDITOR)
        private static bool _isFallbackNoticed;
#endif
        
        public DownloadHandlerTTAssetBundle(string uri, uint crc)
            : this(
                uri,
                crc,
                TTAssetBundle.IsAssetBundleUrlRegistered(uri),
                false)
        {
        }

        internal DownloadHandlerTTAssetBundle(
            string uri,
            uint crc,
            bool ownsRegistration)
            : this(
                uri,
                crc,
                ownsRegistration,
                ownsRegistration)
        {
        }

        private DownloadHandlerTTAssetBundle(
            string uri,
            uint crc,
            bool useAbfs,
            bool ownsRegistration)
        {
            _uri = uri;
            _crc = crc;
            _useAbfs = useAbfs;
            _ownsRegistration = ownsRegistration;
        }

        [Preserve]
        public AssetBundle assetBundle
        {
            get
            {
                if (_assetBundle == null)
                {
                    // Capture the transport mode independently from
                    // registration ownership. The public constructor follows
                    // the URL's actual registration state instead of treating
                    // global ABFS readiness as proof that this particular URL
                    // is registered. This keeps explicit caller-owned ABFS and
                    // ordinary fallback requests both deterministic.
                    if (_useAbfs)
                    {
                        if (_contentLength != 0)
                        {
                            Debug.LogError($"DownloadHandlerTTAssetBundle contentLength not 0!");
                            return null;
                        }
                        try
                        {
                            _assetBundle =
                                AssetBundle.LoadFromFile(_uri, _crc);
                        }
                        catch
                        {
                            if (_ownsRegistration)
                            {
                                _ownsRegistration = false;
                                TTAssetBundle.ReleaseRegistration(_uri);
                            }
                            throw;
                        }
                        if (_assetBundle != null)
                        {
                            TTAssetBundle.TrackAssetBundle(
                                _assetBundle,
                                _uri);
                            _ownsRegistration = false;
                        }
                        else if (_ownsRegistration)
                        {
                            TTAssetBundle.ReleaseRegistration(_uri);
                            _ownsRegistration = false;
                        }
                    }
                    else if (_contentStream != null)
                    {
                        // [V2 B.5] 用 AssetBundle.LoadFromStream 替代 LoadFromMemory + ToArray()，
                        // 避免在 fallback 路径上把整包 AB 内容再复制一份到 LOH。
                        // 实测收益约等于一份 AB 字节数；绝对峰值仍取决于
                        // MemoryStream capacity 的扩容余量与 Unity 内部 allocator，
                        // 不能简单写成固定的“2× → 1×”。
                        _contentStream.Seek(0, SeekOrigin.Begin);
                        _assetBundle = AssetBundle.LoadFromStream(_contentStream, _crc);
                        _streamLifetimeTransferred = _assetBundle != null;
                    }
                    else
                    {
                        _assetBundle = AssetBundle.LoadFromMemory(Array.Empty<byte>());
                    }
                }
                return _assetBundle;
            }
        }

        [Preserve]
        protected override void ReceiveContentLengthHeader(
            ulong contentLength)
        {
            // Pre-size the fallback buffer when the server provides a usable
            // length. This avoids MemoryStream's geometric growth and also
            // makes it possible for GetData() to return the exact backing
            // array without another full AssetBundle-sized copy.
            if (_contentStream == null &&
                contentLength > 0 &&
                contentLength <= int.MaxValue)
            {
                _contentStream = new MemoryStream((int) contentLength);
            }
        }

        [Preserve]
        protected override byte[] GetData()
        {
            // [V2 B.5] 缓存 ToArray() 结果。业务代码（如 UnityWebRequest.downloadHandler.data）
            // 多次访问只触发一次复制，避免每次访问都生成 ~AB 体积大小的新 byte[]。
            if (_cachedData != null) return _cachedData;
            if (_contentStream == null) return null;

            if (_contentStream.TryGetBuffer(
                    out ArraySegment<byte> buffer) &&
                buffer.Offset == 0 &&
                buffer.Count == _contentStream.Length &&
                buffer.Array != null &&
                buffer.Array.Length == _contentStream.Length)
            {
                _cachedData = buffer.Array;
            }
            else
            {
                _cachedData = _contentStream.ToArray();
            }
            return _cachedData;
        }

        [Preserve]
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength < 1)
                return false;
            
#if !(UNITY_WEBGL && !UNITY_EDITOR)
            if (!_isFallbackNoticed)
            {
                _isFallbackNoticed = true;
                Debug.LogWarning("TTAssetBundle 仅在 WebGL 方案有优化效果，当前环境下回滚到 UnityWebRequestAssetBundle 加载实现。");
            }
#endif
            
            if (_contentStream == null)
                _contentStream = new MemoryStream();
            _contentStream.Write(data, 0, dataLength);
            _contentLength += dataLength;
            return true;
        }

        [Preserve]
        protected override void CompleteContent() => isDone = true;

        public override void Dispose()
        {
            try
            {
                if (_ownsRegistration)
                {
                    _ownsRegistration = false;
                    TTAssetBundle.ReleaseRegistration(_uri);
                }
            }
            finally
            {
                if (!_streamLifetimeTransferred)
                    _contentStream?.Dispose();
                _contentStream = null;
                _cachedData = null;
                base.Dispose();
            }
        }
        
    }
}
