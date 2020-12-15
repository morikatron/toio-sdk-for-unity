using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace toio.Android
{
    public class BleScannedDevice
    {
        public string address { get; private set; }
        public string name { get; private set; }
        public int rssi { get; private set; }

        public BleScannedDevice(string addr,string n)
        {
            this.address = addr;
            this.name = n;
        }
        public void setRssi(int r)
        {
            this.rssi = r;
        }
    }

}