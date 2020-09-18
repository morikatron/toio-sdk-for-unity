using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace toio
{
    public class BLEMobileService : BLEServiceInterface
    {
        public void RequestDevice(Action<BLEDeviceInterface> action)
        {
            Ble.Initialize(() =>
            {
                action(new BLEMobileDevice());
            }, (error) =>
            {
#if !RELEASE
                Debug.LogErrorFormat("[BLEMobileService.requestDevice]error = {0}", error);
#endif
            });
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

        public void DisconnectAll()
        {
            Ble.DisconnectAllPeripherals();
            Ble.Finalize();
        }
    }
}
