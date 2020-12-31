//
// Copyright (c) 2020-present, Sony Interactive Entertainment Inc.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace toio
{
    public class BleiOS
    {

        // #if UNITY_EDITOR_OSX
        //     private const string DLL_NAME = "bleplugin";
        // #elif UNITY_IOS
        private const string DLL_NAME = "__Internal";
        // #endif

#if UNITY_IOS
	[DllImport (DLL_NAME)]
    private static extern void _uiOSCreateClient(InitializedActionDelegate initializedAction, ErrorActionDelegate errorAction);

	[DllImport (DLL_NAME)]
    private static extern void _uiOSDestroyClient(FinalizedActionDelegate finalizedAction);

	[DllImport (DLL_NAME)]
    private static extern void _uiOSStartDeviceScan(string[] filteredUUIDs, DiscoveredActionDelegate discoveredAction, bool allowDuplicates);

	[DllImport (DLL_NAME)]
    private static extern void _uiOSStopDeviceScan();

	[DllImport (DLL_NAME)]
    private static extern void _uiOSConnectToDevice(string identifier, ConnectedPeripheralActionDelegate connectedPeripheralAction, DiscoveredServiceActionDelegate discoveredServiceAction, DiscoveredCharacteristicActionDelegate discoveredCharacteristicAction, PendingDisconnectedPeripheralActionDelegate disconnectedPeripheralAction);

	[DllImport (DLL_NAME)]
    private static extern void _uiOSCancelDeviceConnection(string identifier, DisconnectedPeripheralActionDelegate disconnectedPeripheralAction);

	[DllImport (DLL_NAME)]
    private static extern void _uiOSCancelDeviceConnectionAll();

	[DllImport (DLL_NAME)]
    private static extern void _uiOSReadCharacteristicForDevice(string identifier, string serviceUUID, string characteristicUUID, DidReadCharacteristicActionDelegate didReadChracteristicAction);

	[DllImport (DLL_NAME)]
    private static extern void _uiOSWriteCharacteristicForDevice(string identifier, string serviceUUID, string characteristicUUID, string data, int length, bool withResponse, DidWriteCharacteristicActionDelegate didWriteCharacteristicAction);

	[DllImport (DLL_NAME)]
    private static extern void _uiOSMonitorCharacteristicForDevice(string identifier, string serviceUUID, string characteristicUUID, NotifiedCharacteristicActionDelegate notifiedCharacteristicAction);

	[DllImport (DLL_NAME)]
    private static extern void _uiOSUnMonitorCharacteristicForDevice(string identifier, string serviceUUID, string characteristicUUID);
#endif

        //
        // ErrorAction
        //
        private static Action<string> ErrorAction = null;
        private delegate void ErrorActionDelegate(string errorCode, string errorMessage, string errorDescription);
        [AOT.MonoPInvokeCallback(typeof(ErrorActionDelegate))]
        private static void ErrorActionCallback(string errorCode, string errorMessage, string errorDescription)
        {
            if (ErrorAction != null)
            {
                ErrorAction(errorDescription);
            }
        }

        //
        // InitializedAction
        //
        private static Action InitializedAction = null;
        private delegate void InitializedActionDelegate();
        [AOT.MonoPInvokeCallback(typeof(InitializedActionDelegate))]
        private static void InitializedActionCallback()
        {
            if (InitializedAction != null)
            {
                InitializedAction();
            }
        }

        //
        // FinalizedAction
        //
        private static Action FinalizedAction = null;
        private delegate void FinalizedActionDelegate();
        [AOT.MonoPInvokeCallback(typeof(FinalizedActionDelegate))]
        private static void FinalizedActionCallback()
        {
            if (FinalizedAction != null)
            {
                FinalizedAction();
            }
        }

        //
        // DiscoveredAction
        //
        private static Action<string, string, int, byte[]> DiscoveredAction = null;
        private delegate void DiscoveredActionDelegate(string identifier, string name, string rssi, string base64ManufacturerData);
        [AOT.MonoPInvokeCallback(typeof(DiscoveredActionDelegate))]
        private static void DiscoveredActionCallback(string identifier, string name, string rssi, string base64ManufacturerData)
        {
            if (DiscoveredAction != null)
            {
                int iRssi = 0;
                if (!int.TryParse(rssi, out iRssi))
                {
                    iRssi = 0;
                }

                byte[] data = null;
                try
                {
                    data = System.Convert.FromBase64String(base64ManufacturerData);
                }
                catch { }

                DiscoveredAction(identifier, name, iRssi, data);
            }
        }


        //
        // ConnectedPeripheralAction
        //
        private static Dictionary<string, Action<string>> ConnectedPeripheralAction = new Dictionary<string, Action<string>>();
        private delegate void ConnectedPeripheralActionDelegate(string identifier);
        [AOT.MonoPInvokeCallback(typeof(ConnectedPeripheralActionDelegate))]
        private static void ConnectedPeripheralActionCallback(string identifier)
        {
            identifier = identifier.ToUpper();
            if (ConnectedPeripheralAction[identifier] != null)
            {
                ConnectedPeripheralAction[identifier](identifier);
            }
        }


        //
        // DiscoveredServiceAction
        //
        private static Dictionary<string, Action<string, string>> DiscoveredServiceAction = new Dictionary<string, Action<string, string>>();
        private delegate void DiscoveredServiceActionDelegate(string identifier, string serviceUUID);
        [AOT.MonoPInvokeCallback(typeof(DiscoveredServiceActionDelegate))]
        private static void DiscoveredServiceActionCallback(string identifier, string serviceUUID)
        {
            identifier = identifier.ToUpper();
            if (DiscoveredServiceAction[identifier] != null)
            {
                DiscoveredServiceAction[identifier](identifier, serviceUUID);
            }
        }

        //
        // DiscoveredCharacteristicAction
        //
        private static Dictionary<string, Action<string, string, string>> DiscoveredCharacteristicAction = new Dictionary<string, Action<string, string, string>>();
        private delegate void DiscoveredCharacteristicActionDelegate(string identifier, string serviceUUID, string characteristicUUID);
        [AOT.MonoPInvokeCallback(typeof(DiscoveredCharacteristicActionDelegate))]
        private static void DiscoveredCharacteristicActionCallback(string identifier, string serviceUUID, string characteristicUUID)
        {
            identifier = identifier.ToUpper();
            if (DiscoveredCharacteristicAction[identifier] != null)
            {
                DiscoveredCharacteristicAction[identifier](identifier, serviceUUID, characteristicUUID);
            }
        }


        //
        // PendingDisconnectedPeripheralAction
        //
        private static Dictionary<string, Action<string>> PendingDisconnectedPeripheralAction = new Dictionary<string, Action<string>>();
        private delegate void PendingDisconnectedPeripheralActionDelegate(string identifier);
        [AOT.MonoPInvokeCallback(typeof(PendingDisconnectedPeripheralActionDelegate))]
        private static void PendingDisconnectedPeripheralActionCallback(string identifier)
        {
            identifier = identifier.ToUpper();
            if (PendingDisconnectedPeripheralAction[identifier] != null)
            {
                PendingDisconnectedPeripheralAction[identifier](identifier);
            }
        }

        //
        // DisconnectedPeripheralAction
        //
        private static Dictionary<string, Action<string>> DisconnectedPeripheralAction = new Dictionary<string, Action<string>>();
        private delegate void DisconnectedPeripheralActionDelegate(string identifier);
        [AOT.MonoPInvokeCallback(typeof(DisconnectedPeripheralActionDelegate))]
        private static void DisconnectedPeripheralActionCallback(string identifier)
        {
            identifier = identifier.ToUpper();
            if (DisconnectedPeripheralAction[identifier] != null)
            {
                DisconnectedPeripheralAction[identifier](identifier);
            }
        }

        //
        // DidReadCharacteristicAction
        //
        private static Dictionary<string, Dictionary<string, Action<string, string, byte[]>>> DidReadCharacteristicAction = new Dictionary<string, Dictionary<string, Action<string, string, byte[]>>>();
        private delegate void DidReadCharacteristicActionDelegate(string identifier, string characteristicUUID, string base64Data);
        [AOT.MonoPInvokeCallback(typeof(DidReadCharacteristicActionDelegate))]
        private static void DidReadCharacteristicActionCallback(string identifier, string characteristicUUID, string base64Data)
        {
            identifier = identifier.ToUpper();
            characteristicUUID = characteristicUUID.ToUpper();
            if (DidReadCharacteristicAction != null && DidReadCharacteristicAction.ContainsKey(identifier))
            {
                var actions = DidReadCharacteristicAction[identifier];
                if (actions != null && actions.ContainsKey(characteristicUUID))
                {
                    var action = actions[characteristicUUID];
                    if (action != null)
                    {
                        byte[] data = System.Convert.FromBase64String(base64Data);
                        action(identifier, characteristicUUID, data);
                    }
                }
            }
        }

        //
        // DidWriteCharacteristicAction
        //
        private static Dictionary<string, Dictionary<string, Action<string, string>>> DidWriteCharacteristicAction = new Dictionary<string, Dictionary<string, Action<string, string>>>();
        private delegate void DidWriteCharacteristicActionDelegate(string identifier, string characteristicUUID);
        [AOT.MonoPInvokeCallback(typeof(DidWriteCharacteristicActionDelegate))]
        private static void DidWriteCharacteristicActionCallback(string identifier, string characteristicUUID)
        {
            identifier = identifier.ToUpper();
            characteristicUUID = characteristicUUID.ToUpper();
            if (DidWriteCharacteristicAction != null && DidWriteCharacteristicAction.ContainsKey(identifier))
            {
                var actions = DidWriteCharacteristicAction[identifier];
                if (actions != null && actions.ContainsKey(characteristicUUID))
                {
                    var action = actions[characteristicUUID];
                    if (action != null)
                    {
                        action(identifier, characteristicUUID);
                    }
                }
            }
        }

        //
        // NotifiedCharacteristicAction
        //
        private static Dictionary<string, Dictionary<string, Action<string, string, byte[]>>> NotifiedCharacteristicAction = new Dictionary<string, Dictionary<string, Action<string, string, byte[]>>>();
        private delegate void NotifiedCharacteristicActionDelegate(string identifier, string characteristicUUID, string base64Data);
        [AOT.MonoPInvokeCallback(typeof(NotifiedCharacteristicActionDelegate))]
        private static void NotifiedCharacteristicActionCallback(string identifier, string characteristicUUID, string base64Data)
        {
            identifier = identifier.ToUpper();
            characteristicUUID = characteristicUUID.ToUpper();
            if (NotifiedCharacteristicAction != null && NotifiedCharacteristicAction.ContainsKey(identifier))
            {
                var actions = NotifiedCharacteristicAction[identifier];
                if (actions != null && actions.ContainsKey(characteristicUUID))
                {
                    var action = actions[characteristicUUID];
                    if (action != null)
                    {
                        byte[] data = System.Convert.FromBase64String(base64Data);
                        action(identifier, characteristicUUID, data);
                    }
                }
            }
        }

        public static void Initialize(Action initializedAction, Action<string> errorAction = null)
        {
#if UNITY_IOS
        InitializedAction = initializedAction;
        ErrorAction = errorAction;
        _uiOSCreateClient(InitializedActionCallback, ErrorActionCallback);
#endif
        }

        public static void Finalize(Action finalizedAction = null)
        {
#if UNITY_IOS
        FinalizedAction = finalizedAction;
        _uiOSDestroyClient(FinalizedActionCallback);
#endif
        }

        public static void EnableBluetooth(bool enable)
        {
        }

        public static void StartScan(string[] serviceUUIDs, Action<string, string, int, byte[]> discoveredAction = null)
        {
#if UNITY_IOS
        DiscoveredAction = discoveredAction;
        _uiOSStartDeviceScan(serviceUUIDs, DiscoveredActionCallback, false);
#endif
        }

        public static void StopScan()
        {
#if UNITY_IOS
        _uiOSStopDeviceScan();
#endif
        }

        public static void ConnectToPeripheral(string identifier, Action<string> connectedPeripheralAction = null, Action<string, string> discoveredServiceAction = null, Action<string, string, string> discoveredCharacteristicAction = null, Action<string> disconnectedPeripheralAction = null)
        {
#if UNITY_IOS
        identifier = identifier.ToUpper();
        ConnectedPeripheralAction[identifier] = connectedPeripheralAction;
        DiscoveredServiceAction[identifier] = discoveredServiceAction;
        DiscoveredCharacteristicAction[identifier] = discoveredCharacteristicAction;
        PendingDisconnectedPeripheralAction[identifier] = disconnectedPeripheralAction;
        _uiOSConnectToDevice(identifier, ConnectedPeripheralActionCallback, DiscoveredServiceActionCallback, DiscoveredCharacteristicActionCallback, PendingDisconnectedPeripheralActionCallback);
#endif
        }

        public static void DisconnectPeripheral(string identifier, Action<string> disconnectedPeripheralAction = null)
        {
#if UNITY_IOS
        identifier = identifier.ToUpper();
        DisconnectedPeripheralAction[identifier] = disconnectedPeripheralAction;
        _uiOSCancelDeviceConnection(identifier, DisconnectedPeripheralActionCallback);
#endif
        }

        public static void DisconnectAllPeripherals()
        {
#if UNITY_IOS
        _uiOSCancelDeviceConnectionAll();
#endif
        }

        public static void ReadCharacteristic(string identifier, string serviceUUID, string characteristicUUID, Action<string, string, byte[]> didReadChracteristicAction)
        {
#if UNITY_IOS
        identifier = identifier.ToUpper();
        serviceUUID = serviceUUID.ToUpper();
        characteristicUUID = characteristicUUID.ToUpper();
        if (!DidReadCharacteristicAction.ContainsKey(identifier)) {
            DidReadCharacteristicAction[identifier] = new Dictionary<string, Action<string, string, byte[]>>();
        }
        DidReadCharacteristicAction[identifier][characteristicUUID] = didReadChracteristicAction;
        _uiOSReadCharacteristicForDevice(identifier, serviceUUID, characteristicUUID, DidReadCharacteristicActionCallback);
#endif
        }

        public static void WriteCharacteristic(string identifier, string serviceUUID, string characteristicUUID, byte[] data, int length, bool withResponse, Action<string, string> didWriteCharacteristicAction)
        {
#if UNITY_IOS
        identifier = identifier.ToUpper();
        serviceUUID = serviceUUID.ToUpper();
        characteristicUUID = characteristicUUID.ToUpper();
        if (!DidWriteCharacteristicAction.ContainsKey(identifier)) {
            DidWriteCharacteristicAction[identifier] = new Dictionary<string, Action<string, string>>();
        }
        DidWriteCharacteristicAction[identifier][characteristicUUID] = didWriteCharacteristicAction;
        _uiOSWriteCharacteristicForDevice(identifier, serviceUUID, characteristicUUID, Convert.ToBase64String(data), length, withResponse, DidWriteCharacteristicActionCallback);
#endif
        }

        public static void SubscribeCharacteristic(string identifier, string serviceUUID, string characteristicUUID, Action<string, string, byte[]> notifiedCharacteristicAction)
        {
#if UNITY_IOS
        identifier = identifier.ToUpper();
        serviceUUID = serviceUUID.ToUpper();
        characteristicUUID = characteristicUUID.ToUpper();
        if (!NotifiedCharacteristicAction.ContainsKey(identifier)) {
            NotifiedCharacteristicAction[identifier] = new Dictionary<string, Action<string, string, byte[]>>();
        }
        NotifiedCharacteristicAction[identifier][characteristicUUID] = notifiedCharacteristicAction;
        _uiOSMonitorCharacteristicForDevice(identifier, serviceUUID, characteristicUUID, NotifiedCharacteristicActionCallback);
#endif
        }

        public static void UnSubscribeCharacteristic(string identifier, string serviceUUID, string characteristicUUID, Action<string> action)
        {
#if UNITY_IOS
        identifier = identifier.ToUpper();
        serviceUUID = serviceUUID.ToUpper();
        characteristicUUID = characteristicUUID.ToUpper();
        if (!NotifiedCharacteristicAction.ContainsKey(identifier)) {
            NotifiedCharacteristicAction[identifier] = new Dictionary<string, Action<string, string, byte[]>>();
        }
        NotifiedCharacteristicAction[identifier][characteristicUUID] = null;
        _uiOSUnMonitorCharacteristicForDevice(identifier, serviceUUID, characteristicUUID);
#endif
        }
    }
}
