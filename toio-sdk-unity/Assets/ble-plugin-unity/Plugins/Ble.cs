//
// Copyright (c) 2020-present, Sony Interactive Entertainment Inc.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//
#if UNITY_ANDROID && !UNITY_EDITOR
#define UNITY_ANDROID_RUNTIME
#endif

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
#define UNITY_OSX
#endif

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
#define UNITY_WIN
#endif

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace toio
{
    public class Ble
    {


        public static void Initialize(Action initializedAction, Action<string> errorAction = null)
        {
#if UNITY_IOS || MRK_DEV
            toio.BleiOS.Initialize(initializedAction, errorAction);
#elif UNITY_ANDROID_RUNTIME || MRK_DEV
            toio.Android.BleAndroid.Initialize(initializedAction, errorAction);
#elif UNITY_OSX || MRK_DEV
            toio.BlemacOS.Initialize(initializedAction, errorAction);
#elif UNITY_WIN || MRK_DEV
            toio.Windows.BleWin.Initialize(initializedAction, errorAction);
#endif
        }

        public static void Finalize(Action finalizedAction = null)
        {
#if UNITY_IOS || MRK_DEV
            toio.BleiOS.Finalize(finalizedAction);
#endif
#if UNITY_ANDROID_RUNTIME || MRK_DEV
            toio.Android.BleAndroid.Finalize(finalizedAction);
#endif
#if UNITY_OSX || MRK_DEV
            toio.BlemacOS.Finalize(finalizedAction);
#endif
#if UNITY_WIN || MRK_DEV
            toio.Windows.BleWin.Finalize(finalizedAction);
#endif
        }

        public static void EnableBluetooth(bool enable)
        {
#if UNITY_IOS || MRK_DEV
            toio.BleiOS.EnableBluetooth(enable);
#endif
#if UNITY_ANDROID_RUNTIME || MRK_DEV
            toio.Android.BleAndroid.EnableBluetooth(enable);
#endif
#if UNITY_OSX || MRK_DEV
            toio.BlemacOS.EnableBluetooth(enable);
#endif
#if UNITY_WIN || MRK_DEV
            toio.Windows.BleWin.EnableBluetooth(enable);
#endif
        }

        public static void StartScan(string[] serviceUUIDs, Action<(string, string, int, byte[])[]> discoveredAction = null)
        {
#if UNITY_IOS || MRK_DEV
            toio.BleiOS.StartScan(serviceUUIDs, discoveredAction);
#endif
#if UNITY_ANDROID_RUNTIME || MRK_DEV
            toio.Android.BleAndroid.StartScan(serviceUUIDs, discoveredAction);
#endif
#if UNITY_OSX || MRK_DEV
            toio.BlemacOS.StartScan(serviceUUIDs, discoveredAction);
#endif
#if UNITY_WIN || MRK_DEV
            toio.Windows.BleWin.StartScan(serviceUUIDs, discoveredAction);
#endif
        }

        public static void StopScan()
        {
#if UNITY_IOS || MRK_DEV
            toio.BleiOS.StopScan();
#endif
#if UNITY_ANDROID_RUNTIME || MRK_DEV
            toio.Android.BleAndroid.StopScan();
#endif
#if UNITY_OSX || MRK_DEV
            toio.BlemacOS.StopScan();
#endif
#if UNITY_WIN || MRK_DEV
            toio.Windows.BleWin.StopScan();
#endif
        }

        public static void ConnectToPeripheral(string identifier, Action<string> connectedPeripheralAction = null, Action<string, string> discoveredServiceAction = null, Action<string, string, string> discoveredCharacteristicAction = null, Action<string> disconnectedPeripheralAction = null)
        {
#if UNITY_IOS || MRK_DEV
            toio.BleiOS.ConnectToPeripheral(identifier, connectedPeripheralAction, discoveredServiceAction,
                discoveredCharacteristicAction, disconnectedPeripheralAction);
#endif
#if UNITY_ANDROID_RUNTIME || MRK_DEV
            toio.Android.BleAndroid.ConnectToPeripheral(identifier,connectedPeripheralAction,
                discoveredServiceAction,discoveredCharacteristicAction,disconnectedPeripheralAction);
#endif
#if UNITY_OSX || MRK_DEV
            toio.BlemacOS.ConnectToPeripheral(identifier, connectedPeripheralAction, discoveredServiceAction,
                discoveredCharacteristicAction, disconnectedPeripheralAction);
#endif
#if UNITY_WIN || MRK_DEV
            toio.Windows.BleWin.ConnectToPeripheral(identifier, connectedPeripheralAction, discoveredServiceAction,
                discoveredCharacteristicAction, disconnectedPeripheralAction);
#endif
        }

        public static void DisconnectPeripheral(string identifier, Action<string> disconnectedPeripheralAction = null)
        {
#if UNITY_IOS || MRK_DEV
            toio.BleiOS.DisconnectPeripheral(identifier, disconnectedPeripheralAction);
#endif
#if UNITY_ANDROID_RUNTIME || MRK_DEV
            toio.Android.BleAndroid.DisconnectPeripheral(identifier, disconnectedPeripheralAction);
#endif
#if UNITY_OSX || MRK_DEV
            toio.BlemacOS.DisconnectPeripheral(identifier, disconnectedPeripheralAction);
#endif
#if UNITY_WIN || MRK_DEV
            toio.Windows.BleWin.DisconnectPeripheral(identifier, disconnectedPeripheralAction);
#endif
        }

        public static void DisconnectAllPeripherals()
        {
#if UNITY_IOS || MRK_DEV
            toio.BleiOS.DisconnectAllPeripherals();
#endif
#if UNITY_ANDROID_RUNTIME || MRK_DEV
            toio.Android.BleAndroid.DisconnectAllPeripherals();
#endif
#if UNITY_OSX || MRK_DEV
            toio.BlemacOS.DisconnectAllPeripherals();
#endif
#if UNITY_WIN || MRK_DEV
            toio.Windows.BleWin.DisconnectAllPeripherals();
#endif
        }

        public static void ReadCharacteristic(string identifier, string serviceUUID, string characteristicUUID, Action<string, string, byte[]> didReadChracteristicAction)
        {
#if UNITY_IOS || MRK_DEV
            toio.BleiOS.ReadCharacteristic(identifier, serviceUUID,
                characteristicUUID, didReadChracteristicAction);
#endif
#if UNITY_ANDROID_RUNTIME || MRK_DEV
            toio.Android.BleAndroid.ReadCharacteristic(identifier, serviceUUID,characteristicUUID,didReadChracteristicAction);
#endif
#if UNITY_OSX || MRK_DEV
            toio.BlemacOS.ReadCharacteristic(identifier, serviceUUID,
                characteristicUUID, didReadChracteristicAction);
#endif
#if UNITY_WIN || MRK_DEV
            toio.Windows.BleWin.ReadCharacteristic(identifier, serviceUUID,
                characteristicUUID, didReadChracteristicAction);
#endif
        }

        public static void WriteCharacteristic(string identifier, string serviceUUID, string characteristicUUID, byte[] data, int length, bool withResponse, Action<string, string> didWriteCharacteristicAction)
        {
#if UNITY_IOS || MRK_DEV
            toio.BleiOS.WriteCharacteristic(identifier, serviceUUID, characteristicUUID,
                data, length, withResponse, didWriteCharacteristicAction);
#endif
#if UNITY_ANDROID_RUNTIME || MRK_DEV
            toio.Android.BleAndroid.WriteCharacteristic(identifier, serviceUUID, characteristicUUID, data,length,withResponse,didWriteCharacteristicAction);
#endif
#if UNITY_OSX || MRK_DEV
            toio.BlemacOS.WriteCharacteristic(identifier, serviceUUID, characteristicUUID,
                data, length, withResponse, didWriteCharacteristicAction);
#endif
#if UNITY_WIN || MRK_DEV
            toio.Windows.BleWin.WriteCharacteristic(identifier, serviceUUID, characteristicUUID,
                data, length, withResponse, didWriteCharacteristicAction);
#endif
        }

        public static void SubscribeCharacteristic(string identifier, string serviceUUID, string characteristicUUID, Action<string, string, byte[]> notifiedCharacteristicAction)
        {
#if UNITY_IOS || MRK_DEV
            toio.BleiOS.SubscribeCharacteristic(identifier, serviceUUID,
                characteristicUUID, notifiedCharacteristicAction);
#endif
#if UNITY_ANDROID_RUNTIME || MRK_DEV
            toio.Android.BleAndroid.SubscribeCharacteristic(identifier, serviceUUID, characteristicUUID, notifiedCharacteristicAction);
#endif
#if UNITY_OSX || MRK_DEV
            toio.BlemacOS.SubscribeCharacteristic(identifier, serviceUUID,
                characteristicUUID, notifiedCharacteristicAction);
#endif
#if UNITY_WIN || MRK_DEV
            toio.Windows.BleWin.SubscribeCharacteristic(identifier, serviceUUID,
                characteristicUUID, notifiedCharacteristicAction);
#endif
        }

        public static void UnSubscribeCharacteristic(string identifier, string serviceUUID, string characteristicUUID, Action<string> action)
        {
#if UNITY_IOS || MRK_DEV
            toio.BleiOS.UnSubscribeCharacteristic(identifier, serviceUUID, characteristicUUID, action);
#endif
#if UNITY_ANDROID_RUNTIME || MRK_DEV
            toio.Android.BleAndroid.UnSubscribeCharacteristic(identifier, serviceUUID, characteristicUUID, action);
#endif
#if UNITY_OSX || MRK_DEV
            toio.BlemacOS.UnSubscribeCharacteristic(identifier, serviceUUID, characteristicUUID, action);
#endif
#if UNITY_WIN || MRK_DEV
            toio.Windows.BleWin.UnSubscribeCharacteristic(identifier, serviceUUID, characteristicUUID, action);
#endif
        }
    }
}
