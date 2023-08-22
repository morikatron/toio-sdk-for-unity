using System;
using System.Collections.Generic;
using UnityEngine;
using toio;

public class Sample_Bluetooth : MonoBehaviour
{
    enum State
    {
        None,
        Scan,
        Connect,
        Subscribe,
        Control
    }

    State state = State.None;
    float _timeout = 0f;
    // BLE interfaces
    BLEDeviceInterface device;
    BLEPeripheralInterface peripheral;
    Dictionary<string, BLECharacteristicInterface> characteristicTable;
    // BLE device ids
    const string SERVICE_UUID = "10B20100-5B3B-4571-9508-CF3EFCD7BBAE";
    const string CHARACTERISTIC_ID = "10B20101-5B3B-4571-9508-CF3EFCD7BBAE";
    const string CHARACTERISTIC_MOTOR = "10B20102-5B3B-4571-9508-CF3EFCD7BBAE";

    void SetState(State newState, float timeout = 0.1f)
    {
        state = newState;
        this._timeout = timeout;
    }

    void Start()
    {
        Debug.Log("モバイル端末もしくはMacのみで動作します");

        // モバイル実装を注入
        BLEService.Instance.SetImplement(new BLEMobileService());
        // characteristicテーブルを作成
        characteristicTable = new Dictionary<string, BLECharacteristicInterface>();
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
                    // 端末のBLE機能変数を取得
                    BLEService.Instance.RequestDevice((_device) =>
                    {
                        device = _device;
                        string[] uuids = { SERVICE_UUID };
                        // 検索対象デバイスをtoioのサービスIDに絞って検索
                        device.Scan(uuids, true, (_peripheral) =>
                        {
                            // 一台発見したら接続モードへ変更
                            peripheral = _peripheral;
                            Debug.LogFormat("peripheral.device_name: {0}, peripheral.device_address: {1}", peripheral.device_name, peripheral.device_address);
                            device.StopScan();
                            SetState(State.Connect, 1f);
                        });
                    }
                    );
                }
                if (state == State.Connect)
                {
                    // peripheral(デバイス)に接続して全てのcharacteristic(機能)を取得
                    peripheral.Connect((chara) =>
                    {
                        characteristicTable[chara.characteristicUUID] = chara;
                        // 全てのcharacteristicへの接続を確認
                        if (characteristicTable.Count == 8)
                        {
                            SetState(State.Subscribe, 1f);
                        }
                    });
                }
                if (state == State.Subscribe)
                {
                    // 座標や角度を定期受信出来るように購読開始
                    characteristicTable[CHARACTERISTIC_ID].StartNotifications(action:Recv_Id);
                    SetState(State.Control, 1f);
                }
                if (state == State.Control)
                {
                    // コアキューブ通信仕様に沿ってパケット送信
                    // https://toio.github.io/toio-spec/docs/2.0.0/ble_motor#時間指定付きモーター制御
                    byte[] buff = { 2, 1, 1, 100, 2, 1, 70, 100 };
                    // モーターcharacteristicに対してパケット送信
                    characteristicTable[CHARACTERISTIC_MOTOR].WriteValue(data:buff, withResponse:false);
                    // 送信処理を50ミリ秒間隔で実行
                    SetState(State.Control, 0.05f);
                }
            }
        }
    }

    static int frameCnt = 0;
    void Recv_Id(byte[] data)
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
                Debug.LogFormat("x: {0}, y: {1}, angle: {2}", posX, posY, angle);
            }
        }
    }
}
