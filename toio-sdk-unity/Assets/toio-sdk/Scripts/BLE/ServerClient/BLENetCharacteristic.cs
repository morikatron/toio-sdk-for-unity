using System;
using UnityEngine;

namespace toio
{
    public class BLENetCharacteristic : BLECharacteristicInterface
    {
        public string deviceAddress { get; private set; }
        public string serviceUUID { get; private set; }
        public string characteristicUUID { get; private set; }
        public byte characteristicShortID { get; private set; }
        public TCallbackProvider<string, byte[], bool> writeDataCallback { get; private set; }
        public TCallbackProvider<string, byte[]> readDataCallback { get; private set; }
        public TCallbackProvider<string, byte[]> notifiedCallback { get; private set; }

        public BLENetPeripheral peripheral { get; private set; }


        public BLENetCharacteristic(string deviceAddress, string serviceUUID, string characteristicUUID, byte characteristicShortID, BLENetPeripheral peripheral)
        {
            this.deviceAddress = deviceAddress;
            this.serviceUUID = serviceUUID;
            this.characteristicUUID = characteristicUUID;
            this.characteristicShortID = characteristicShortID;
            this.writeDataCallback = new TCallbackProvider<string, byte[], bool>();
            this.readDataCallback = new TCallbackProvider<string, byte[]>();
            this.notifiedCallback = new TCallbackProvider<string, byte[]>();
            this.peripheral = peripheral;
        }

        public void ReadValue(Action<string, byte[]> action)
        {
            // ReadValue命令をクライアントへ送信
            var buff = BLENetProtocol.Encode_S2C_READ(this.peripheral.localIndex, this.characteristicUUID);
            this.peripheral.remoteHost.udpClient.SendData(buff);
            // 返信データを受信した場合のコールバック設定
            this.peripheral.server.RegisterRecvReadCallback(this.peripheral, this.characteristicUUID, (charaID, data)=>
            {
                action?.Invoke(charaID, data);
                this.readDataCallback.Notify(charaID, data);
            });
        }

        public void WriteValue(byte[] data, bool withResponse)
        {
            var buff = BLENetProtocol.Encode_S2C_WRITE(this.peripheral.localIndex, this.characteristicShortID, data, withResponse);
            this.peripheral.remoteHost.udpClient.SendData(buff);
            this.writeDataCallback.Notify(this.characteristicUUID, data, withResponse);
        }

        public void StartNotifications(Action<byte[]> action)
        {
            // リアルキューブに講読命令は送らず、クライアントから勝手に送られてくるデータを受け取る
            this.peripheral.server.RegisterRecvSubscribeCallback(this.peripheral, this.characteristicUUID, (data)=> { action?.Invoke(data); this.notifiedCallback.Notify(this.characteristicUUID, data); });
        }
        public void StopNotifications()
        {
        }
    }
}