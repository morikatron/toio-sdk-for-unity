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
            this.address = addr.ToLower();
            this.characteristicUUID = ch.ToLower();
            this.serviceUUID = service.ToLower();
        }

        public bool IsSameAddress(string addr)
        {
            return (this.address == addr.ToLower());
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

        public static void GetServices(HashSet<string> destServices , List<BleCharastericsKeyInfo> list)
        {
            destServices.Clear();
            foreach (var info in list)
            {
                string serviceUuid = info.serviceUUID;
                if (destServices.Contains(serviceUuid))
                {
                    destServices.Add(serviceUuid);
                }
            }
        }
        public static void GetInfoByService(List<BleCharastericsKeyInfo> dest,string service, List<BleCharastericsKeyInfo> src)
        {
            dest.Clear();
            foreach (var info in src)
            {
                if (service == info.serviceUUID)
                {
                    dest.Add(info);
                }
            }
        }

    }

}
#endif