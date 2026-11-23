#if UNITY_WEBGL && !UNITY_EDITOR
  #define TT_UDP_ENABLED
#endif

using System;
#if TT_UDP_ENABLED
using System.Runtime.InteropServices;
using AOT;
#endif

namespace TTSDK.Network
{
    public class TTUDPSocketHandler
    {
#if UNITY_EDITOR
        public const string NotImplMessage = "TT UDP Socket not supported on Unity Editor. Test it with Douyin App.";
#else
        public const string NotImplMessage = "TT UDP Socket not supported on current platform.";
#endif

        public delegate void StarkUDPSocketOnMessageCallback(string instanceId, IntPtr ptrMsg, int lenMsg,
            IntPtr ptrLocalInfo, IntPtr ptrRemoteInfo);

#if TT_UDP_ENABLED
        [DllImport("__Internal", EntryPoint = "StarkCreateUDPSocket")]
        public static extern string StarkCreateUDPSocket();

        [DllImport("__Internal", EntryPoint = "StarkUDPSocketClose")]
        public static extern void StarkUDPSocketClose(string id);

        [DllImport("__Internal", EntryPoint = "StarkUDPSocketConnect")]
        public static extern void StarkUDPSocketConnect(string id, string option);

        [DllImport("__Internal", EntryPoint = "StarkUDPSocketOnClose")]
        public static extern void StarkUDPSocketOnClose(string id);

        [DllImport("__Internal", EntryPoint = "StarkUDPSocketOffClose")]
        public static extern void StarkUDPSocketOffClose(string id);

        [DllImport("__Internal", EntryPoint = "StarkUDPSocketOnError")]
        public static extern void StarkUDPSocketOnError(string id);

        [DllImport("__Internal", EntryPoint = "StarkUDPSocketOffError")]
        public static extern void StarkUDPSocketOffError(string id);

        [DllImport("__Internal", EntryPoint = "StarkUDPSocketOnListening")]
        public static extern void StarkUDPSocketOnListening(string id);

        [DllImport("__Internal", EntryPoint = "StarkUDPSocketOffListening")]
        public static extern void StarkUDPSocketOffListening(string id);

        [DllImport("__Internal", EntryPoint = "StarkUDPSocketOnMessage")]
        public static extern void StarkUDPSocketOnMessage(string id, bool needInfo);

        [DllImport("__Internal", EntryPoint = "StarkUDPSocketOffMessage")]
        public static extern void StarkUDPSocketOffMessage(string id);

        [DllImport("__Internal", EntryPoint = "StarkUDPSocketSendString")]
        public static extern void StarkUDPSocketSend(string id, string data, string param);

        [DllImport("__Internal", EntryPoint = "StarkUDPSocketSendBuffer")]
        public static extern void StarkUDPSocketSend(string id, byte[] data, int dataLength, string param);

        [DllImport("__Internal", EntryPoint = "StarkUDPSocketSetTTL")]
        public static extern void StarkUDPSocketSetTTL(string id, double ttl);

        [DllImport("__Internal", EntryPoint = "StarkUDPSocketBind")]
        public static extern int StarkUDPSocketBind(string id, string param);

        [DllImport("__Internal", EntryPoint = "StarkUDPSocketDestroy")]
        public static extern void StarkUDPSocketDestroy(string id);

        [DllImport("__Internal", EntryPoint = "StarkRegisterUDPSocketOnMessageCallback")]
        public static extern void StarkRegisterUDPSocketOnMessageCallback(
            StarkUDPSocketOnMessageCallback callback);
#else
        public static string StarkCreateUDPSocket()
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkUDPSocketClose(string id)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkUDPSocketConnect(string id, string option)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkUDPSocketOnClose(string id)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkUDPSocketOffClose(string id)
        {
            throw new NotSupportedException(NotImplMessage);
        }
        
        public static void StarkUDPSocketOnError(string id)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkUDPSocketOffError(string id)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkUDPSocketOnListening(string id)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkUDPSocketOffListening(string id)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkUDPSocketOnMessage(string id, bool needInfo)
        {
            throw new NotSupportedException(NotImplMessage);
        }
        
        public static void StarkUDPSocketOffMessage(string id)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkUDPSocketSend(
            string id,
            string data,
            string param)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkUDPSocketSend(
            string id,
            byte[] data,
            int dataLength,
            string param)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkUDPSocketSetTTL(string id, double ttl)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static int StarkUDPSocketBind(string id, string param)
        {
            throw new NotSupportedException(NotImplMessage);
        }
        
        public static void StarkUDPSocketDestroy(string id)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkRegisterUDPSocketOnMessageCallback(StarkUDPSocketOnMessageCallback callback)
        {
            throw new NotSupportedException(NotImplMessage);
        }
#endif

#if TT_UDP_ENABLED
        [MonoPInvokeCallback(typeof(StarkUDPSocketOnMessageCallback))]
#endif
        public static void _UDPSocketOnMessageCallback(string instanceId, IntPtr ptrMsg, int lenMsg,
            IntPtr ptrLocalInfo, IntPtr ptrRemoteInfo)
        {
            _messageCallback?.Invoke(
                instanceId,
                ptrMsg,
                lenMsg,
                ptrLocalInfo,
                ptrRemoteInfo);
        }

        private static readonly StarkUDPSocketOnMessageCallback MessageBridgeCallback =
            _UDPSocketOnMessageCallback;
        private static Action<string, IntPtr, int, IntPtr, IntPtr> _messageCallback;

        public static void SetTTUDPSocketMessageCallback(Action<string, IntPtr, int, IntPtr, IntPtr> callback)
        {
            _messageCallback = callback;
            StarkRegisterUDPSocketOnMessageCallback(
                callback == null ? null : MessageBridgeCallback);
        }
    }
}
