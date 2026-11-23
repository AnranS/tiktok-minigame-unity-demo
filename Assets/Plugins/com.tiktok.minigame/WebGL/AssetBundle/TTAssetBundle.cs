using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace TTSDK
{
    public class TTAssetBundle
    {
        public static Dictionary<AssetBundle, string> bundle2path = new Dictionary<AssetBundle, string>();

        private const int StaleEntryPruneInterval = 32;
        private static readonly List<AssetBundle> StaleBundles =
            new List<AssetBundle>();
        private static int _trackedSinceLastPrune;

        public static bool isAbfsReady = CheckReady();

        /// <summary>
        /// The type can be initialized before plugin.js finishes installing
        /// ABFS. Re-check a previous miss at request time so that one early
        /// access cannot disable the optimized path for the whole process.
        /// The public field remains for source/binary compatibility.
        /// </summary>
        internal static bool EnsureAbfsReady()
        {
            if (!isAbfsReady)
                isAbfsReady = CheckReady();
            return isAbfsReady;
        }
        
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal", EntryPoint = "StarkAbfsCheckReady")]
        public static extern bool CheckReady();
#else
        public static bool CheckReady()
        {
            return false;
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal", EntryPoint = "StarkAbfsRegisterAssetBundleUrl")]
        public static extern void RegisterAssetBundleUrl(string path);
#else
        public static void RegisterAssetBundleUrl(string path) {
            
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal", EntryPoint = "StarkAbfsIsAssetBundleUrlRegistered")]
        internal static extern bool IsAssetBundleUrlRegistered(string path);
#else
        internal static bool IsAssetBundleUrlRegistered(string path)
        {
            return false;
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal", EntryPoint = "StarkAbfsUnregisterAssetBundleUrl")]
        public static extern void UnregisterAssetBundleUrl(string path);
#else
        public static void UnregisterAssetBundleUrl(string path) {
            
        }
#endif
        
        public static UnityWebRequest GetAssetBundle(string uri) => GetAssetBundle(uri, 0U);

        public static UnityWebRequest GetAssetBundle(string uri, uint crc)
        {
            bool ownsRegistration = false;
            if (EnsureAbfsReady())
            {
                RegisterAssetBundleUrl(uri);
                ownsRegistration = true;
            }

            try
            {
                return new UnityWebRequest(
                    uri,
                    "GET",
                    new DownloadHandlerTTAssetBundle(
                        uri,
                        crc,
                        ownsRegistration),
                    null);
            }
            catch
            {
                if (ownsRegistration)
                    UnregisterAssetBundleUrl(uri);
                throw;
            }
        }

        internal static void TrackAssetBundle(
            AssetBundle bundle,
            string path)
        {
            if (bundle == null)
                return;

            if (bundle2path == null)
                bundle2path = new Dictionary<AssetBundle, string>();

            _trackedSinceLastPrune++;
            if (_trackedSinceLastPrune >= StaleEntryPruneInterval)
            {
                PruneUnloadedBundles();
                _trackedSinceLastPrune = 0;
            }

            if (bundle2path.TryGetValue(
                    bundle,
                    out string previousPath))
            {
                if (string.Equals(previousPath, path))
                {
                    // AssetBundle.LoadFromFile may return the same wrapper for
                    // overlapping loads. Consume the new registration without
                    // replacing the existing owner's tracking entry.
                    ReleaseRegistration(path);
                    return;
                }
                ReleaseRegistration(previousPath);
            }
            bundle2path[bundle] = path;
        }

        internal static bool ReleaseAssetBundleTracking(AssetBundle bundle)
        {
            // A successfully unloaded UnityEngine.Object compares equal to
            // null. ReferenceEquals still lets us remove its dictionary entry
            // after Unload/UnloadAsync has completed.
            if (bundle2path == null || ReferenceEquals(bundle, null) ||
                !bundle2path.TryGetValue(bundle, out string path))
            {
                return false;
            }

            bundle2path.Remove(bundle);
            ReleaseRegistration(path);
            return true;
        }

        internal static void ReleaseRegistration(string path)
        {
            // Ownership, not the mutable compatibility flag, decides whether
            // a registration must be released. A caller can change
            // isAbfsReady after registration; skipping cleanup in that case
            // strands the plugin's ref-counted URL mapping. The jslib bridge
            // safely no-ops when ABFS is unavailable.
            if (!string.IsNullOrEmpty(path))
                UnregisterAssetBundleUrl(path);
        }

        private static void PruneUnloadedBundles()
        {
            if (bundle2path == null || bundle2path.Count == 0)
                return;

            StaleBundles.Clear();
            foreach (KeyValuePair<AssetBundle, string> entry in bundle2path)
            {
                if (entry.Key == null)
                    StaleBundles.Add(entry.Key);
            }

            foreach (AssetBundle staleBundle in StaleBundles)
            {
                if (bundle2path.TryGetValue(
                        staleBundle,
                        out string stalePath))
                {
                    bundle2path.Remove(staleBundle);
                    ReleaseRegistration(stalePath);
                }
            }
            StaleBundles.Clear();
        }

        /// <summary>
        /// Unload an AssetBundle and clean up the bundle2path tracking entry.
        /// </summary>
        public static void Unload(AssetBundle bundle, bool unloadAllLoadedObjects)
        {
            if (ReferenceEquals(bundle, null))
                return;
            try
            {
                if (bundle != null)
                    bundle.Unload(unloadAllLoadedObjects);
            }
            finally
            {
                ReleaseAssetBundleTracking(bundle);
            }
        }
    }
}
