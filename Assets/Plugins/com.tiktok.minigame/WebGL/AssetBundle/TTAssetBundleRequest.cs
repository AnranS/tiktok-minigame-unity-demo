using System;
using System.Collections;
using UnityEngine;

namespace TTSDK
{
    /// <summary>
    /// Compatibility surface retained for projects that referenced the
    /// historical request type. The abandoned XHR bridge that used to back
    /// this type was never constructed by the provider; local provider loads
    /// now use Unity's AssetBundleCreateRequest directly.
    /// </summary>
    public class TTAssetBundleRequest : AsyncOperation, IEnumerator
    {
        public string Url;
        public bool IsDone;

        private readonly uint _crc;
        private readonly ulong _offset;
        private AssetBundle _bundle;

        public delegate void TTAssetBundleCallback(
            IntPtr idPtr,
            uint errCode,
            IntPtr msgPtr);

        internal TTAssetBundleRequest(
            string url,
            uint crc,
            ulong offset)
        {
            Url = url;
            _crc = crc;
            _offset = offset;
            IsDone = true;
        }

        public AssetBundle assetBundle
        {
            get
            {
                if (_bundle == null && !string.IsNullOrEmpty(Url))
                {
                    bool ownsRegistration = false;
                    if (TTAssetBundle.EnsureAbfsReady())
                    {
                        TTAssetBundle.RegisterAssetBundleUrl(Url);
                        ownsRegistration = true;
                    }

                    try
                    {
                        _bundle = AssetBundle.LoadFromFile(
                            Url,
                            _crc,
                            _offset);
                        if (_bundle != null)
                        {
                            TTAssetBundle.TrackAssetBundle(
                                _bundle,
                                Url);
                            ownsRegistration = false;
                        }
                    }
                    finally
                    {
                        if (ownsRegistration)
                            TTAssetBundle.ReleaseRegistration(Url);
                    }
                }
                return _bundle;
            }
        }

        public void Dispose()
        {
            IsDone = true;
        }

        // Kept only for source compatibility with the historical callback
        // symbol. No runtime bridge registers or calls this method anymore.
        public static void Callback(
            IntPtr idPtr,
            uint errCode,
            IntPtr msgPtr)
        {
        }

        public object Current => null;

        public bool MoveNext() => !IsDone;

        public void Reset() =>
            throw new NotImplementedException("Reset Not Implemented");
    }
}
