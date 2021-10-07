using System;
using toio;

public class BLENetCharacteristic : BLECharacteristicInterface
{
    public string deviceAddress { get; private set; }
    public string serviceUUID { get; private set; }
    public string characteristicUUID { get; private set; }
    public byte characteristicShortID { get; private set; }
    public TCallbackProvider<string, byte[]> readDataCallback { get; private set; }
    public TCallbackProvider<string, byte[], bool> writeDataCallback { get; private set; }

    public BLENetPeripheral peripheral { get; private set; }


    public BLENetCharacteristic(BLENetPeripheral peripheral, string characteristicUUID, byte characteristicShortID)
    {
        this.peripheral = peripheral;
        this.characteristicUUID = characteristicUUID;
        this.characteristicShortID = characteristicShortID;
        this.readDataCallback = new TCallbackProvider<string, byte[]>();
        this.writeDataCallback = new TCallbackProvider<string, byte[], bool>();
    }

    public void ReadValue(Action<string, byte[]> action)
    {
    }

    public void WriteValue(byte[] data, bool withResponse)
    {
        var buff = BLENetProtocol.Encode_S2C_WRITE(this.peripheral.localIndex, this.characteristicShortID, data, withResponse);
        this.peripheral.remoteHost.udpClient.SendData(buff);
    }

    public void StartNotifications(Action<byte[]> action)
    {
        this.peripheral.server.RegisterRecvSubscribeCallback(this.peripheral, this.characteristicUUID, (data)=> { action?.Invoke(data); this.readDataCallback.Notify(this.characteristicUUID, data); });
    }
    public void StopNotifications()
    {
    }
}
