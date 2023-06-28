using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace toio
{
    public class WebBluetoothScript
    {
#if UNITY_WEBGL
        private static bool isInited = false;
        // RequestDevice
        private static Action<int, string, string> callback_RequestDevice = null;
        private static Action<string> errorCallback_RequestDevice = null;
        // Connect
        private static Dictionary<int, Action<int, int, string>> callbackTable_Connect = new Dictionary<int, Action<int, int, string>>();
        // Disconnected
        private static Dictionary<int, Action<int>> callbackTable_Disconnected = new Dictionary<int, Action<int>>();
        // GetCharacteristic
        private static Dictionary<string, Action<int, string>> callbackTable_GetCharacteristic = new Dictionary<string, Action<int, string>>();
        // GetCharacteristics
        private static Dictionary<int, Action<int, string>> callbackTable_GetCharacteristics = new Dictionary<int, Action<int, string>>();
        // ReadValue
        private static Dictionary<int, Action<int, byte[]>> callbackTable_ReadValue = new Dictionary<int, Action<int, byte[]>>();
        // Notifications
        private static Dictionary<int, Action<byte[]>> callbackTable_StartNotifications = new Dictionary<int, Action<byte[]>>();
#endif


        /// <summary>
        /// BLEデバイスをスキャンする
        /// </summary>
        /// <param name="SERVICE_UUID">BLEデバイスのサービスUUID</param>
        /// <param name="callback">スキャン成功コールバック(int deviceID, string deviceUUID, deviceName)</param>
        /// <param name="errorCallback">スキャン失敗コールバック(string errMsg)</param>
        public static void RequestDevice(string SERVICE_UUID, Action<int, string, string> callback, Action<string> errorCallback)
        {
#if UNITY_WEBGL
            callback_RequestDevice = callback;
            errorCallback_RequestDevice = errorCallback;
            call_RequestDevice(SERVICE_UUID);
#endif
        }

        /// <summary>
        /// BLEデバイスに接続する
        /// </summary>
        /// <param name="deviceID">デバイス発行ID</param>
        /// <param name="SERVICE_UUID">BLEデバイスのサービスUUID</param>
        /// <param name="callback">接続コールバック(int serverID, int serviceID, string serviceUUID)</param>
        /// <param name="disconnectedCallback">切断コールバック(int deviceID)</param>
        public static void Connect(int deviceID, string SERVICE_UUID, Action<int, int, string> callback, Action<int> disconnectedCallback = null)
        {
#if UNITY_WEBGL
            callbackTable_Connect[deviceID] = callback;
            if (null != disconnectedCallback)
            {
                callbackTable_Disconnected[deviceID] = disconnectedCallback;
            }
            call_Connect(deviceID, SERVICE_UUID);
#endif
        }

        /// <summary>
        /// BLEデバイスを切断する
        /// </summary>
        /// <param name="serverID">BLEサーバー発行ID</param>
        public static void Disconnect(int serverID)
        {
#if UNITY_WEBGL
            call_Disconnect(serverID);
#endif
        }

        /// <summary>
        /// Characteristicを取得する
        /// </summary>
        /// <param name="serviceID">BLEサービス発行ID</param>
        /// <param name="characteristicUUID">characteristicUUID</param>
        /// <param name="callback">取得コールバック(int characteristicID, string characteristicUUID)</param>
        public static void GetCharacteristic(int serviceID, string characteristicUUID, Action<int, string> callback)
        {
#if UNITY_WEBGL
            var id = serviceID + characteristicUUID;
            callbackTable_GetCharacteristic[id] = callback;
            call_getCharacteristic(serviceID, characteristicUUID);
#endif
        }

        /// <summary>
        /// 全てのCharacteristicを取得する
        /// </summary>
        /// <param name="serviceID">BLEサービス発行ID</param>
        /// <param name="callback">取得コールバック(int characteristicID, string characteristicUUID)</param>
        public static void GetCharacteristics(int serviceID, Action<int, string> callback)
        {
#if UNITY_WEBGL
            callbackTable_GetCharacteristics[serviceID] = callback;
            call_getCharacteristics(serviceID);
#endif
        }

        /// <summary>
        /// Characteristicにデータを書き込む
        /// </summary>
        /// <param name="characteristicID">characteristic発行ID</param>
        /// <param name="data">書き込みデータ</param>
        public static void WriteValue(int characteristicID, byte[] data)
        {
#if UNITY_WEBGL
            call_WriteValue(characteristicID, data, data.Length);
#endif
        }

        /// <summary>
        /// Characteristicからデータを読み込む
        /// </summary>
        /// <param name="characteristicID">characteristic発行ID</param>
        /// <param name="callback">読み込みコールバック(int characteristicID, byte[] 読み込みデータ)</param>
        public static void ReadValue(int characteristicID, Action<int, byte[]> callback)
        {
#if UNITY_WEBGL
            callbackTable_ReadValue[characteristicID] = callback;
            call_ReadValue(characteristicID);
#endif
        }

        /// <summary>
        /// Characteristicから情報を定期受信出来るように定期購読を開始する
        /// </summary>
        /// <param name="characteristicID">characteristic発行ID</param>
        /// <param name="callback">購読コールバック(byte[] 読み込みデータ)</param>
        public static void StartNotifications(int characteristicID, Action<byte[]> callback)
        {
#if UNITY_WEBGL
            callbackTable_StartNotifications[characteristicID] = callback;
            call_startNotifications(characteristicID);
#endif
        }

        /// <summary>
        /// 定期購読を終了する
        /// </summary>
        /// <param name="characteristicID">characteristic発行ID</param>
        public static void StopNotifications(int characteristicID)
        {
#if UNITY_WEBGL
            call_stopNotifications(characteristicID);
#endif
        }

        public static void Init()
        {
#if UNITY_WEBGL
            if (isInited) return;
            isInited = true;
            InitMethods(return_RequestDevice, return_Connect, callback_Disconnected, return_getCharacteristic, return_getCharacteristics, return_ReadValue, callback_StartNotifications);
            InitErrorMethods(error_RequestDevice);
#endif
        }

#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern IntPtr InitMethods(Action<int, string, string> return_RequestDevice, Action<int, int, int, string> return_Connect, Action<int> callback_Disconnected, Action<int, int, string> return_getCharacteristic, Action<int, int, int, int, string> return_getCharacteristics, Action<int, IntPtr, int> return_ReadValue, Action<int, IntPtr, int> callback_StartNotifications);
        [DllImport("__Internal")]
        private static extern IntPtr InitErrorMethods(Action<string> error_RequestDevice);
        [DllImport("__Internal")]
        private static extern int call_RequestDevice(string SERVICE_UUID);
        [DllImport("__Internal")]
        private static extern int call_Connect(int deviceID, string SERVICE_UUID);
        [DllImport("__Internal")]
        private static extern int call_Disconnect(int serverID);
        [DllImport("__Internal")]
        private static extern int call_getCharacteristic(int serviceID, string characteristicUUID);
        [DllImport("__Internal")]
        private static extern int call_getCharacteristics(int serviceID);
        [DllImport("__Internal")]
        private static extern int call_WriteValue(int characteristicID, byte[] pByteArr, int pByteArrLen);
        [DllImport("__Internal")]
        private static extern int call_ReadValue(int characteristicID);
        [DllImport("__Internal")]
        private static extern int call_startNotifications(int characteristicID);
        [DllImport("__Internal")]
        private static extern int call_stopNotifications(int characteristicID);

        [MonoPInvokeCallback(typeof(Action<int, string, string>))]
        private static void return_RequestDevice(int deviceID, string uuid, string name)
        {
            //Debug.LogFormat("[WebBluetoothScript.return_RequestDevice]デバイスid: {0}, uuid: {1}, name: {2}", deviceID, uuid, name);
            callback_RequestDevice.Invoke(deviceID, uuid, name);
            callback_RequestDevice = null;
            errorCallback_RequestDevice = null;
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void error_RequestDevice(string msg)
        {
            //Debug.LogFormat("[WebBluetoothScript.error_RequestDevice]msg: {0}", msg);
            errorCallback_RequestDevice.Invoke(msg);
            callback_RequestDevice = null;
            errorCallback_RequestDevice = null;
        }

        [MonoPInvokeCallback(typeof(Action<int, int, int, string>))]
        private static void return_Connect(int deviceID, int serverID, int serviceID, string serviceUUID)
        {
            //Debug.LogFormat("[WebBluetoothScript.return_Connect]deviceID: {0}, serverID: {1}, serviceID: {2}, serviceUUID: {3}", deviceID, serverID, serviceID, serviceUUID);

            callbackTable_Connect[deviceID].Invoke(serverID, serviceID, serviceUUID);
            callbackTable_Connect.Remove(deviceID);
        }

        [MonoPInvokeCallback(typeof(Action<int>))]
        private static void callback_Disconnected(int deviceID)
        {
            //Debug.LogFormat("[WebBluetoothScript.callback_Disconnected]deviceID: {0}", deviceID);
            if (callbackTable_Disconnected.ContainsKey(deviceID))
            {
                callbackTable_Disconnected[deviceID].Invoke(deviceID);
            }
        }

        [MonoPInvokeCallback(typeof(Action<int, int, string>))]
        private static void return_getCharacteristic(int serviceID, int characteristicID, string characteristicUUID)
        {
            var str = serviceID + characteristicUUID;
            //Debug.LogFormat("[WebBluetoothScript.return_getCharacteristic]serviceID: {0}, characteristicUUID: {1}, characteristicID: {2}", serviceID, characteristicUUID, characteristicID);

            callbackTable_GetCharacteristic[str].Invoke(characteristicID, characteristicUUID);
            callbackTable_GetCharacteristic.Remove(str);
        }

        [MonoPInvokeCallback(typeof(Action<int, int, int, int, string>))]
        private static void return_getCharacteristics(int serviceID, int len, int idx, int characteristicID, string characteristicUUID)
        {
            //Debug.LogFormat("[WebBluetoothScript.return_getCharacteristics]serviceID: {0}, len: {1}, idx: {2}, characteristicID: {3}, characteristicUUID: {4}", serviceID, len, idx, characteristicID, characteristicUUID);

            callbackTable_GetCharacteristics[serviceID].Invoke(characteristicID, characteristicUUID);
            if (len-1 <= idx)
            {
                callbackTable_GetCharacteristics.Remove(serviceID);
            }
        }

        [MonoPInvokeCallback(typeof(Action<int, IntPtr, int>))]
        private static void return_ReadValue(int characteristicID, IntPtr ptr, int len)
        {
            var bytes = IntPtr2Bytes(ptr, len);
            //Debug.LogFormat("[WebBluetoothScript.return_ReadValue]characteristicID: {0}, buff: {1}", characteristicID, Bytes2String(bytes));
            callbackTable_ReadValue[characteristicID].Invoke(characteristicID, bytes);
        }

        [MonoPInvokeCallback(typeof(Action<int, IntPtr, int>))]
        private static void callback_StartNotifications(int characteristicID, IntPtr ptr, int len)
        {
            var bytes = IntPtr2Bytes(ptr, len);
            //Debug.LogFormat("[WebBluetoothScript.callback_StartNotifications]characteristicID: {0}, buff: {1}", characteristicID, Bytes2String(bytes));
            callbackTable_StartNotifications[characteristicID].Invoke(bytes);
        }

        private static string Bytes2String(byte[] bytes)
        {
            if (1 <= bytes.Length)
            {
                var sb = new System.Text.StringBuilder();
                foreach(var b in bytes)
                {
                    sb.Append(b);
                    sb.Append(',');
                }
                return sb.ToString();
            }
            return "";
        }

        private static byte[] IntPtr2Bytes(IntPtr ptr, int length)
        {
            var bytes = new byte[length];
            Marshal.Copy(ptr, bytes, 0, length);
            return bytes;
        }
#endif
    }
}
