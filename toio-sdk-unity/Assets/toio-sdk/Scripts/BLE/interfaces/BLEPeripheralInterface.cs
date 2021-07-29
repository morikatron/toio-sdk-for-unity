using System;
using System.Collections.Generic;

namespace toio
{
    public interface BLEPeripheralInterface
    {
        string[] serviceUUIDs { get; }
        string device_address { get; }
        string device_name { get; }
        float rssi { get; }
        bool isConnected { get; }

        /// peripheralに接続
        /// [peripheral:1 - characteristic:多]の関係なので、characteristicActionが複数回呼び出される
        void Connect(Action<BLECharacteristicInterface> characteristicAction);

        /// <summary>
        /// peripheralを切断
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 接続/切断コールバックを登録
        /// </summary>
        void AddConnectionListener(string key, Action<BLEPeripheralInterface> action);

        /// <summary>
        /// 接続/切断コールバックを解除
        /// </summary>
        void RemoveConnectionListener(string key);

        /// <summary>
        /// 接続/切断コールバックを呼び出し
        /// 通信実装を追加した場合、明示的にこの関数を呼び出さなければコールバックは呼び出されない
        /// </summary>
        void ConnectionNotify(BLEPeripheralInterface peri);
    }
}