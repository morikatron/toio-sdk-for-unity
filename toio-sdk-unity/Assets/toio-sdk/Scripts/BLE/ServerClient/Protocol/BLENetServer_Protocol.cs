using System;
using System.Collections.Generic;

namespace toio
{
    public partial class BLENetServer
    {
        // コールバック
        //public TCallbackProvider<BLENetServer, BLENetRemoteHost> connectedCallback = new TCallbackProvider<BLENetServer, BLENetRemoteHost>();

        ///////////////////////////////////////////////////
        //      コールバック
        ///////////////////////////////////////////////////

        private TCallbackProvider<BLENetRemoteHost, int, string, string, string[]> joinPeripheralCallback = new TCallbackProvider<BLENetRemoteHost, int, string, string, string[]>();
        public void RegisterJoinPeripheralCallback(string key, Action<BLENetRemoteHost, int, string, string, string[]> callback)
        {
            this.joinPeripheralCallback.AddListener(key, callback);
        }

        private Dictionary<BLEPeripheralInterface, Dictionary<string, Action<byte[]>>> recvSubscribeCallbackTable = new Dictionary<BLEPeripheralInterface, Dictionary<string, Action<byte[]>>>();
        public void RegisterRecvSubscribeCallback(BLEPeripheralInterface keyPeripheral, string keyCharacteristicUUID, Action<byte[]> callback)
        {
            if (!this.recvSubscribeCallbackTable.ContainsKey(keyPeripheral)) { this.recvSubscribeCallbackTable[keyPeripheral] = new Dictionary<string, Action<byte[]>>(); }
            this.recvSubscribeCallbackTable[keyPeripheral][keyCharacteristicUUID] = callback;
        }

        private Dictionary<BLEPeripheralInterface, Dictionary<string, Action<string, byte[]>>> recvReadValueCallbackTable = new Dictionary<BLEPeripheralInterface, Dictionary<string, Action<string, byte[]>>>();
        public void RegisterRecvReadCallback(BLEPeripheralInterface keyPeripheral, string keyCharacteristicUUID, Action<string, byte[]> callback)
        {
            if (!this.recvReadValueCallbackTable.ContainsKey(keyPeripheral)) { this.recvReadValueCallbackTable[keyPeripheral] = new Dictionary<string, Action<string, byte[]>>(); }
            this.recvReadValueCallbackTable[keyPeripheral][keyCharacteristicUUID] = callback;
        }

        ///////////////////////////////////////////////////
        //      プロトコル
        ///////////////////////////////////////////////////

        private Dictionary<byte, Action<BLENetRemoteHost, byte[]>> MakeProtocolTable()
        {
            var protocolTable = new Dictionary<byte, Action<BLENetRemoteHost, byte[]>>();
            protocolTable.Add(BLENetProtocol.C2S_JOIN, OnJoin_Peripheral);
            protocolTable.Add(BLENetProtocol.C2S_JOINS, OnJoin_Peripherals);
            protocolTable.Add(BLENetProtocol.C2S_SUBSCRIBE, OnRecv_Subscribe);
            protocolTable.Add(BLENetProtocol.C2S_READ_CALLBACK, OnRecv_ReadValue_Callback);
            return protocolTable;
        }

        private void OnJoin_Peripheral(BLENetRemoteHost remoteHost, byte[] data)
        {
            var (localIndex, deviceAddr, deviceName, charaList) = BLENetProtocol.Decode_C2S_JOIN(data);
            this.joinPeripheralCallback.Notify(remoteHost, localIndex, deviceAddr, deviceName, charaList);
        }

        private void OnJoin_Peripherals(BLENetRemoteHost remoteHost, byte[] data)
        {
            var readdata = BLENetProtocol.Decode_C2S_JOINS(data);
            foreach(var (localIndex, deviceAddr, deviceName, charaList) in readdata)
            {
                this.joinPeripheralCallback.Notify(remoteHost, localIndex, deviceAddr, deviceName, charaList);
            }
        }

        private void OnRecv_Subscribe(BLENetRemoteHost remoteHost, byte[] data)
        {
            var readdata = BLENetProtocol.Decode_C2S_SUBSCRIBE(data);
            foreach(var (localIndex, charaID, buffer) in readdata)
            {
                var peri = remoteHost.GetPeripheral(localIndex);
                if (null == peri) { return; }

                if (this.recvSubscribeCallbackTable.ContainsKey(peri) && this.recvSubscribeCallbackTable[peri].ContainsKey(charaID))
                {
                    this.recvSubscribeCallbackTable[peri][charaID].Invoke(buffer);
                }
            }
        }

        private void OnRecv_ReadValue_Callback(BLENetRemoteHost remoteHost, byte[] netdata)
        {
            var (localCubeIndex, charaID, bledata) = BLENetProtocol.Decode_C2S_READ_CALLBACK(netdata);
            var peri = remoteHost.GetPeripheral(localCubeIndex);
            this.recvReadValueCallbackTable[peri][charaID].Invoke(charaID, bledata);
        }
    }
}