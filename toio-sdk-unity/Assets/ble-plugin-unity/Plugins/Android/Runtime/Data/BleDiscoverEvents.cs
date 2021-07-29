
#if UNITY_ANDROID && !UNITY_EDITOR
#define UNITY_ANDROID_RUNTIME
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



#if UNITY_ANDROID_RUNTIME
namespace toio.Android.Data {
    public class BleDiscoverEvents
    {
        public Action<string> connectedAct;
        public Action<string, string> discoveredServiceAct;
        public Action<string, string, string> discoveredCharacteristicAct;
        public Action<string> disconnectedAct;

        public bool callDiscoverEvent = false;

        public BleDiscoverEvents(
            Action<string> connectedAct,
            Action<string, string> discoveredServiceAct,
            Action<string, string, string> discoveredCharacteristicAct,
            Action<string> disconnectedAct
        ){
            this.connectedAct = connectedAct;
            this.discoveredServiceAct = discoveredServiceAct;
            this.discoveredCharacteristicAct = discoveredCharacteristicAct;
            this.disconnectedAct = disconnectedAct;
        }
    }

}
#endif