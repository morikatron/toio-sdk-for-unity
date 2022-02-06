using System;
using System.Net;
using System.Collections.Generic;

namespace toio.ble.net
{
    public class BLENetRemoteHost
    {
        public string hostid { get; private set; }
        public IPEndPoint endpoint { get; private set; }
        public UDPClient udpClient { get; private set; }
        public bool IsConnected { get { return this.udpClient.IsConnected; } }
        // ペリフェラル
        private Dictionary<string, int> peripheralIndexTable = new Dictionary<string, int>();
        private Dictionary<int, BLENetPeripheral> peripheralTable = new Dictionary<int, BLENetPeripheral>();

        public BLENetRemoteHost(string hostid, IPEndPoint _endpoint, UDPClient client)
        {
            this.hostid = hostid;
            this.endpoint = _endpoint;
            this.udpClient = client;
        }

        public void RegisterPeripheral(string deviceName, int localIndex, BLENetPeripheral peripheral)
        {
            this.peripheralIndexTable[deviceName] = localIndex;
            this.peripheralTable[localIndex] = peripheral;
        }

        public BLENetPeripheral RemovePeripheral(string deviceName)
        {
            if (this.peripheralIndexTable.ContainsKey(deviceName))
            {
                var peripheral = this.peripheralTable[this.peripheralIndexTable[deviceName]];
                if (peripheral.device_name == deviceName)
                {
                    this.peripheralTable.Remove(this.peripheralIndexTable[deviceName]);
                    this.peripheralIndexTable.Remove(deviceName);
                    return peripheral;
                }
                return null;
            }
            return null;
        }

        public BLENetPeripheral GetPeripheral(string deviceName)
        {
            if (this.peripheralIndexTable.ContainsKey(deviceName) && this.peripheralTable.ContainsKey(this.peripheralIndexTable[deviceName]))
                return this.peripheralTable[this.peripheralIndexTable[deviceName]];
            return null;
        }

        public BLENetPeripheral GetPeripheral(int localIndex)
        {
            if (this.peripheralTable.ContainsKey(localIndex))
                return this.peripheralTable[localIndex];
            return null;
        }

        public int GetLocalIndex(string deviceName)
        {
            if (this.peripheralIndexTable.ContainsKey(deviceName))
                return this.peripheralIndexTable[deviceName];
            return -1;
        }

        public void OnDisconnected()
        {

        }
    }
}