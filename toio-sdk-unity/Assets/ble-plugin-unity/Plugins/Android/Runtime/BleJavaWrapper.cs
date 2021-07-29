
#if UNITY_ANDROID && !UNITY_EDITOR
#define UNITY_ANDROID_RUNTIME
#endif


#if UNITY_ANDROID_RUNTIME
using UnityEngine;
using System;
using System.Collections.Generic;
using toio.Android.Data;

namespace toio.Android
{
    public class BleJavaWrapper : IDisposable
    {
        private IntPtr bleManagerCls;
        private IntPtr bleScannerCls;
        private IntPtr bleDeviceCls;
        private IntPtr javaBleManagerObj;

        private ArgJvalueBuilder argBuilder;

        private List<BleScannedDevice> scannedDevices = new List<BleScannedDevice>();
        private List<BleCharacteristicData> readDatas = new List<BleCharacteristicData>();
        private List<string> disconnectedDevices = new List<string>();
        private Dictionary<string, List<BleCharastericsKeyInfo> > charastericsKeyInfos = new Dictionary<string, List<BleCharastericsKeyInfo> >();

        public void Initialize()
        {
            this.argBuilder = new ArgJvalueBuilder();
            AndroidJNI.PushLocalFrame(32);
            var contxt = GetContext();
            this.bleManagerCls = GetGlobalRefClass("com/toio/ble/BleManagerObj");
            this.bleScannerCls = GetGlobalRefClass("com/toio/ble/BleScannerObj");
            this.bleDeviceCls = GetGlobalRefClass("com/toio/ble/BleDeviceObj");
            var getInstanceMethod = AndroidJNI.GetStaticMethodID(bleManagerCls, "getInstance",
                "()Lcom/toio/ble/BleManagerObj;");

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

            if (uuids != null)
            {
                foreach (var uuid in uuids)
                {
                    this.argBuilder.Clear().Append(ArgJvalueBuilder.GenerateJvalue(uuid));
                    AndroidJNI.CallVoidMethod(scanner, addScanFilterMethod, this.argBuilder.Build());
                }
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
            var connectMethod = AndroidJNI.GetMethodID(this.bleManagerCls, "connect", "(Ljava/lang/String;)Lcom/toio/ble/BleDeviceObj;");
            this.argBuilder.Clear().Append(ArgJvalueBuilder.GenerateJvalue(addr));
            AndroidJNI.CallObjectMethod(this.javaBleManagerObj, connectMethod, this.argBuilder.Build());
        }

        public List<BleScannedDevice> GetScannedDevices()
        {
            return this.scannedDevices;
        }

        public List<BleCharacteristicData> GetCharacteristicDatas()
        {
            return this.readDatas;
        }

        public Dictionary<string,List<BleCharastericsKeyInfo>> GetCharastricKeyInfos()
        {
            return this.charastericsKeyInfos;
        }

        public void WriteCharacteristic(string addr, 
            string serviceUuid,
            string characteristicUUID, byte[] data, int length,
            bool withResponse)
        {
            var deviceObj = GetDeviceObj(addr);
            var writeMethod = AndroidJNI.GetMethodID(bleDeviceCls, "writeData",
                "(Ljava/lang/String;Ljava/lang/String;[BZ)V");
            this.argBuilder.Clear().
                Append(ArgJvalueBuilder.GenerateJvalue(serviceUuid)).
                Append(ArgJvalueBuilder.GenerateJvalue(characteristicUUID)).
                Append(ArgJvalueBuilder.GenerateJvalue(data, length)).
                Append(ArgJvalueBuilder.GenerateJvalue(withResponse));
            AndroidJNI.CallVoidMethod(deviceObj, writeMethod, this.argBuilder.Build());
        }

        public void SetNotificateFlag(string addr, string serviceUuid,
            string characteristicUUID, bool isEnable)
        {
            var deviceObj = GetDeviceObj(addr);
            var setNotificationMethod = AndroidJNI.GetMethodID(bleDeviceCls, "setNotification",
                "(Ljava/lang/String;Ljava/lang/String;Z)V");
            this.argBuilder.Clear().
                Append(ArgJvalueBuilder.GenerateJvalue(serviceUuid)).
                Append(ArgJvalueBuilder.GenerateJvalue(characteristicUUID)).
                Append(ArgJvalueBuilder.GenerateJvalue(isEnable));
            AndroidJNI.CallVoidMethod(deviceObj, setNotificationMethod, this.argBuilder.Build());
        }

        public void ReadCharacteristicRequest(string addr, string serviceUuid,
            string characteristicUUID )
        {
            var deviceObj = GetDeviceObj(addr);
            var readrequestMethod = AndroidJNI.GetMethodID(bleDeviceCls, "readRequest",
                "(Ljava/lang/String;Ljava/lang/String;)V");
            this.argBuilder.Clear().
                Append(ArgJvalueBuilder.GenerateJvalue(serviceUuid)).
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
                "getConnectedDevice", "(I)Lcom/toio/ble/BleDeviceObj;");
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
            if(device == IntPtr.Zero) { return; }
            AndroidJNI.PushLocalFrame(64);
            var blitMethod = AndroidJNI.GetMethodID(bleDeviceCls, "blit", "()V");
            var getAddrMethod = AndroidJNI.GetMethodID(bleDeviceCls, "getAddress", "()Ljava/lang/String;");
            var readNumMethod = AndroidJNI.GetMethodID(bleDeviceCls, "getReadNum", "()I");
            var getCharacteristicMethod = AndroidJNI.GetMethodID(bleDeviceCls, "getCharacteristicFromReadData", "(I)Ljava/lang/String;");
            var getServiceUuidMethod = AndroidJNI.GetMethodID(bleDeviceCls, "getServiceUuidFromReadData", "(I)Ljava/lang/String;");
            var isNotifyMethod = AndroidJNI.GetMethodID(bleDeviceCls, "isNotifyReadData", "(I)Z");
            var getReadDataMethod = AndroidJNI.GetMethodID(bleDeviceCls, "getDataFromReadData", "(I)[B");

            string addr = AndroidJNI.CallStringMethod(device, getAddrMethod, null);
            this.UpdateCharastricsKeys(addr, device);

            // read Charastrics Data
            AndroidJNI.CallVoidMethod(device, blitMethod, null);
            int readNum = AndroidJNI.CallIntMethod(device, readNumMethod,null);
            
            for ( int i = 0; i < readNum; ++i)
            {
                this.argBuilder.Clear().Append(ArgJvalueBuilder.GenerateJvalue(i));
                string serviceUuid = AndroidJNI.CallStringMethod(device, getServiceUuidMethod, argBuilder.Build());
                string charastristic = AndroidJNI.CallStringMethod(device,getCharacteristicMethod,argBuilder.Build() );
                bool isNotify = AndroidJNI.CallBooleanMethod(device, isNotifyMethod, argBuilder.Build());
                var dataObj = AndroidJNI.CallObjectMethod(device, getReadDataMethod, argBuilder.Build());
                var sbytes = AndroidJNI.FromSByteArray(dataObj);
                var characteristicData = new BleCharacteristicData(addr,serviceUuid, charastristic, sbytes, isNotify);
                this.readDatas.Add(characteristicData);
            }
            AndroidJNI.PopLocalFrame(IntPtr.Zero);
        }

        private void UpdateCharastricsKeys(string addr, IntPtr device)
        {
            List<BleCharastericsKeyInfo> list = null;
            if( this.charastericsKeyInfos.TryGetValue(addr ,out list))
            {
                return;
            }
            var blitCharaMethod = AndroidJNI.GetMethodID(this.bleDeviceCls, "blitChara", "()V");
            var getKeyNumMethod = AndroidJNI.GetMethodID(this.bleDeviceCls, "getKeysNum", "()I");
            var getServiceUuidFromKeysMethod = AndroidJNI.GetMethodID(this.bleDeviceCls,
                "getServiceUuidFromKeys", "(I)Ljava/lang/String;");
            var getCharastricUuidFromKeysMethod = AndroidJNI.GetMethodID(this.bleDeviceCls,
                "getCharastricUuidFromKeys", "(I)Ljava/lang/String;");
            // blit chara
            AndroidJNI.CallVoidMethod(device, blitCharaMethod, null);
            int num = AndroidJNI.CallIntMethod(device, getKeyNumMethod,null);
            if (num <= 0)
            {
                return;
            }
            list = new List<BleCharastericsKeyInfo>(num);
            for( int i =0;i < num; ++i)
            {
                this.argBuilder.Clear().Append(ArgJvalueBuilder.GenerateJvalue(i));
                string serviceUuid = AndroidJNI.CallStringMethod(device, getServiceUuidFromKeysMethod, argBuilder.Build());
                string charastricUuid = AndroidJNI.CallStringMethod(device, getCharastricUuidFromKeysMethod, argBuilder.Build());
                var keyInfo = new BleCharastericsKeyInfo(addr, serviceUuid, charastricUuid);
                list.Add(keyInfo);
            }

            this.charastericsKeyInfos.Add(addr, list);
        }
        public void Disconnect(string addr)
        {
            var disconnectMethod = AndroidJNI.GetMethodID(this.bleManagerCls,
                "disconnect", "(Ljava/lang/String;)V");
            this.argBuilder.Clear().Append(ArgJvalueBuilder.GenerateJvalue(addr));
            AndroidJNI.CallVoidMethod(this.javaBleManagerObj, 
                disconnectMethod, this.argBuilder.Build());
        }

        public void UpdateDisconnectedDevices()
        {
            var updateDisconnectedMethod = AndroidJNI.GetMethodID(this.bleManagerCls,
                "updateDisconnected", "()V");
            var getDisconnectedNumMethod = AndroidJNI.GetMethodID(this.bleManagerCls,
                "getDisconnectedDeviceNum", "()I");
            var getDisconnectedDeviceAddr = AndroidJNI.GetMethodID(this.bleManagerCls,
                "getDisconnectedDeviceAddr", "(I)Ljava/lang/String;");
            AndroidJNI.CallVoidMethod(this.javaBleManagerObj, updateDisconnectedMethod, null);
            disconnectedDevices.Clear();

            int num = AndroidJNI.CallIntMethod(this.javaBleManagerObj, getDisconnectedNumMethod, null);
            for(int i = 0; i < num; ++i)
            {
                this.argBuilder.Clear().Append( ArgJvalueBuilder.GenerateJvalue(i) );
                string addr = AndroidJNI.CallStringMethod(this.javaBleManagerObj,
                    getDisconnectedDeviceAddr,this.argBuilder.Build());
                this.disconnectedDevices.Add(addr);
            }

            foreach (var addr in disconnectedDevices)
                this.charastericsKeyInfos.Remove(addr);
        }
        public List<string> GetDisconnectedDevices()
        {
            return this.disconnectedDevices;
        }

        private System.IntPtr GetScanner()
        {
            var getScanner = AndroidJNI.GetMethodID(bleManagerCls, "getScanner", "()Lcom/toio/ble/BleScannerObj;");
            var scanner = AndroidJNI.CallObjectMethod(this.javaBleManagerObj, getScanner, null);
            return scanner;
        }
        private System.IntPtr GetDeviceObj(string addr)
        {
            var getDeviceMethod = AndroidJNI.GetMethodID(bleManagerCls, "getDeviceByAddr", "(Ljava/lang/String;)Lcom/toio/ble/BleDeviceObj;");
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
#endif