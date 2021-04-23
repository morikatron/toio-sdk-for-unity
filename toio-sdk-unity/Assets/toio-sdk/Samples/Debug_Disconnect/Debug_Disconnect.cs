using System;
using System.Collections.Generic;
using UnityEngine;
using toio;

public class Debug_Disconnect : MonoBehaviour
{
    enum State
    {
        None,
        Scan,
        Connect,
        Subscribe,
        Control,
        Disconnect
    }

    State state = State.None;
    float _timeout = 0f;
    // BLE device ids
    const string SERVICE_UUID = "10B20100-5B3B-4571-9508-CF3EFCD7BBAE";
    const string CHARACTERISTIC_ID = "10B20101-5B3B-4571-9508-CF3EFCD7BBAE";
    const string CHARACTERISTIC_MOTOR = "10B20102-5B3B-4571-9508-CF3EFCD7BBAE";

    string device_address;
    string serviceUUID;
    List<string> characteristicIDs = new List<string>();
    int controlCnt = 0;

    void SetState(State newState, float timeout = 0.1f)
    {
        state = newState;
        this._timeout = timeout;
    }

    void Start()
    {
        Debug.Log("モバイル端末のみで動作します");
        SetState(State.Scan);
    }

    void Update()
    {
        if (_timeout > 0f)
        {
            _timeout -= Time.deltaTime;
            if (_timeout <= 0f)
            {
                _timeout = 0f;
                if (state == State.Scan)
                {
                    Debug.Log("[State.Scan] Start");
                    /*
                    Ble.Initialize(() => {
                        string[] uuids = { SERVICE_UUID };
                        // 端末のBLE機能変数を取得
                        Debug.Log("[StartScan-Start]");
                        Ble.StartScan(uuids, (device_address, device_name, rssi, bytes) =>
                        {
                            // 一台発見したら接続モードへ変更
                            this.device_address = device_address;
                            Debug.LogFormat("[StartScan-Received] device_address: {0}, device_name: {1}, rssi: {2}, bytes: {3}", device_address, device_name, rssi, bytes);
                            Ble.StopScan();
                            SetState(State.Connect, 1f);
                            Debug.Log("[State.Scan] SetState(State.Connect)");
                        });
                    });
                    */
                    Debug.Log("[State.Scan] End");
                }
                else if (state == State.Connect)
                {
                    Debug.Log("[State.Connect] Start");
                    Debug.LogFormat("[ConnectToPeripheral-Start]");
                    // peripheral(デバイス)に接続して全てのcharacteristic(機能)を取得
                    Ble.ConnectToPeripheral(this.device_address, (device_address) =>
                    {
                        Debug.LogFormat("[ConnectToPeripheral-Received]connectedPeripheral. device_address: {0}", device_address);
                    },
                    null, (address, serviceUUID, characteristicUUID) =>
                    {
                        Debug.LogFormat("[ConnectToPeripheral-Received]connectedCharacteristic. address: {0}, serviceUUID: {1}, characteristicUUID: {2}, charaIds_Cnt: {3}", address, serviceUUID, characteristicUUID, characteristicIDs.Count);
                        this.serviceUUID = serviceUUID;
                        characteristicIDs.Add(characteristicUUID);
                        // 全てのcharacteristicへの接続を確認
                        if (characteristicIDs.Count == 8)
                        {
                            SetState(State.Subscribe, 1f);
                            Debug.Log("[State.Connect] SetState(State.Subscribe)");
                        }
                    }, (device_address) =>
                    {
                        Debug.LogFormat("[ConnectToPeripheral-Received]disconnectedPeripheral. device_address: {0}", device_address);
                    });
                    Debug.Log("[State.Connect] End");
                }
                else if (state == State.Subscribe)
                {
                    Debug.Log("[State.Subscribe] Start");
                    // 座標や角度を定期受信出来るように購読開始
                    Ble.SubscribeCharacteristic(this.device_address, this.serviceUUID, CHARACTERISTIC_ID, Recv_Id);
                    SetState(State.Control, 1f);
                    Debug.Log("[State.Subscribe] SetState(State.Control)");
                    Debug.Log("[State.Subscribe] End");
                }
                else if (state == State.Control)
                {
                    Debug.Log("[State.Control] Start");
                    this.controlCnt++;
                    if (15 < this.controlCnt)
                    {
                        SetState(State.Disconnect, 0.5f);
                        Debug.Log("[State.Control] SetState(State.Disconnect)");
                    }
                    else
                    {
                        Debug.Log("[WriteCharacteristic]");
                        // コアキューブ通信仕様に沿ってパケット送信
                        // https://toio.github.io/toio-spec/docs/2.0.0/ble_motor#時間指定付きモーター制御
                        byte[] buff = { 2, 1, 1, 100, 2, 1, 70, 100 };
                        // モーターcharacteristicに対してパケット送信
                        Ble.WriteCharacteristic(this.device_address, this.serviceUUID, CHARACTERISTIC_MOTOR, buff, buff.Length, false, null);
                        // 送信処理を200ミリ秒間隔で実行
                        SetState(State.Control, 0.2f);
                        Debug.Log("[State.Control] SetState(State.Control)");
                    }
                    Debug.Log("[State.Control] End");
                }
                else if (state == State.Disconnect)
                {
                    Debug.Log("[State.Disconnect] Start");
                    Debug.Log("[DisconnectPeripheral-Start]");
                    Ble.DisconnectPeripheral(this.device_address, (device_address)=> {
                        Debug.LogFormat("[DisconnectPeripheral-Received]device_address: {0}", device_address);
                        Debug.Log("切断が完了したため、再びスキャンを行います");
                        SetState(State.Scan);
                        Debug.Log("[State.Disconnect] SetState(State.Scan)");
                    });
                    Debug.Log("[State.Disconnect] End");
                }
            }
        }
    }

    static int frameCnt = 0;
    void Recv_Id(string address, string characteristicUUID, byte[] data)
    {
        int type = data[0];

        // position id
        if (1 == type)
        {
            var posX = BitConverter.ToInt16(data, 1);
            var posY = BitConverter.ToInt16(data, 3);
            var angle = BitConverter.ToInt16(data, 5);
            var sensorPosX = BitConverter.ToInt16(data, 7);
            var sensorPosY = BitConverter.ToInt16(data, 9);
            var sensorAngle = BitConverter.ToInt16(data, 11);
            // 毎フレーム表示すると困るので30フレーム間隔で表示
            if (30 < frameCnt++)
            {
                frameCnt = 0;
                Debug.LogFormat("[Recv_Id] x: {0}, y: {1}, angle: {2}", posX, posY, angle);
            }
        }
    }
}
