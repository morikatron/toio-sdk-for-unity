//
// Copyright (c) 2020-present, Sony Interactive Entertainment Inc.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using UnityEngine;
using System;
using System.Collections.Generic;
using toio.Android.Data;

namespace toio.Android
{
    public class BleJavaWrapper:IDisposable
    {
        private IntPtr bleManagerCls;
        private IntPtr bleScannerCls;
        private IntPtr bleDeviceCls;
        private IntPtr javaBleManagerObj;

        private ArgJvalueBuilder argBuilder;

        private List<BleScannedDevice> scannedDevices = new List<BleScannedDevice>();
        private List<BleCharacteristicData> readDatas = new List<BleCharacteristicData>();

        public void Initialize()
        {
            this.argBuilder = new ArgJvalueBuilder();
            AndroidJNI.PushLocalFrame(32);
            var contxt = GetContext();
            this.bleManagerCls = GetGlobalRefClass("com/utj/ble/BleManagerObj");
            this.bleScannerCls = GetGlobalRefClass("com/utj/ble/BleScannerObj");
            this.bleDeviceCls = GetGlobalRefClass("com/utj/ble/BleDeviceObj");
            var getInstanceMethod = AndroidJNI.GetStaticMethodID(bleManagerCls, "getInstance",
                "()Lcom/utj/ble/BleManagerObj;");

            var obj = AndroidJNI.CallStaticObjectMethod(bleManagerCls, getInstanceMethod, null);
            javaBleManagerObj = AndroidJNI.NewGlobalRef(obj);

            argBuilder.Clear().Append(ArgJvalueBuilder.GenerateJvalue(contxt.GetRawObject()));
            var initMethod = AndroidJNI.GetMethodID(bleManagerCls, "initialize", "(Landroid/content/Context;)V");
            AndroidJNI.CallVoidMethod(javaBleManagerObj, initMethod, argBuilder.Build());

            AndroidJNI.PopLocalFrame(IntPtr.Zero);
        }
        private IntPtr GetGlobalRefClass(string name)
        {
            var cls = AndroidJNI.FindClass(name);
            return AndroidJNI.NewGlobalRef(cls);
        }

        private AndroidJavaObject GetContext()
        {
            // UnityPlayerƒNƒ‰ƒX‚ðŽæ“¾
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
                    return context;
                }
            }
        }

        public void StartScan(string uuid)
        {
            AndroidJNI.PushLocalFrame(32);
            var scanner = GetScanner();
            var scanMethod = AndroidJNI.GetMethodID(this.bleScannerCls, "startScan", "(Ljava/lang/String;)V");

            this.argBuilder.Clear().Append(ArgJvalueBuilder.GenerateJvalue(uuid));

            AndroidJNI.CallVoidMethod(scanner, scanMethod, this.argBuilder.Build());
            AndroidJNI.PopLocalFrame(IntPtr.Zero);
        }

        public void StartScan(string[] uuids)
        {
            AndroidJNI.PushLocalFrame(32);
            var scanner = GetScanner();
            var startScanMethod = AndroidJNI.GetMethodID(this.bleScannerCls, "startScan", "()V");
            var addScanFilterMethod = AndroidJNI.GetMethodID(this.bleScannerCls, "addScanFilter", "(Ljava/lang/String;)V");
            var clearScanMethod = AndroidJNI.GetMethodID(this.bleScannerCls, "clearScanFilter", "()V");

            AndroidJNI.CallVoidMethod(scanner, clearScanMethod, null);

            foreach (var uuid in uuids)
            {
                this.argBuilder.Clear().Append(ArgJvalueBuilder.GenerateJvalue(uuid));
                AndroidJNI.CallVoidMethod(scanner, clearScanMethod, this.argBuilder.Build() );
            }

            AndroidJNI.CallVoidMethod(scanner, startScanMethod, null);
            AndroidJNI.PopLocalFrame(IntPtr.Zero);
        }

        public void StopScan()
        {
            AndroidJNI.PushLocalFrame(32);
            var scanner = GetScanner();
            var scanMethod = AndroidJNI.GetMethodID(this.bleScannerCls, "stopScan", "()V");
            AndroidJNI.CallVoidMethod(scanner, scanMethod, null);
            AndroidJNI.PopLocalFrame(IntPtr.Zero);
        }

        public void UpdateScannerResult()
        {
            AndroidJNI.PushLocalFrame(32);
            var scanner = GetScanner();
            var blitMethod = AndroidJNI.GetMethodID(this.bleScannerCls, "blit", "()V");
            var getDeviceNumMethod = AndroidJNI.GetMethodID(this.bleScannerCls, "getDeviceNum", "()I");
            var getDeviceAddrMethod = AndroidJNI.GetMethodID(this.bleScannerCls, "getDeviceAddr", "(I)Ljava/lang/String;");
            var getDeviceNameByAddrMethod = AndroidJNI.GetMethodID(this.bleScannerCls, "getDeviceNameByAddr", "(Ljava/lang/String;)Ljava/lang/String;");
            var getRssiByAddrMethod = AndroidJNI.GetMethodID(this.bleScannerCls, "getRssiByAddr", "(Ljava/lang/String;)I");

            AndroidJNI.CallVoidMethod(scanner, blitMethod, null);
            int num = AndroidJNI.CallIntMethod(scanner, getDeviceNumMethod, null);
            scannedDevices.Clear();
            for (int i = 0; i < num; ++i)
            {
                argBuilder.Clear().Append(ArgJvalueBuilder.GenerateJvalue(i));
                string addr = AndroidJNI.CallStringMethod(scanner, getDeviceAddrMethod, argBuilder.Build());
                argBuilder.Clear().Append(ArgJvalueBuilder.GenerateJvalue(addr));
                string name = AndroidJNI.CallStringMethod(scanner, getDeviceNameByAddrMethod, argBuilder.Build());
                int rssi = AndroidJNI.CallIntMethod(scanner, getRssiByAddrMethod, argBuilder.Build());
                var scanDevice = new BleScannedDevice(addr, name, rssi);
                this.scannedDevices.Add(scanDevice);
            }
            AndroidJNI.PopLocalFrame(IntPtr.Zero);
        }

        public void ConnectRequest(string addr)
        {
            var connectMethod = AndroidJNI.GetMethodID(this.bleManagerCls, "connect", "(Ljava/lang/String;)Lcom/utj/ble/BleDeviceObj;");
            this.argBuilder.Clear().Append(ArgJvalueBuilder.GenerateJvalue(addr));
            AndroidJNI.CallObjectMethod(this.javaBleManagerObj, connectMethod, this.argBuilder.Build());
        }

        public List<BleScannedDevice> GetScannedDevices()
        {
            return this.scannedDevices;
        }

        public void WriteCharacteristic(string addr, string characteristicUUID, byte[] data, int length,
            bool withResponse)
        {
            var deviceObj = GetDeviceObj(addr);
            var writeMethod = AndroidJNI.GetMethodID(bleDeviceCls, "writeData",
                "(Ljava/lang/String;[BZ)V");
            this.argBuilder.Clear().
                Append(ArgJvalueBuilder.GenerateJvalue(characteristicUUID)).
                Append(ArgJvalueBuilder.GenerateJvalue(data, length)).
                Append(ArgJvalueBuilder.GenerateJvalue(withResponse));
            AndroidJNI.CallVoidMethod(deviceObj, writeMethod, this.argBuilder.Build());
        }

        public void SetNotificateFlag(string addr, string characteristicUUID, bool isEnable)
        {
            var deviceObj = GetDeviceObj(addr);
            var setNotificationMethod = AndroidJNI.GetMethodID(bleDeviceCls, "setNotification",
                "(Ljava/lang/String;Z)V");
            this.argBuilder.Clear().
                Append(ArgJvalueBuilder.GenerateJvalue(characteristicUUID)).
                Append(ArgJvalueBuilder.GenerateJvalue(isEnable));
            AndroidJNI.CallVoidMethod(deviceObj, setNotificationMethod, this.argBuilder.Build());
        }

        public void ReadCharacteristicRequest(string addr, string characteristicUUID )
        {
            var deviceObj = GetDeviceObj(addr);
            var readrequestMethod = AndroidJNI.GetMethodID(bleDeviceCls, "readRequest", "(Ljava/lang/String;)V");
            this.argBuilder.Clear().
                Append(ArgJvalueBuilder.GenerateJvalue(characteristicUUID));
            AndroidJNI.CallVoidMethod(deviceObj, readrequestMethod, this.argBuilder.Build());
        }

        public void UpdateConnectedDevices()
        {
            this.readDatas.Clear();
            AndroidJNI.PushLocalFrame(32);
            var getConnectedDeviceNumMethod = AndroidJNI.GetMethodID(bleManagerCls, 
                "getConnectedDeviceNum", "()I");
            var getConnectedDeviceMethod = AndroidJNI.GetMethodID(bleManagerCls,
                "getConnectedDevice", "(I)Lcom/utj/ble/BleDeviceObj;");
            int num = AndroidJNI.CallIntMethod(javaBleManagerObj, getConnectedDeviceNumMethod, null);
            for (int i = 0; i < num; ++i)
            {
                AndroidJNI.PushLocalFrame(32);
                this.argBuilder.Clear().Append(ArgJvalueBuilder.GenerateJvalue(i));
                var device = AndroidJNI.CallObjectMethod(this.javaBleManagerObj, getConnectedDeviceMethod, argBuilder.Build());
                this.UpdateBleDevice(device);
                AndroidJNI.PopLocalFrame(IntPtr.Zero);
            }
            AndroidJNI.PopLocalFrame(IntPtr.Zero);
        }


        public void Dispose()
        {
            SafeRelease(ref bleManagerCls);
            SafeRelease(ref bleScannerCls);
            SafeRelease(ref bleDeviceCls);
            SafeRelease(ref javaBleManagerObj);
        }
        private void UpdateBleDevice(IntPtr device)
        {
            AndroidJNI.PushLocalFrame(32);
            var blitMethod = AndroidJNI.GetMethodID(bleDeviceCls, "blit", "()V");
            var getAddrMethod = AndroidJNI.GetMethodID(bleDeviceCls, "getAddress", "");
            var readNumMethod = AndroidJNI.GetMethodID(bleDeviceCls, "getReadNum", "()I");
            var getCharacteristicMethod = AndroidJNI.GetMethodID(bleDeviceCls, "getCharacteristicFromReadData", "(I)Lcom/java/util/String");
            var isNotifyMethod = AndroidJNI.GetMethodID(bleDeviceCls, "isNotifyReadData", "(I)Z");
            var getReadDataMethod = AndroidJNI.GetMethodID(bleDeviceCls, "getDataFromReadData", "(I)[B");

            string addr = AndroidJNI.CallStringMethod(device, getAddrMethod,null);
            AndroidJNI.CallVoidMethod(device, blitMethod, null);
            int readNum = AndroidJNI.CallIntMethod(device, readNumMethod,null);
            for( int i = 0; i < readNum; ++i)
            {
                this.argBuilder.Clear().Append(ArgJvalueBuilder.GenerateJvalue(i));
                string charastristic = AndroidJNI.CallStringMethod(device,getCharacteristicMethod,argBuilder.Build() );
                bool isNotify = AndroidJNI.CallBooleanMethod(device, isNotifyMethod, argBuilder.Build());
                var dataObj = AndroidJNI.CallObjectMethod(device, getReadDataMethod, argBuilder.Build());
                var sbytes = AndroidJNI.FromSByteArray(dataObj);
                var characteristicData = new BleCharacteristicData(addr, charastristic, sbytes, isNotify);
                this.readDatas.Add(characteristicData);
            }
            AndroidJNI.PopLocalFrame(IntPtr.Zero);
        }
        private System.IntPtr GetScanner()
        {
            var getScanner = AndroidJNI.GetMethodID(bleManagerCls, "getScanner", "()Lcom/utj/ble/BleScannerObj;");
            var scanner = AndroidJNI.CallObjectMethod(this.javaBleManagerObj, getScanner, null);
            return scanner;
        }
        private System.IntPtr GetDeviceObj(string addr)
        {
            var getDeviceMethod = AndroidJNI.GetMethodID(bleManagerCls, "getDeviceByAddr", "(Ljava/lang/String;)Lcom/utj/ble/BleDeviceObj;");
            this.argBuilder.Clear().Append(ArgJvalueBuilder.GenerateJvalue(addr));
            var deviceObj = AndroidJNI.CallObjectMethod(this.javaBleManagerObj, getDeviceMethod, this.argBuilder.Build());
            return deviceObj;
        }

        private void SafeRelease(ref IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                AndroidJNI.DeleteGlobalRef(ptr);
                ptr = IntPtr.Zero;
            }
        }
    }
}
