using System;

namespace toio
{
    public class BLEMobileCharacteristic : BLECharacteristicInterface
    {
        public string deviceAddress { get; private set; }
        public string serviceUUID { get; private set; }
        public string characteristicUUID { get; private set; }
        public TCallbackProvider<string, byte[]> readDataCallback { get; private set; }
        public TCallbackProvider<string, byte[], bool> writeDataCallback { get; private set; }


        public BLEMobileCharacteristic(string deviceAddress, string serviceUUID, string characteristicUUID)
        {
            this.deviceAddress = deviceAddress;
            this.serviceUUID = serviceUUID.ToUpper();
            this.characteristicUUID = characteristicUUID.ToUpper();
            this.readDataCallback = new TCallbackProvider<string, byte[]>();
            this.writeDataCallback = new TCallbackProvider<string, byte[], bool>();
        }

        public void ReadValue(Action<string, byte[]> action)
        {
            Ble.ReadCharacteristic(this.deviceAddress, this.serviceUUID, this.characteristicUUID, (address, characteristicUUID, bytes) =>
            {
                action(characteristicUUID, bytes);
            });
        }
        public void WriteValue(byte[] data, bool withResponse)
        {
            Ble.WriteCharacteristic(this.deviceAddress, this.serviceUUID, this.characteristicUUID, data, data.Length, withResponse, null);
        }
        public void StartNotifications(Action<byte[]> action)
        {
            Ble.SubscribeCharacteristic(this.deviceAddress, this.serviceUUID, this.characteristicUUID, (address, characteristicUUID, bytes) =>
            {
                action(bytes);
                this.readDataCallback.Notify(characteristicUUID, bytes);
            });
        }
        public void StopNotifications()
        {
            Ble.UnSubscribeCharacteristic(this.deviceAddress, this.serviceUUID, this.characteristicUUID, null);
        }
    }
}
