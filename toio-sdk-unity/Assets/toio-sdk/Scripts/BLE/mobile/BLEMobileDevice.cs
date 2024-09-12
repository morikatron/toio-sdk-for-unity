using System;
using Cysharp.Threading.Tasks;

namespace toio
{
    public class BLEMobileDevice : BLEDeviceInterface
    {
        public void Scan(String[] serviceUUIDs, bool rssiOnly, Action<BLEPeripheralInterface[]> action)
        {
            Ble.StartScan(serviceUUIDs, (infos) =>
            {
                // device_address, device_name, rssi, bytes
                var peripherals = new BLEPeripheralInterface[infos.Length];
                for (int i = 0; i < infos.Length; i++)
                {
                    var (device_address, device_name, rssi, bytes) = infos[i];
                    peripherals[i] = new BLEMobilePeripheral(serviceUUIDs, device_address, device_name, rssi);
                }
                action(peripherals);
            });
        }

        public void StopScan()
        {
            Ble.StopScan();
        }

        public async UniTask Disconnect(Action action)
        {
            Ble.DisconnectAllPeripherals();
            Ble.Finalize();
            await UniTask.Delay(500);
            action.Invoke();
        }

        public async UniTask Enable(bool enable, Action action)
        {
            Ble.EnableBluetooth(enable);
#if !UNITY_EDITOR && UNITY_ANDROID
            await UniTask.Delay(1000);
#else
            await UniTask.Delay(1);
#endif
            action.Invoke();
        }
    }
}
