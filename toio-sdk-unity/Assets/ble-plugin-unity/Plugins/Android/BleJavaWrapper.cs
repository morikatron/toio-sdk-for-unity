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

namespace toio.Android
{
    public class BleJavaWrapper
    {
        private IntPtr bleManagerCls;
        private IntPtr bleScannerCls;
        private IntPtr bleDeviceCls;
        private IntPtr javaBleManagerObj;

        private ArgJvalueBuilder argBuilder;

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
            AndroidJNI.CallVoidMethod(javaBleManagerObj, initMethod, argBuilder.Build() );

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

            AndroidJNI.CallVoidMethod(scanner, scanMethod, this.argBuilder.Build() );
            AndroidJNI.PopLocalFrame(IntPtr.Zero);
        }

        private System.IntPtr GetScanner()
        {
            var getScanner = AndroidJNI.GetMethodID(bleManagerCls, "getScanner", "()Lcom/utj/ble/BleScannerObj;");
            var scanner = AndroidJNI.CallObjectMethod(this.javaBleManagerObj, getScanner, null);
            return scanner;
        }

        public void UpdateScanResult()
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
            for( int i = 0; i < num; ++i)
            {
                argBuilder.Clear().Append(ArgJvalueBuilder.GenerateJvalue(i));
                string addr = AndroidJNI.CallStringMethod(scanner, getDeviceAddrMethod, argBuilder.Build());
                argBuilder.Clear().Append(ArgJvalueBuilder.GenerateJvalue(addr));
                string name = AndroidJNI.CallStringMethod(scanner, getDeviceNameByAddrMethod, argBuilder.Build());
                int rssi = AndroidJNI.CallIntMethod(scanner, getRssiByAddrMethod, argBuilder.Build());
                // debug
                Debug.Log(i + "::"+ addr + " " + name  +"::::" + rssi);
            }
            AndroidJNI.PopLocalFrame(IntPtr.Zero);
        }

        public void ConnectRequest(string addr)
        {
            var connectMethod = AndroidJNI.GetMethodID(this.bleManagerCls, "connect", "(Ljava/lang/String;)Lcom/utj/ble/BleDeviceObj;");
            this.argBuilder.Clear().Append(ArgJvalueBuilder.GenerateJvalue(addr));
            AndroidJNI.CallObjectMethod(this.javaBleManagerObj, connectMethod, this.argBuilder.Build());
        }
    }
}
