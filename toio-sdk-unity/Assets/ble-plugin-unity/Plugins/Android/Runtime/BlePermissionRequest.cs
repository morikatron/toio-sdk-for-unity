
#if UNITY_ANDROID && !UNITY_EDITOR
#define UNITY_ANDROID_RUNTIME
#endif


#if UNITY_ANDROID_RUNTIME
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Android;

namespace toio.Android
{
    public class BlePermissionRequest
    {
        Action initializedAction;
        Action<string> errorAction;

        public BlePermissionRequest(Action success,Action <string> error)
        {
            this.initializedAction = success;
            this.errorAction = error;
        }


        public IEnumerator Request()
        {
            string permission = Permission.FineLocation;
            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                Permission.RequestUserPermission(permission);
            }
            yield return null;
            if (Permission.HasUserAuthorizedPermission(permission))
            {
                this.initializedAction();
            }
            else
            {
                this.errorAction("No Permission:" + permission);
            }
        }


    }
}
#endif