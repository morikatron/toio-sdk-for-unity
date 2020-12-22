
#if UNITY_ANDROID && !UNITY_EDITOR
#define UNITY_ANDROID_RUNTIME
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



#if UNITY_ANDROID_RUNTIME
namespace toio.Android.Data {
    public struct BleDeviceEvent 
    {
        public Action<string> connectedAct;
        public Action<string, string> discoveredServiceAct;
        public Action<string, string, string> discoveredCharacteristicAct;
        public Action<string> disconnectedAct;

        public bool callDiscoverEvent;

        public void InitFlags()
        {
            callDiscoverEvent = false;
        }
    }

}
#endif