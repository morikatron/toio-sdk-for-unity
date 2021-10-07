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
    public string[] charaNames { get; private set; }
    private Dictionary<string, BLENetCharacteristic> characteristicTable = new Dictionary<string, BLENetCharacteristic>();

    public BLENetPeripheral(BLENetServer server, BLENetRemoteHost remoteHost, int localIndex, string device_address, string[] charaNames)
    {
        this.server = server;
        this.remoteHost = remoteHost;
        this.localIndex = localIndex;
        this.device_address = device_address;
        this.charaNames = charaNames;
        this.isConnected = true;

        foreach(var chara in this.charaNames)
        {
            var c = new BLENetCharacteristic(this, chara, BLENetProtocol.Characteristic2ShortID(chara));
            this.characteristicTable.Add(chara, c);
        }
    }

    /// <summary>
    /// peripheralに接続
    /// [peripheral:1 - characteristic:多]の関係なので、characteristicActionが複数回呼び出される
    /// </summary>
    public void Connect(Action<BLECharacteristicInterface> characteristicAction)
    {
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
    }

    /// <summary>
    /// 接続/切断コールバックを解除
    /// </summary>
    public void RemoveConnectionListener(string key)
    {
    }

    /// <summary>
    /// 接続/切断コールバックを呼び出し
    /// </summary>
    public void ConnectionNotify(BLEPeripheralInterface peri)
    {
    }

    /// <summary>
    /// 通信コールバック(接続時)
    /// </summary>
    private static void OnConnected(string device_address)
    {
    }

    /// <summary>
    /// 通信コールバック(切断時)
    /// </summary>
    private static void OnDisconnected(string device_address)
    {
    }
}
