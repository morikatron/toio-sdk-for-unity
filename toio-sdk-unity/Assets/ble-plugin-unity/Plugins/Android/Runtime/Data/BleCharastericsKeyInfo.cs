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
        public string address;
        public string serviceUUID;
        public string characteristicUUID;

        public BleCharastericsKeyInfo(string addr,string service, string ch)
        {
            this.address = addr;
            this.characteristicUUID = ch;
            this.serviceUUID = service;
        }
        

        public int Compare(BleCharastericsKeyInfo x, BleCharastericsKeyInfo y)
        {
            int idParam = x.address.CompareTo(y.address);
            if (idParam != 0)
            {
                return idParam;
            }
            int serviceParam = x.serviceUUID.CompareTo(y.serviceUUID);
            if (serviceParam != 0)
            {
                return serviceParam;
            }
            int chParam = x.characteristicUUID.CompareTo(y.characteristicUUID);
            return chParam;
        }
        public override int GetHashCode()
        {
            return address.GetHashCode() + serviceUUID.GetHashCode() + characteristicUUID.GetHashCode();
        }

        public static HashSet<string> GetServices(List<BleCharastericsKeyInfo> list)
        {
            HashSet<string> services = new HashSet<string>();
            foreach(var info in list)
            {
                string serviceUuid = info.serviceUUID;
                if (services.Contains(serviceUuid))
                {
                    services.Add(serviceUuid);
                }
            }
            return services;
        }
    }

}
#endif