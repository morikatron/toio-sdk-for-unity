

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
namespace toio.Windows.Data {
    internal class BleDiscoverEvents
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