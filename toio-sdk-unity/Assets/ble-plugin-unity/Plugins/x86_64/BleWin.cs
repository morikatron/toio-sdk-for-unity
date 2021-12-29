

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN


using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;
using toio.Windows.Data;

namespace toio.Windows
{
    public class BleWin
    {
        private static Action<string, string, int, byte[]> s_discoverAction;
        private static Dictionary<string, BleDiscoverEvents> s_deviceDiscoverEvents = new Dictionary<string, BleDiscoverEvents>();

        private static List<BleWriteRequestData> s_writeRequests = new List<BleWriteRequestData>();
        private static List<BleReadRequestData> s_readRequests = new List<BleReadRequestData>();
        private static Dictionary<BleCharastericsKeyInfo, BleNotifyData> s_notifyEvents = new Dictionary<BleCharastericsKeyInfo, BleNotifyData>();

        private static HashSet<string> s_allreadyCallServiceBuffer = new HashSet<string>();
        private static List<int> s_removeIdxBuffer = new List<int>();
        private static List<string> s_removeKeyBuffer = new List<string>();

        private static bool s_isInitialized = false;

        public static void Initialize(Action initializedAction, Action<string> errorAction = null)
        {
            // clear data
            s_discoverAction = null;
            s_deviceDiscoverEvents.Clear();
            s_writeRequests.Clear();
            s_readRequests.Clear();
            s_notifyEvents.Clear();
            s_isInitialized = false;

            BehaviourProxy.Create(InitAction(initializedAction,errorAction),OnUpdate);
        }
        private static IEnumerator InitAction(Action initializedAction, Action<string> errorAction)
        {
            DllInterface.BleAdapterStatusRequest();
            DllInterface.EBluetoothStatus stat = DllInterface.EBluetoothStatus.None;

            while (stat == DllInterface.EBluetoothStatus.None)
            {
                stat = DllInterface.BleAdapterUpdate();
                yield return null;
            }
            switch (stat)
            {
                case DllInterface.EBluetoothStatus.Fine:
                    s_isInitialized = true;
                    if (initializedAction != null){
                        initializedAction.Invoke();
                    }
                    break;
                case DllInterface.EBluetoothStatus.NotSupportBle:
                    if (errorAction != null){
                        errorAction.Invoke("Bluetooth Adapter not Support BLE Central.");
                    }
                    break;
                case DllInterface.EBluetoothStatus.BluetoothDisable:
                    if (errorAction != null)
                    {
                        errorAction.Invoke("Bluetooth Adapter isn't enabled.");
                    }
                    break;
                default:
                    if (errorAction != null)
                    {
                        errorAction.Invoke("UnknonwError");
                    }
                    break;
            }
        }

        public static void Finalize(Action finalizedAction = null)
        {
            if (!s_isInitialized) { return; }
            //Debug.Log("Finalize");
            DllInterface.FinalizePlugin();
            if(finalizedAction != null) { finalizedAction(); }
        }

        public static void EnableBluetooth(bool enable)
        {
        }

        public static void StartScan(string[] serviceUUIDs, 
            Action<string, string, int, byte[]> discoveredAction = null)
        {
            if (!s_isInitialized) { return; }
            s_discoverAction = discoveredAction;
            DllInterface.ClearScanFilter();
            if (serviceUUIDs != null)
            {
                foreach (var uuid in serviceUUIDs)
                {
                    var uuidHandle = UuidDatabase.GetUuid(uuid);
                    DllInterface.AddScanServiceUuid(uuidHandle);
                }
            }
            DllInterface.StartScan();
        }

        public static void StopScan()
        {
            if (!s_isInitialized) { return; }
            //Debug.Log("StopScan " );
            DllInterface.StopScan();
        }

        public static void ConnectToPeripheral(string identifier, 
            Action<string> connectedPeripheralAction = null,
            Action<string, string> discoveredServiceAction = null,
            Action<string, string, string> discoveredCharacteristicAction = null,
            Action<string> disconnectedPeripheralAction = null)
        {
            if (!s_isInitialized) { return; }
            //Debug.Log("Connect to peripheral " + identifier);
            if (s_deviceDiscoverEvents.ContainsKey(identifier))
            {
                return;
            }
            var addr = DeviceAddressDatabase.GetAddressValue(identifier);
            DllInterface.ConnectDevice(addr);
            var evt = new BleDiscoverEvents(connectedPeripheralAction,
                discoveredServiceAction,
                discoveredCharacteristicAction, 
                disconnectedPeripheralAction);

            s_deviceDiscoverEvents.Add(identifier, evt);
        }

        public static void DisconnectPeripheral(string identifier, 
            Action<string> disconnectedPeripheralAction = null)
        {
            if (!s_isInitialized) { return; }
            //Debug.Log("DisconnectPeripheral " + identifier);

            var addr = DeviceAddressDatabase.GetAddressValue(identifier);
            DllInterface.DisconnectDevice(addr);
        }

        public static void DisconnectAllPeripherals()
        {
            if (!s_isInitialized) { return; }
            DllInterface.DisconnectAllDevice();
        }

        public static void ReadCharacteristic(string identifier,
            string serviceUUID, 
            string characteristicUUID,
            Action<string, string, byte[]> didReadChracteristicAction)
        {
            if (!s_isInitialized) { return; }
            //Debug.Log("ReadCharacteristic " + identifier);
            var addr = DeviceAddressDatabase.GetAddressValue(identifier);
            var serviceHandle = UuidDatabase.GetUuid(serviceUUID);
            var characteristicHandle = UuidDatabase.GetUuid(characteristicUUID);
            var charastricsItem = new BleCharastericsKeyInfo(identifier, serviceUUID, characteristicUUID);

            var readRequestHandle = DllInterface.ReadCharastristicRequest(addr, serviceHandle, characteristicHandle);
            var requestData = new BleReadRequestData(charastricsItem, readRequestHandle, didReadChracteristicAction);
            s_readRequests.Add(requestData);
        }

        public static void WriteCharacteristic(string identifier,
            string serviceUUID, string characteristicUUID,
            byte[] data, int length, bool withResponse,
            Action<string, string> didWriteCharacteristicAction)
        {
            if (!s_isInitialized) { return; }
            //Debug.Log("WriteCharacteristic " + identifier);
            var addr = DeviceAddressDatabase.GetAddressValue(identifier);

            var serviceHandle = UuidDatabase.GetUuid(serviceUUID);
            var characteristicHandle = UuidDatabase.GetUuid(characteristicUUID);
            var writeRequest = DllInterface.WriteCharastristicRequest(addr, serviceHandle, characteristicHandle, data,0,length);

            if (withResponse && didWriteCharacteristicAction == null)
            {
                var charastricsItem = new BleCharastericsKeyInfo(identifier, serviceUUID, characteristicUUID);
                var requestData = new BleWriteRequestData(charastricsItem, writeRequest, didWriteCharacteristicAction);
                s_writeRequests.Add(requestData);
            }
            else { 
                DllInterface.ReleaseWriteRequest(addr, writeRequest);
            }
        }

        public static void SubscribeCharacteristic(string identifier,
            string serviceUUID, string characteristicUUID,
            Action<string, string, byte[]> notifiedCharacteristicAction)
        {
            //Debug.Log("SubscribeCharacteristic " + identifier + ":" + serviceUUID + ":" + characteristicUUID);
            var addr = DeviceAddressDatabase.GetAddressValue(identifier);
            var serviceHandle = UuidDatabase.GetUuid(serviceUUID);
            var characteristicHandle = UuidDatabase.GetUuid(characteristicUUID);
            var charastricsItem = new BleCharastericsKeyInfo(identifier, serviceUUID, characteristicUUID);

            s_notifyEvents[charastricsItem] = new BleNotifyData(notifiedCharacteristicAction);
            DllInterface.SetNotificationRequest(addr, serviceHandle,characteristicHandle , true);
        }

        public static void UnSubscribeCharacteristic(string identifier, 
            string serviceUUID, string characteristicUUID,
            Action<string> action)
        {
            if (!s_isInitialized) { return; }
            //Debug.Log("UnSubscribeCharacteristic " + identifier + ":" + serviceUUID + ":" + characteristicUUID); 
            var addr = DeviceAddressDatabase.GetAddressValue(identifier);
            var serviceHandle = UuidDatabase.GetUuid(serviceUUID);
            var characteristicHandle = UuidDatabase.GetUuid(characteristicUUID);
            var charastricsItem = new BleCharastericsKeyInfo(identifier, serviceUUID, characteristicUUID);
            DllInterface.SetNotificationRequest(addr, serviceHandle, characteristicHandle, false);
            s_notifyEvents.Remove(charastricsItem);
        }

        private static void OnUpdate()
        {
            if (!s_isInitialized) { return; }
            DllInterface.UpdateFromMainThread();
            UpdateScanDeviceEvents();
            UpdateDeviceConnectEvents();
            UpdateWriteRequests();
            UpdateReadRequests();
            UpdateNotification();
            UpdateDisconnectedDevice();
        }

        private static void UpdateScanDeviceEvents()
        {
            if (!s_isInitialized) { return; }
            // update scan devices
            int scanNum = DllInterface.ScanGetDeviceLength();
            // Debug.Log("UpdateScanDeviceEvents " + scanNum);
            for (int i = 0; i < scanNum; ++i)
            {
                ulong addr = DllInterface.ScanGetDeviceAddr(i);
                var identifier = DeviceAddressDatabase.GetAddressStr(addr);
                //Debug.Log("UpdateScanDeviceEvents identifier " + identifier);
                var name = "";// DllInterface.ScanGetDeviceName(i);
                //Debug.Log("UpdateScanDeviceEvents name " + name);
                var rssi = DllInterface.ScanGetDeviceRssi(i);
                //Debug.Log("UpdateScanDeviceEvents rssi " + rssi);
                if (s_discoverAction != null)
                {
                    s_discoverAction(identifier, name, rssi, null);
                }
            }
        }

        private static void UpdateDeviceConnectEvents()
        {
            if (!s_isInitialized) { return; }
            s_allreadyCallServiceBuffer.Clear();
            // update connect Event
            int deviceNum = DllInterface.GetConnectDeviceNum();
            //Debug.Log("UpdateDeviceConnectEvents " + deviceNum);
            for (int i = 0; i < deviceNum; ++i)
            {
                var addr = DllInterface.GetConnectDeviceAddr(i);
                string identifier = DeviceAddressDatabase.GetAddressStr(addr);
                BleDiscoverEvents bleDiscoverEvents;
                if( !s_deviceDiscoverEvents.TryGetValue(identifier,out bleDiscoverEvents))
                {
                    continue;
                }
                if (bleDiscoverEvents.callDiscoverEvent)
                {
                    continue;
                }
                if (bleDiscoverEvents.connectedAct != null)
                {
                    bleDiscoverEvents.connectedAct(identifier);
                }

                var deviceHandle = DllInterface.GetConnectDevicePtr(i);
                int chNum = DllInterface.GetDeviceCharastricsNum(deviceHandle);
                for (int j = 0; j < chNum; ++j)
                {
                    var serviceHandle = DllInterface.GetDeviceCharastricServiceUuid(deviceHandle, j);
                    var charaHandle = DllInterface.GetDeviceCharastricUuid(deviceHandle, j);
                    var serviceUuidStr = UuidDatabase.GetUuidStr(serviceHandle);
                    var charaUuidStr = UuidDatabase.GetUuidStr(charaHandle);

                    if (!s_allreadyCallServiceBuffer.Contains(serviceUuidStr))
                    {
                        if (bleDiscoverEvents.discoveredServiceAct != null)
                        {
                            bleDiscoverEvents.discoveredServiceAct(identifier, serviceUuidStr);
                        }
                        s_allreadyCallServiceBuffer.Add(serviceUuidStr);
                    }
                    if (bleDiscoverEvents.discoveredCharacteristicAct != null)
                    {
                        bleDiscoverEvents.discoveredCharacteristicAct(identifier, serviceUuidStr, charaUuidStr);
                    }
                }

                bleDiscoverEvents.callDiscoverEvent = true;
            }
        }

        private static void UpdateWriteRequests()
        {
            if (!s_isInitialized) { return; }
            s_removeIdxBuffer.Clear();
            int count = s_writeRequests.Count;
            for ( int i = 0; i < count; ++i)
            {
                var request = s_writeRequests[i];
                if( DllInterface.IsWriteRequestComplete(request.handle))
                {
                    if (request.didWriteCharacteristicAction != null)
                    {
                        request.didWriteCharacteristicAction(request.charastericsInfo.serviceUUID, request.charastericsInfo.characteristicUUID);
                    }
                    s_removeIdxBuffer.Add(i);
                }else if (DllInterface.IsWriteRequestError(request.handle))
                {
                    s_removeIdxBuffer.Add(i);
                }                
            }
            // remove done request
            s_removeIdxBuffer.Reverse();
            foreach( var idx in s_removeIdxBuffer)
            {
                var request = s_writeRequests[idx];
                var addr = DeviceAddressDatabase.GetAddressValue( request.charastericsInfo.address);
                DllInterface.ReleaseWriteRequest(addr,request.handle);
                s_writeRequests.RemoveAt(idx);
            }
        }
        private static void UpdateReadRequests()
        {
            if (!s_isInitialized) { return; }
            s_removeIdxBuffer.Clear();
            int count = s_readRequests.Count;
            for (int i = 0; i < count; ++i)
            {
                var request = s_readRequests[i];
                if (DllInterface.IsReadRequestComplete(request.handle))
                {
                    if (request.didReadChracteristicAction != null)
                    {
                        var data = DllInterface.GetReadRequestData(request.handle, 32);
                        request.didReadChracteristicAction(request.charastericsInfo.serviceUUID, request.charastericsInfo.characteristicUUID,data);
                    }
                    s_removeIdxBuffer.Add(i);
                }
                else if (DllInterface.IsReadRequestError(request.handle))
                {
                    s_removeIdxBuffer.Add(i);
                }

            }
            // remove done request
            s_removeIdxBuffer.Reverse();
            foreach (var idx in s_removeIdxBuffer)
            {
                var request = s_readRequests[idx];
                var addr = DeviceAddressDatabase.GetAddressValue(request.charastericsInfo.address);
                DllInterface.ReleaseReadRequest(addr, request.handle);
                s_readRequests.RemoveAt(idx);
            }
        }
        
        private static void UpdateNotification()
        {
            if (!s_isInitialized) { return; }
            int deviceNum = DllInterface.GetConnectDeviceNum();
            for(int i = 0; i < deviceNum; ++i)
            {
                ulong addr = DllInterface.GetConnectDeviceAddr(i);
                string identifier = DeviceAddressDatabase.GetAddressStr(addr);

                int num = DllInterface.GetDeviceNotificateNum(addr);
                //Debug.Log("UpdateNotification " + identifier + "::" +num + "  " + i + "/" + deviceNum );
                for (int j = 0; j < num; ++j)
                {
                    var data = DllInterface.GetDeviceNotificateData(addr, j);
                    var serviceHandle = DllInterface.GetDeviceNotificateServiceUuid(addr, j);
                    var charastricHandle = DllInterface.GetDeviceNotificateCharastricsUuid(addr, j);
                    string serviceUUID = UuidDatabase.GetUuidStr(serviceHandle);
                    string characteristicUUID = UuidDatabase.GetUuidStr(charastricHandle);

                    var key = new BleCharastericsKeyInfo(identifier, serviceUUID, characteristicUUID);
                    BleNotifyData bleNotifyData;
                    if(s_notifyEvents.TryGetValue(key,out bleNotifyData))
                    {
                        if (bleNotifyData.notifiedCharacteristicAction != null)
                        {
                            bleNotifyData.notifiedCharacteristicAction(serviceUUID, characteristicUUID, data);
                        }
                    }
                }
            }
        }

        private static void UpdateDisconnectedDevice()
        {
            if (!s_isInitialized) { return; }
            s_removeKeyBuffer.Clear();
            foreach (var kvs in s_deviceDiscoverEvents)
            {
                string identifier = kvs.Key;
                var addr = DeviceAddressDatabase.GetAddressValue(identifier);
                var discoverEvt = kvs.Value;
                if (!discoverEvt.callDiscoverEvent)
                {
                    continue;
                }
                if (DllInterface.IsDeviceConnected(addr))
                {
                    continue;
                }
                if (discoverEvt.disconnectedAct != null)
                {
                    discoverEvt.disconnectedAct(identifier);
                }
                s_removeKeyBuffer.Add(identifier);
                //Debug.Log("DisconnectDevice " + identifier);
            }
            foreach (var key in s_removeKeyBuffer)
            {
                s_deviceDiscoverEvents.Remove(key);
            }
        }

    }
}
#endif
