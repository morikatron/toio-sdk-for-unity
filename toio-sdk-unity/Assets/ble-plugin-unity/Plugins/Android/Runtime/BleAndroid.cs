
#if UNITY_ANDROID && !UNITY_EDITOR
#define UNITY_ANDROID_RUNTIME
#endif


#if UNITY_ANDROID_RUNTIME

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using toio.Android.Data;

namespace toio.Android
{
    public class BleAndroid
    {
        private static BleBehaviour behaviour;
        private static BleJavaWrapper javaWrapper;
        private static Action<string, string, int, byte[]> s_discoveredAction;
        private static Dictionary<string, BleDeviceEvent> s_deviceEvents = new Dictionary<string, BleDeviceEvent>();


        public static void Initialize(Action initializedAction, Action<string> errorAction = null)
        {
            behaviour = BleBehaviour.Create();
            var req = new BlePermissionRequest(
                ()=>
                {
                    javaWrapper = new BleJavaWrapper();
                    behaviour.AddUpdateAction(OnUpdate);
                    initializedAction();
                }, 
                errorAction);
            behaviour.AddExecute(req.Request(), null);
        }

        public static void Finalize(Action finalizedAction = null)
        {
            if (behaviour != null)
            {
                behaviour.DeleteObject();
            }
            if (javaWrapper != null)
            {
                javaWrapper.Dispose();
            }
        }

        public static void EnableBluetooth(bool enable)
        {
        }


        public static void StartScan(string[] serviceUUIDs, 
            Action<string, string, int, byte[]> discoveredAction = null)
        {
            if (javaWrapper == null) { return; }
            javaWrapper.StartScan(serviceUUIDs);
            s_discoveredAction = discoveredAction;
        }

        public static void StopScan()
        {
            if (javaWrapper == null){ return; }
            javaWrapper.StopScan();
        }

        public static void ConnectToPeripheral(string identifier, 
            Action<string> connectedPeripheralAction = null,
            Action<string, string> discoveredServiceAction = null,
            Action<string, string, string> discoveredCharacteristicAction = null,
            Action<string> disconnectedPeripheralAction = null)
        {
            if(javaWrapper == null) { return; }
            javaWrapper.ConnectRequest(identifier);
            var deviceEvent = new BleDeviceEvent()
            {
                connectedAct = connectedPeripheralAction,
                disconnectedAct = disconnectedPeripheralAction,
                discoveredCharacteristicAct = discoveredCharacteristicAction,
                discoveredServiceAct = discoveredServiceAction
            };
            s_deviceEvents[identifier] = deviceEvent;
        }

        public static void DisconnectPeripheral(string identifier, 
            Action<string> disconnectedPeripheralAction = null)
        {
            if (javaWrapper == null) { return; }
        }

        public static void DisconnectAllPeripherals()
        {
            if (javaWrapper == null) { return; }
        }

        public static void ReadCharacteristic(string identifier,
            string serviceUUID, 
            string characteristicUUID,
            Action<string, string, byte[]> didReadChracteristicAction)
        {
            if (javaWrapper == null) { return; }
            javaWrapper.ReadCharacteristicRequest(identifier, serviceUUID,
                characteristicUUID);
        }

        public static void WriteCharacteristic(string identifier,
            string serviceUUID, string characteristicUUID,
            byte[] data, int length, bool withResponse,
            Action<string, string> didWriteCharacteristicAction)
        {
            if (javaWrapper == null) { return; }
            javaWrapper.WriteCharacteristic(identifier, serviceUUID,
                characteristicUUID,
                data, length, withResponse);
        }

        public static void SubscribeCharacteristic(string identifier,
            string serviceUUID, string characteristicUUID,
            Action<string, string, byte[]> notifiedCharacteristicAction)
        {
            if (javaWrapper == null) { return; }
            javaWrapper.SetNotificateFlag(identifier, serviceUUID,
                characteristicUUID, true);

        }

        public static void UnSubscribeCharacteristic(string identifier, 
            string serviceUUID, string characteristicUUID,
            Action<string> action)
        {
            if (javaWrapper == null) { return; }
            javaWrapper.SetNotificateFlag(identifier,serviceUUID,
                characteristicUUID, false);
        }

        private static void OnUpdate()
        {
            if (javaWrapper == null)
            {
                return;
            }
            javaWrapper.UpdateScannerResult();
            // scan callback
            if (s_discoveredAction != null)
            {
                var scanDevices = javaWrapper.GetScannedDevices();
                foreach (var device in scanDevices)
                {
                    s_discoveredAction(device.address, device.name, device.rssi, null);
                }
            }
            // read/notify
            javaWrapper.UpdateConnectedDevices();
            var readDatas = javaWrapper.GetCharacteristicDatas();
        }
    }
}
#endif
