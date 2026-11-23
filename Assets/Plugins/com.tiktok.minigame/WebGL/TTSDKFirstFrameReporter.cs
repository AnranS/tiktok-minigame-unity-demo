#if UNITY_WEBGL && !UNITY_EDITOR
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace TTSDK
{
    /// <summary>
    /// Reports the first Unity frame at the end of the render loop.
    ///
    /// The JavaScript launcher also retains mainCalled + requestAnimationFrame
    /// as a compatibility fallback. Its one-shot scheduler makes duplicate
    /// signals harmless.
    /// </summary>
    [Preserve]
    internal sealed class TTSDKFirstFrameReporter : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void TTSDKNotifyUnityFirstFrameRendered();

        [DllImport("__Internal")]
        private static extern void TTSDKRegisterUnityFirstFrameReporter();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void RegisterCapability()
        {
            try
            {
                TTSDKRegisterUnityFirstFrameReporter();
            }
            catch
            {
                // Older host plugins retain their mainCalled + rAF fallback.
                // Capability discovery must never affect game startup.
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            var host = new GameObject("TTSDK.FirstFrameReporter")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            DontDestroyOnLoad(host);
            host.AddComponent<TTSDKFirstFrameReporter>();
        }

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            try
            {
                TTSDKNotifyUnityFirstFrameRendered();
            }
            finally
            {
                Destroy(gameObject);
            }
        }
    }
}
#endif
