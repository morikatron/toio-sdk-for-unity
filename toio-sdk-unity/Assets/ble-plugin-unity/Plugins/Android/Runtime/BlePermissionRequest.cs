
#if UNITY_ANDROID && !UNITY_EDITOR
#define UNITY_ANDROID_RUNTIME
#endif

#if UNITY_ANDROID_RUNTIME
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Android;

namespace toio.Android
{
    public class BlePermissionRequest
    {
        const string androidFineLocationPermission = "android.permission.ACCESS_FINE_LOCATION";
        const string androidCourceLocationPermission = "android.permission.ACCESS_COARSE_LOCATION";
        const string androidBluetoothPermission = "android.permission.BLUETOOTH";
        const string androidBluetoothAdminPermission = "android.permission.BLUETOOTH_ADMIN";
        const string androidBluetoothScanPermission = "android.permission.BLUETOOTH_SCAN";
        const string androidBluetoothConnectPermission = "android.permission.BLUETOOTH_CONNECT";
        Action initializedAction;
        Action<string> errorAction;

        public BlePermissionRequest(Action success,Action <string> error)
        {
            this.initializedAction = success;
            this.errorAction = error;
        }


        public IEnumerator Request()
        {
            
            var androidOsVersion = GetSdkInt();
            var willAskPermissions = new List<string>();
            if (androidOsVersion >= 31)
            {
                willAskPermissions.Add(androidBluetoothScanPermission);
                willAskPermissions.Add(androidBluetoothConnectPermission);
            }
            else if(androidOsVersion >= 29)
            {
                willAskPermissions.Add(androidBluetoothPermission);
                willAskPermissions.Add(androidBluetoothAdminPermission);
                willAskPermissions.Add(androidFineLocationPermission);
            }
            else
            {
                willAskPermissions.Add(androidBluetoothPermission);
                willAskPermissions.Add(androidBluetoothAdminPermission);
                willAskPermissions.Add(androidCourceLocationPermission);
            }

            
            foreach (var permission in willAskPermissions.Where(permission =>
                         !Permission.HasUserAuthorizedPermission(permission)))
            {
                Permission.RequestUserPermission(permission);
            }

            yield return null;
            var failedList = willAskPermissions.Where(permission => Permission.HasUserAuthorizedPermission(permission) == false).ToList();

            if (failedList.Count == 0)
            {
                this.initializedAction();
            }
            else
            {
                this.errorAction("No Permission:" + string.Join(",", failedList));
            }
        }

        public static int GetSdkInt()
        {
#if UNITY_ANDROID
            if (Application.isEditor) return -1;

            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<int>("SDK_INT");
            }
#else
            return -1;
#endif
        }
    }
}
#endif