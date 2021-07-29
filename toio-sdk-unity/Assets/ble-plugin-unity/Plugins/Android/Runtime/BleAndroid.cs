
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
        private static Dictionary<string, BleDiscoverEvents> s_deviceDiscoverEvents = new Dictionary<string, BleDiscoverEvents>();
        private static Dictionary<BleCharastericsKeyInfo, BleDeviceDataEvents> s_deviceDataEvents = new Dictionary<BleCharastericsKeyInfo, BleDeviceDataEvents>();

        public static void Initialize(Action initializedAction, Action<string> errorAction = null)
        {
            behaviour = BleBehaviour.Create();
            var req = new BlePermissionRequest(
                ()=>
                {
                    javaWrapper = new BleJavaWrapper();
                    javaWrapper.Initialize();
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
            s_discoveredAction = null;
        }

        public static void ConnectToPeripheral(string identifier, 
            Action<string> connectedPeripheralAction = null,
            Action<string, string> discoveredServiceAction = null,
            Action<string, string, string> discoveredCharacteristicAction = null,
            Action<string> disconnectedPeripheralAction = null)
        {
            if(javaWrapper == null) { return; }
            javaWrapper.ConnectRequest(identifier);
            var deviceEvent = new BleDiscoverEvents(
                connectedAct: connectedPeripheralAction,
                discoveredServiceAct: discoveredServiceAction,
                discoveredCharacteristicAct: discoveredCharacteristicAction,
                disconnectedAct: disconnectedPeripheralAction
            );
            s_deviceDiscoverEvents[identifier] = deviceEvent;
        }

        public static void DisconnectPeripheral(string identifier, 
            Action<string> disconnectedPeripheralAction = null)
        {
            if (javaWrapper == null) { return; }
            javaWrapper.Disconnect(identifier);
        }
        private static void RemoveDeviceDataInStaticVars(string identifier)
        {
            s_deviceDiscoverEvents.Remove(identifier);
            List<BleCharastericsKeyInfo> removeKeys = new List<BleCharastericsKeyInfo>(s_deviceDataEvents.Count);

            foreach( var key in s_deviceDataEvents.Keys)
            {
                if(key.IsSameAddress(identifier))
                {
                    removeKeys.Add(key);
                }
            }
            foreach( var key in removeKeys)
            {
                s_deviceDataEvents.Remove(key);
            }
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
            var dataEvt = GetDataEvent(identifier, serviceUUID, characteristicUUID);
            dataEvt.SetReadAct(didReadChracteristicAction);
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
            var dataEvt = GetDataEvent(identifier, serviceUUID, characteristicUUID);
            // todo callback実装
            if (withResponse)
            {
                dataEvt.SetWriteAct(didWriteCharacteristicAction);
            }
        }

        public static void SubscribeCharacteristic(string identifier,
            string serviceUUID, string characteristicUUID,
            Action<string, string, byte[]> notifiedCharacteristicAction)
        {
            if (javaWrapper == null) { return; }
            javaWrapper.SetNotificateFlag(identifier, serviceUUID,
                characteristicUUID, true);

            var dataEvt = GetDataEvent(identifier, serviceUUID, characteristicUUID);
            dataEvt.SetNotifyAct(notifiedCharacteristicAction);
        }

        public static void UnSubscribeCharacteristic(string identifier, 
            string serviceUUID, string characteristicUUID,
            Action<string> action)
        {
            if (javaWrapper == null) { return; }
            javaWrapper.SetNotificateFlag(identifier,serviceUUID,
                characteristicUUID, false);
            var dataEvt = GetDataEvent(identifier, serviceUUID, characteristicUUID);
            // todo 引数が良くわからない…
            dataEvt.RemoveNotifyAct();
        }

        private static BleDeviceDataEvents GetDataEvent(string identifier,
            string serviceUUID, string characteristicUUID)
        {
            BleCharastericsKeyInfo key = new BleCharastericsKeyInfo(identifier, serviceUUID, characteristicUUID);
            BleDeviceDataEvents data = null;
            if(s_deviceDataEvents.TryGetValue(key , out data))
            {
                return data;
            }
            data = new BleDeviceDataEvents();
            s_deviceDataEvents.Add(key, data);
            return data;

        }

        private static void OnUpdate()
        {
            if (javaWrapper == null)
            {
                return;
            }
            // devices
            javaWrapper.UpdateConnectedDevices();

            UpdateScanResult();
            UpdateDeviceFoundEvents();
            UpdateDeviceData();
            UpdateDisconnectedDevices();
        }

        private static void UpdateScanResult()
        {
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
        }
        private static void UpdateDeviceFoundEvents()
        {

            // charastric / service found Infos
            var services = new HashSet<string>();

            var charstricInfoByDevice = javaWrapper.GetCharastricKeyInfos();
            foreach (var kvs in s_deviceDiscoverEvents)
            {
                string addr = kvs.Key;
                var deviceEvent = kvs.Value;
                List<BleCharastericsKeyInfo> charstricInfos = null;

                if (charstricInfoByDevice.TryGetValue(addr, out charstricInfos))
                {
                    if (!deviceEvent.callDiscoverEvent)
                    {
                        // connected
                        deviceEvent.connectedAct(addr);

                        // callback discover service
                        BleCharastericsKeyInfo.GetServices(services, charstricInfos);
                        foreach (var service in services)
                        {
                            deviceEvent.discoveredServiceAct(addr, service);
                        }
                        // callback discover service
                        foreach (var chInfo in charstricInfos)
                        {
                            deviceEvent.discoveredCharacteristicAct(chInfo.address, chInfo.serviceUUID, chInfo.characteristicUUID);
                        }
                        deviceEvent.callDiscoverEvent = true;
                    }
                }

            }

        }
        private static void UpdateDeviceData() {
            // read/notify data
            var readDatas = javaWrapper.GetCharacteristicDatas();

            foreach( var readData in readDatas)
            {
                var key = new BleCharastericsKeyInfo(readData.deviceAddr, readData.serviceUuid, readData.characteristic);
                BleDeviceDataEvents dataEvt = null;
                if( !s_deviceDataEvents.TryGetValue(key,out dataEvt))
                {
                    Debug.LogError("Not Found key");
                    continue;
                }
                if(readData.isNotify)
                {
                    dataEvt.CallNotify(readData.serviceUuid, readData.characteristic, readData.data);
                }
                else
                {
                    dataEvt.CallRead(readData.serviceUuid, readData.characteristic, readData.data);
                }
            }
        }
        private static void UpdateDisconnectedDevices()
        {
            javaWrapper.UpdateDisconnectedDevices();
            BleDiscoverEvents evt;
            var disconnectedDevices = javaWrapper.GetDisconnectedDevices();
            foreach( var device in disconnectedDevices)
            {
                if( s_deviceDiscoverEvents.TryGetValue(device,out evt))
                {
                    evt.disconnectedAct(device);
                    RemoveDeviceDataInStaticVars(device);
                }
            }
        }
    }
}
#endif
