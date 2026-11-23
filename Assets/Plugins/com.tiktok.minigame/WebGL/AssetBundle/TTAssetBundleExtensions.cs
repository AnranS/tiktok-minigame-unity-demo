using UnityEngine;

namespace TTSDK
{
    public static class TTAssetBundleExtensions
    {
        public static void TTUnload(this AssetBundle ab, bool unloadAllLoadedObjects)
        {
            if (ReferenceEquals(ab, null))
                return;

            bool released;
            try
            {
                if (ab != null)
                    ab.Unload(unloadAllLoadedObjects);
            }
            finally
            {
                released =
                    TTAssetBundle.ReleaseAssetBundleTracking(ab);
            }

            if (!released &&
                TTAssetBundle.isAbfsReady)
                Debug.LogError("AssetBundle.TTUnload() Failure: Unregistered Asset Bundle. Loaded without TTAssetBundle methods?");
        }
    }
}
