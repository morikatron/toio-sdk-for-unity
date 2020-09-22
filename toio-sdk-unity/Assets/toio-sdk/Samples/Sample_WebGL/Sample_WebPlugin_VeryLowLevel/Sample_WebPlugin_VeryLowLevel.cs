using System;
using System.Collections.Generic;
using UnityEngine;
using toio;

public class Sample_WebPlugin_VeryLowLevel : MonoBehaviour
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
    // control
    int deviceID;
    int serverID;
    int serviceID;
    Dictionary<string, int> characteristicIDTable = new Dictionary<string, int>();
    // BLE device ids(lower)
    const string SERVICE_UUID = "10b20100-5b3b-4571-9508-cf3efcd7bbae";
    const string CHARACTERISTIC_ID = "10b20101-5b3b-4571-9508-cf3efcd7bbae";
    const string CHARACTERISTIC_MOTOR = "10b20102-5b3b-4571-9508-cf3efcd7bbae";

    void SetState(State newState, float timeout = 0.1f)
    {
        state = newState;
        _timeout = timeout;
    }

    void Start()
    {
        Debug.Log("ブラウザのみで動作します");
    }

    public void OnDownConnectButton()
    {
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
                    // [呼び出し]スキャン
                    WebBluetoothScript.Instance.RequestDevice(SERVICE_UUID,
                        // [コールバック]スキャン成功
                        (_deviceID, _deviceUUID, _deviceName) =>
                        {
                            Debug.LogFormat("[RequestDevice]succeeded. deviceID: {0}, deviceUUID: {1}, deviceName: {2}", _deviceID, _deviceUUID, _deviceName);
                            deviceID = _deviceID;
                            // 接続ステートへ移行
                            SetState(State.Connect);
                        },
                        // [コールバック]スキャン失敗
                        (errorMsg) =>
                        {
                            Debug.LogFormat("[RequestDevice]failed. errorMsg: {0}", errorMsg);
                        }
                    );
                }
                if (state == State.Connect)
                {
                    // [呼び出し]接続
                    WebBluetoothScript.Instance.Connect(deviceID, SERVICE_UUID,
                        // [コールバック]接続成功
                        (_serverID, _serviceID, _serviceUUID) =>
                        {
                            Debug.LogFormat("[Connect]connected. serverID: {0}, serviceID: {1}, serviceUUID: {2}", _serverID, _serviceID, _serviceUUID);
                            serverID = _serverID;
                            serviceID = _serviceID;
                            // [呼び出し]全てのCharacteristicを取得
                            WebBluetoothScript.Instance.GetCharacteristics(serviceID,
                                // [コールバック]各Characteristicを取得
                                (_characteristicID, _characteristicUUID) =>
                                {
                                    Debug.LogFormat("[GetCharacteristics]characteristicID: {0}, characteristicUUID: {1}", _characteristicID, _characteristicUUID);
                                    characteristicIDTable[_characteristicUUID] = _characteristicID;
                                    // 全てのcharacteristicへの接続を確認
                                    if (characteristicIDTable.Count == 8)
                                    {
                                        // 情報購読ステートへ移行
                                        SetState(State.Subscribe);
                                    }
                                }
                            );
                        },
                        // [コールバック]切断
                        (_deviceID) =>
                        {
                            Debug.LogFormat("[Connect]disconnected. deviceID: {0}", _deviceID);
                        }
                    );
                }
                if (state == State.Subscribe)
                {
                    // 座標や角度を定期受信出来るように購読開始
                    WebBluetoothScript.Instance.StartNotifications(characteristicID:characteristicIDTable[CHARACTERISTIC_ID], callback:Recv_Id);
                    SetState(State.Control, 1f);
                }
                if (state == State.Control)
                {
                    // コアキューブ通信仕様に沿ってパケット送信
                    // https://toio.github.io/toio-spec/docs/2.0.0/ble_motor#時間指定付きモーター制御
                    byte[] buff = { 2, 1, 1, 100, 2, 1, 70, 100 };
                    // モーターcharacteristicに対してパケット送信
                    WebBluetoothScript.Instance.WriteValue(characteristicID:characteristicIDTable[CHARACTERISTIC_MOTOR], data:buff);
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