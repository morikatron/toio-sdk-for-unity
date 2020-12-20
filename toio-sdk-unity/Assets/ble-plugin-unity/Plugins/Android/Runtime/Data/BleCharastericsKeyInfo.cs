#if UNITY_ANDROID && !UNITY_EDITOR
#define UNITY_ANDROID_RUNTIME
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;



#if UNITY_ANDROID_RUNTIME
namespace toio.Android.Data {
    public struct BleCharastericsKeyInfo : IComparer<BleCharastericsKeyInfo>
    {
        public string identifier;
        // ※ServiceUUIDありますが、判別に含んでません
        public string serviceUUID;
        public string characteristicUUID;

        public BleCharastericsKeyInfo(string id,string service, string ch)
        {
            this.identifier = id;
            this.characteristicUUID = ch;
            this.serviceUUID = service;
        }

        public BleCharastericsKeyInfo(string id,string ch)
        {
            this.identifier = id;
            this.characteristicUUID = ch;
            this.serviceUUID = null;
        }

        public int Compare(BleCharastericsKeyInfo x, BleCharastericsKeyInfo y)
        {
            int idParam = x.identifier.CompareTo(y.identifier);
            if (idParam != 0)
            {
                return idParam;
            }
            int chParam = x.characteristicUUID.CompareTo(y.characteristicUUID);
            return chParam;
        }
        public override int GetHashCode()
        {
            return identifier.GetHashCode() + characteristicUUID.GetHashCode();
        }
    }

}
#endif