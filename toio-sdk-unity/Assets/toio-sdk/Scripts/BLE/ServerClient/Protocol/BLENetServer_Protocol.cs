using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using toio;



public partial class BLENetServer
{
    // コールバック
    //public TCallbackProvider<BLENetServer, BLENetRemoteHost> connectedCallback = new TCallbackProvider<BLENetServer, BLENetRemoteHost>();

    ///////////////////////////////////////////////////
    //      コールバック
    ///////////////////////////////////////////////////

    private TCallbackProvider<BLENetServer, BLEPeripheralInterface> joinPeripheralCallback = new TCallbackProvider<BLENetServer, BLEPeripheralInterface>();
    public void RegisterJoinPeripheralCallback(string key, Action<BLENetServer, BLEPeripheralInterface> callback)
    {
        this.joinPeripheralCallback.AddListener(key, callback);
    }

    private Dictionary<BLEPeripheralInterface, Dictionary<string, Action<byte[]>>> recvSubscribeCallbackTable = new Dictionary<BLEPeripheralInterface, Dictionary<string, Action<byte[]>>>();
    public void RegisterRecvSubscribeCallback(BLEPeripheralInterface keyPeripheral, string keyCharacteristicUUID, Action<byte[]> callback)
    {
        if (!this.recvSubscribeCallbackTable.ContainsKey(keyPeripheral)) { this.recvSubscribeCallbackTable[keyPeripheral] = new Dictionary<string, Action<byte[]>>(); }
        this.recvSubscribeCallbackTable[keyPeripheral][keyCharacteristicUUID] = callback;
    }

    ///////////////////////////////////////////////////
    //      プロトコル
    ///////////////////////////////////////////////////

    private Dictionary<byte, Action<BLENetRemoteHost, byte[]>> MakeProtocolTable()
    {
        var protocolTable = new Dictionary<byte, Action<BLENetRemoteHost, byte[]>>();
        protocolTable.Add(BLENetProtocol.C2S_JOIN, OnJoinPeripheral);
        protocolTable.Add(BLENetProtocol.C2S_JOINS, OnJoinPeripherals);
        protocolTable.Add(BLENetProtocol.C2S_SUBSCRIBE, OnRecvSubscribe);
        return protocolTable;
    }

    private void OnJoinPeripheral(BLENetRemoteHost remoteHost, byte[] data)
    {
        //Debug.Log("join");
        var (localIndex, deviceAddr, charaList) = BLENetProtocol.Decode_C2S_JOIN(data);
        var peripheral = new BLENetPeripheral(this, remoteHost, localIndex, deviceAddr, charaList);
        remoteHost.AddPeripheral(localIndex, peripheral);
        this.joinPeripheralCallback.Notify(this, peripheral);
    }

    private void OnJoinPeripherals(BLENetRemoteHost remoteHost, byte[] data)
    {
        var readdata = BLENetProtocol.Decode_C2S_JOINS(data);
        foreach(var (localIndex, deviceAddr, charaList) in readdata)
        {
            var peripheral = new BLENetPeripheral(this, remoteHost, localIndex, deviceAddr, charaList);
            remoteHost.AddPeripheral(localIndex, peripheral);
            //Debug.Log("join");
            this.joinPeripheralCallback.Notify(this, peripheral);
        }
    }

    private void OnRecvSubscribe(BLENetRemoteHost remoteHost, byte[] data)
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
}