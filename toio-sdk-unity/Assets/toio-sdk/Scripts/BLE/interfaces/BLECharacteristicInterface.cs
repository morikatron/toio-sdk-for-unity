using System;

namespace toio
{
    public interface BLECharacteristicInterface
    {
        string deviceAddress { get; }
        string serviceUUID { get; }
        string characteristicUUID { get; }

        void ReadValue(Action<string, byte[]> action);
        void WriteValue(byte[] data, bool withResponse);
        void StartNotifications(Action<byte[]> action);
        void StopNotifications();
    }
}