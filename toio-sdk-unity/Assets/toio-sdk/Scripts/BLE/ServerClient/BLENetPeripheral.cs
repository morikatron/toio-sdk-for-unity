using System;
using System.Collections.Generic;
using UnityEngine;
using toio;

public class BLENetPeripheral : BLEPeripheralInterface
{
    public string[] serviceUUIDs { get; private set; }
    public string device_address { get; private set; }
    public string device_name { get; private set; }
    public float rssi { get; private set; }
    public bool isConnected { get; private set; }

    public BLENetServer server { get; private set; }
    public BLENetRemoteHost remoteHost { get; private set; }
    public int localIndex { get; private set; }
    private Dictionary<string, BLENetCharacteristic> characteristicTable = new Dictionary<string, BLENetCharacteristic>();
    private TCallbackProvider<BLEPeripheralInterface> callback = new TCallbackProvider<BLEPeripheralInterface>();

    public BLENetPeripheral(string[] serviceUUIDs, string device_address, string device_name, BLENetServer server, BLENetRemoteHost remoteHost, int localIndex, string[] charaNames)
    {
        this.serviceUUIDs = serviceUUIDs;
        this.device_address = device_address.ToUpper();
        this.device_name = device_name;
        this.rssi = 1.0f;
        this.isConnected = true;

        this.server = server;
        this.remoteHost = remoteHost;
        this.localIndex = localIndex;

        foreach(var chara in charaNames)
        {
            var c = new BLENetCharacteristic(device_address, serviceUUIDs[0], chara, BLENetProtocol.Characteristic2ShortID(chara), this);
            this.characteristicTable.Add(chara, c);
        }
    }

    /// <summary>
    /// peripheralに接続
    /// [peripheral:1 - characteristic:多]の関係なので、characteristicActionが複数回呼び出される
    /// </summary>
    public void Connect(Action<BLECharacteristicInterface> characteristicAction)
    {
        this.OnConnected(this.device_address);
        foreach(var chara in this.characteristicTable)
        {
            characteristicAction(chara.Value);
        }
    }

    /// <summary>
    /// peripheralを切断
    /// </summary>
    public void Disconnect()
    {

    }

    /// <summary>
    /// 接続/切断コールバックを登録
    /// </summary>
    public void AddConnectionListener(string key, Action<BLEPeripheralInterface> action)
    {
        this.callback.AddListener(key, action);
    }

    /// <summary>
    /// 接続/切断コールバックを解除
    /// </summary>
    public void RemoveConnectionListener(string key)
    {
        this.callback.RemoveListener(key);
    }

    /// <summary>
    /// 接続/切断コールバックを呼び出し
    /// </summary>
    public void ConnectionNotify(BLEPeripheralInterface peri)
    {
        this.callback.Notify(peri);
    }

    /// <summary>
    /// 通信コールバック(接続時)
    /// </summary>
    private void OnConnected(string device_address)
    {
        if (!this.isConnected)
        {
            this.isConnected = true;
            this.ConnectionNotify(this);
        }
    }

    /// <summary>
    /// 通信コールバック(切断時)
    /// </summary>
    private void OnDisconnected(string device_address)
    {
        if (this.isConnected)
        {
            this.isConnected = false;
            this.ConnectionNotify(this);
        }
    }
}
